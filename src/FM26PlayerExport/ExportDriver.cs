using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using FM26PlayerExport.Handlers;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace FM26PlayerExport;

/// <summary>
/// Plain (non-MonoBehaviour) driver for the export. On arm64 macOS we cannot inject a
/// MonoBehaviour via Il2CppInterop.ClassInjector (its function finders are x86-only), so instead
/// of running on Unity's Update loop this class is ticked from BepInEx's main-thread pump
/// (IL2CPPChainloader.MainThreadTick, driven by the il2cpp_runtime_invoke detour). All work here
/// only READS existing il2cpp objects, which interop supports on arm64.
/// </summary>
public sealed class ExportDriver
{
	private IExportHandler _currentHandler;
	private readonly List<IExportHandler> _availableHandlers = new();
	private bool _started;

	private void EnsureStarted()
	{
		if (_started) return;
		_started = true;
		_availableHandlers.Add(new Fm27GridTableHandler());
		_availableHandlers.Add(new StaffExportHandler());
		_availableHandlers.Add(new PlayerExportHandler());
		Plugin.Log.LogInfo("[FM26Export] Driver pronto (main-thread pump). [F9 / Ctrl+P] = exportar.");
	}

	public void Tick()
	{
		try
		{
			EnsureStarted();
			if (Keyboard.current == null)
				return;

			if (_currentHandler == null)
			{
				var ctrl = ((ButtonControl)Keyboard.current.leftCtrlKey).isPressed
						   || ((ButtonControl)Keyboard.current.rightCtrlKey).isPressed;
				var ctrlP = ctrl && ((ButtonControl)Keyboard.current.pKey).wasPressedThisFrame;
				var f9 = ((ButtonControl)Keyboard.current.f9Key).wasPressedThisFrame;
				var ctrlT = ((ButtonControl)Keyboard.current.f10Key).wasPressedThisFrame;
				if (ctrlT)
				{
					Plugin.Log.LogInfo("[FM26Export] DIAG: дамп дерева UI по F10");
					DumpAllRoots();
				}
				if (ctrlP || f9)
				{
					Plugin.Log.LogInfo("[FM26Export] Iniciando exportação via atalho: " + (f9 ? "F9" : "Ctrl+P"));
					StartCapture();
				}
			}
			else if (_currentHandler.CaptureStep())
			{
				_currentHandler.FinishCapture();
				try { _currentHandler.Cleanup(); } catch { }
				_currentHandler = null;
				try { GC.Collect(); GC.WaitForPendingFinalizers(); } catch { }
			}
		}
		catch (Exception)
		{
		}
	}



	private static System.Collections.Generic.List<VisualElement> GetAllRoots()
	{
		var roots = new System.Collections.Generic.List<VisualElement>();
		try
		{
			var dm = global::SI.UI.DocumentManager.Instance;
			if (dm != null)
			{
				var main = dm.MainRoot;
				if (main != null) roots.Add(main);
				try
				{
					var list = new Il2CppSystem.Collections.Generic.List<VisualElement>();
					dm.GetRoots(list);
					foreach (var r in list)
						if (r != null && !roots.Contains(r)) roots.Add(r);
				}
				catch (System.Exception e) { Plugin.Log.LogWarning("[FM26Export] GetRoots: " + e.Message); }
			}
		}
		catch (System.Exception e) { Plugin.Log.LogWarning("[FM26Export] DocumentManager: " + e.Message); }
		if (roots.Count == 0)
		{
			try
			{
				Il2CppArrayBase<UIDocument> docs = Object.FindObjectsOfType<UIDocument>();
				if (docs != null)
					foreach (var d in docs)
						if (d != null && d.rootVisualElement != null) roots.Add(d.rootVisualElement);
			}
			catch { }
		}
		return roots;
	}

	private static void DumpAllRoots()
	{
		var roots = GetAllRoots();
		Plugin.Log.LogInfo("[FM26Export] DIAG: корней найдено " + roots.Count);
		for (int i = 0; i < roots.Count; i++)
		{
			Plugin.Log.LogInfo("[FM26Export] DIAG: === корень " + i + " ===");
			_dumpCount = 0; DumpTree(roots[i], 0, 30);
		}
	}

	private static int _dumpCount;

	private static void DumpTree(VisualElement el, int depth, int maxDepth)
	{
		if (el == null || depth > maxDepth) return;
		if (++_dumpCount > 6000) return;
		var sb = new System.Text.StringBuilder();
		sb.Append(' ', depth * 2);
		sb.Append(string.IsNullOrEmpty(el.name) ? "(no name)" : el.name);
		sb.Append(" <").Append(((object)el).GetType().Name).Append('>');
		Plugin.Log.LogInfo("[TREE] " + sb.ToString());
		int n = 0;
		try { n = el.childCount; } catch { }
		for (int i = 0; i < n; i++)
		{
			VisualElement child = null;
			try { child = el.ElementAt(i); } catch { }
			DumpTree(child, depth + 1, maxDepth);
		}
	}

	private VisualElement GetMainRoot()
	{
		// FM27: корень интерфейса отдаёт SI.UI.DocumentManager (в FM26 его не было).
		try
		{
			var dm = global::SI.UI.DocumentManager.Instance;
			if (dm != null)
			{
				var mainRoot = dm.MainRoot;
				if (mainRoot != null)
					return mainRoot;
				var mainDoc = dm.MainDocument;
				if (mainDoc != null && mainDoc.Root != null)
					return mainDoc.Root;
			}
			Plugin.Log.LogWarning("[FM26Export] DocumentManager.MainRoot пуст, пробую UIDocument.");
		}
		catch (System.Exception e)
		{
			Plugin.Log.LogWarning("[FM26Export] DocumentManager недоступен: " + e.Message);
		}

		try
		{
			Il2CppArrayBase<UIDocument> docs = Object.FindObjectsOfType<UIDocument>();
			if (docs == null) return null;
			var e = docs.GetEnumerator();
			try
			{
				while (((global::System.Collections.IEnumerator)e).MoveNext())
				{
					UIDocument cur = e.Current;
					if ((Object)(object)cur != (Object)null
						&& cur.rootVisualElement != null
						&& cur.rootVisualElement.name == "PanelManager-container")
						return cur.rootVisualElement;
				}
			}
			finally { ((global::System.IDisposable)e)?.Dispose(); }
		}
		catch { }
		return null;
	}

	private void StartCapture()
	{
		try
		{
			var roots = GetAllRoots();
			if (roots.Count == 0)
			{
				Plugin.Log.LogError("[FM26Export] Sem UIDocument (Painel principal não encontrado).");
				return;
			}
			foreach (var root in roots)
			{
				foreach (var h in _availableHandlers)
				{
					if (h.TryStartCapture(root, out var err))
					{
						Plugin.Log.LogInfo("[FM26Export] Usando handler: " + ((MemberInfo)((object)h).GetType()).Name);
						_currentHandler = h;
						return;
					}
					if (!string.IsNullOrEmpty(err))
						Plugin.Log.LogInfo(err);
				}
			}
			Plugin.Log.LogWarning("[FM26Export] Nenhuma tela suportada para exportação encontrada no momento.");
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError("[FM26Export] Erro ao iniciar captura: " + ex.Message);
		}
	}
}
