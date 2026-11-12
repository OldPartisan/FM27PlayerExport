using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace FM26PlayerExport;

[BepInPlugin("com.koda.fm26.playerexport", "FM26 Player Export (macOS)", "5.1.0-macos")]
public class Plugin : BasePlugin
{
	internal static ManualLogSource Log;
	internal static ExportDriver Driver;

	private static Harmony _harmony;
	private static bool _started;

	public override void Load()
	{
		Log = base.Log;
		Log.LogInfo("[FM26Export v5.1-macos] Loaded. Hotkeys: [F9 or Ctrl+P] = export.");
		PluginConfig.Init(base.Config, Log);

		// Схема, которая работает в BepInEx для FM27 (как в FMManagerPhoto):
		// точечный патч через AccessTools.Method + Harmony.Patch.
		// PatchAll здесь вешает игру.
		try
		{
			_harmony = new Harmony("com.koda.fm26.playerexport");
			var target = AccessTools.Method(typeof(FM.GamePlugin.GamePlugin), "Tick");
			if (target == null)
			{
				Log.LogError("[FM26Export] Не найден FM.GamePlugin.GamePlugin.Tick");
				return;
			}
			_harmony.Patch(target, postfix: new HarmonyMethod(typeof(Plugin), nameof(OnGameTick)));
			Log.LogInfo("[FM26Export] Патч на GamePlugin.Tick установлен.");
		}
		catch (Exception e)
		{
			Log.LogError("[FM26Export] Не удалось установить патч: " + e);
		}
	}

	/// <summary>Вызывается каждый кадр на главном потоке.</summary>
	private static void OnGameTick()
	{
		if (!_started)
		{
			_started = true;
			try
			{
				Driver = new ExportDriver();
				Log.LogInfo("[FM26Export] Драйвер создан в первом тике.");
			}
			catch (Exception e)
			{
				Log.LogError("[FM26Export] Ошибка создания драйвера: " + e);
			}
		}

		Driver?.Tick();
	}

	public override bool Unload()
	{
		_harmony?.UnpatchSelf();
		_harmony = null;
		Driver = null;
		return base.Unload();
	}
}
