using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FM26PlayerExport;
using UnityEngine.UIElements;

namespace FM26PlayerExport.Handlers;

/// <summary>
/// Обработчик таблиц FM27.
/// В FM27 таблица устроена так: контейнер «columns-root» с шаблонами
/// «TIFO-TableHeaderTemplate» (заголовки) и соседний «View» (строки).
/// Ячейка строки — прямой потомок строки, текст лежит в её поддереве.
/// </summary>
public sealed class Fm27GridTableHandler : IExportHandler
{
	private VisualElement _rows;
	private List<string> _headers;
	private bool _done;

	private int headerListSourceCount() { return _headers == null ? 0 : _headers.Count; }

	private static void DumpRow(VisualElement el, int d, int max)
	{
		if (el == null || d > max) return;
		int n = 0;
		try { n = el.childCount; } catch { }
		string nm = string.IsNullOrEmpty(el.name) ? "(no name)" : el.name;
		Plugin.Log.LogInfo("[ROW] " + new string(' ', d * 2) + nm + " [" + n + "] text='" + TextOf(el) + "'");
		for (int i = 0; i < n; i++)
		{
			VisualElement ch = null;
			try { ch = el.ElementAt(i); } catch { }
			DumpRow(ch, d + 1, max);
		}
	}

	private static VisualElement CellRoot(VisualElement row)
	{
		// FM27: ячейки строки лежат в контейнере DragObject
		var drag = UIUtils.FindByName(row, "DragObject");
		if (drag != null && drag.childCount > 2) return drag;
		VisualElement cur = row;
		for (int guard = 0; guard < 6; guard++)
		{
			if (cur == null) return null;
			int n = 0;
			try { n = cur.childCount; } catch { }
			if (n != 1) return cur;
			VisualElement only = null;
			try { only = cur.ElementAt(0); } catch { }
			if (only == null) return cur;
			cur = only;
		}
		return cur;
	}

	private static string TextOf(VisualElement el)
	{
		if (el == null) return string.Empty;
		try
		{
			string direct = UIUtils.GetText(el, false, false);
			if (!string.IsNullOrWhiteSpace(direct)) return direct.Trim();
		}
		catch { }
		try
		{
			var list = new List<string>();
			UIUtils.CollectAllTexts(el, list, 0, false, false);
			foreach (var t in list)
				if (!string.IsNullOrWhiteSpace(t)) return t.Trim();
		}
		catch { }
		return string.Empty;
	}

	private static void FindAllByName(VisualElement el, string name, List<VisualElement> acc)
	{
		if (el == null) return;
		if (el.name == name) acc.Add(el);
		int n = 0;
		try { n = el.childCount; } catch { }
		for (int i = 0; i < n; i++)
		{
			VisualElement ch = null;
			try { ch = el.ElementAt(i); } catch { }
			FindAllByName(ch, name, acc);
		}
	}

	public bool TryStartCapture(VisualElement root, out string errorMessage)
	{
		errorMessage = string.Empty;
		if (root == null) return false;

		// FM27: заголовки — элементы TIFO-TableHeaderTemplate (columns-root пустой),
		// строки — самый наполненный View.
		var headerCells = new List<VisualElement>();
		FindAllByName(root, "TIFO-TableHeaderTemplate", headerCells);
		var views = new List<VisualElement>();
		FindAllByName(root, "View", views);
		Plugin.Log.LogInfo("[FM27] заголовков: " + headerCells.Count + ", View: " + views.Count);

		VisualElement rowsRoot = null;
		int bestRows = 0;
		foreach (var v in views)
		{
			if (v == null) continue;
			int k = 0;
			try { k = v.childCount; } catch { }
			if (k > bestRows) { bestRows = k; rowsRoot = v; }
		}
		Plugin.Log.LogInfo("[FM27] лучший View: строк " + bestRows);

		var headerTexts = new List<string>();
		foreach (var h in headerCells)
		{
			headerTexts.Add(TextOf(h));
		}
		if (headerTexts.Count > 0)
			Plugin.Log.LogInfo("[FM27] заголовки: " + string.Join(" | ", headerTexts.ToArray()));

		VisualElement headersRoot = headerCells.Count > 0 ? headerCells[0] : null;
		if (headersRoot == null || rowsRoot == null)
		{
			errorMessage = "[FM27] нет подходящего columns-root/View";
			return false;
		}

		if (headerTexts.Count < 2 || rowsRoot == null || rowsRoot.childCount == 0)
		{
			errorMessage = "[FM27] мало данных: заголовков " + headerTexts.Count;
			return false;
		}

		_rows = rowsRoot;
		_headers = headerTexts;
		_done = false;
		Plugin.Log.LogInfo("[FM27] Таблица: колонок " + _headers.Count + ", строк " + rowsRoot.childCount);
		return true;
	}

	public bool CaptureStep()
	{
		if (_done) return true;
		_done = true;
		try
		{
			WriteAll();
		}
		catch (Exception e)
		{
			Plugin.Log.LogError("[FM27] Ошибка записи: " + e);
		}
		return true;
	}

	private void WriteAll()
	{
		string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		string dirBase = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			"Sports Interactive", "Football Manager 27", "FM27PlayerExport by vinteset");
		string csvDir = Path.Combine(dirBase, "Exports CSV");
		string htmlDir = Path.Combine(dirBase, "Exports HTML");
		Directory.CreateDirectory(csvDir);
		Directory.CreateDirectory(htmlDir);

		string csvPath = Path.Combine(csvDir, "fm27_table_" + stamp + ".csv");
		string htmlPath = Path.Combine(htmlDir, "fm27_table_" + stamp + ".html");

		int written = 0;
		// число колонок = число ячеек в первой строке; заголовки подрезаем под него
		int cols = 0;
		if (_rows.childCount > 0)
		{
			var firstCells = CellRoot(_rows.ElementAt(0));
			if (firstCells != null) cols = firstCells.childCount;
		}
		// В FM27 первый заголовок пустой, а «Выбран» стоит на месте колонки-иконки.
		if (headerListSourceCount() > 1 && string.IsNullOrWhiteSpace(_headers[0]) && !string.IsNullOrWhiteSpace(_headers[1]))
		{
			var tmp = _headers[1];
			_headers[1] = _headers[0];
			_headers[0] = tmp;
		}

		var headerList = new List<string>();
		for (int i = 0; i < cols; i++)
			headerList.Add(i < _headers.Count ? _headers[i] : "col" + (i + 1));
		Plugin.Log.LogInfo("[FM27] колонок по ячейкам: " + cols);

		using (var csv = new StreamWriter(csvPath, false, new UTF8Encoding(false), 65536))
		{
			csv.WriteLine(string.Join(";", headerList));
			int cellCount = 0;
			for (int r = 0; r < _rows.childCount; r++)
			{
				VisualElement row = _rows.ElementAt(r);
				VisualElement cells = CellRoot(row);
				if (cells == null) continue;
				var values = new List<string>();
				for (int c = 0; c < cells.childCount; c++)
				{
					string t = TextOf(cells.ElementAt(c));
					values.Add(t.Replace(";", ",").Replace("\n", " "));
				}
				if (r == 0)
				{
					cellCount = values.Count;
					Plugin.Log.LogInfo("[FM27] первая строка: ячеек " + values.Count + " -> " + string.Join(" | ", values.GetRange(0, Math.Min(10, values.Count))));
				}
				if (values.Count == 0) continue;
				csv.WriteLine(string.Join(";", values));
				written++;
			}
		}

		using (var html = new StreamWriter(htmlPath, false, new UTF8Encoding(false)))
		{
			html.WriteLine("<html><head><meta charset=\"UTF-8\">");
			html.WriteLine("<style>body,td,th{font-family:Verdana,Arial,sans-serif;font-size:12px}table{border-collapse:collapse;width:98%;margin:20px auto}th{background:#EEE;border:1px solid #000;padding:5px;text-align:left}td{border:1px solid #000;padding:4px}</style>");
			html.WriteLine("</head><body><table><tr>");
			foreach (var h in headerList) html.WriteLine("<th>" + System.Net.WebUtility.HtmlEncode(h) + "</th>");
			html.WriteLine("</tr>");
			for (int r = 0; r < _rows.childCount; r++)
			{
				VisualElement row = _rows.ElementAt(r);
				VisualElement cells = CellRoot(row);
				if (cells == null || cells.childCount == 0) continue;
				html.WriteLine("<tr>");
				for (int c = 0; c < cells.childCount; c++)
					html.WriteLine("<td>" + System.Net.WebUtility.HtmlEncode(TextOf(cells.ElementAt(c))) + "</td>");
				html.WriteLine("</tr>");
			}
			html.WriteLine("</table></body></html>");
		}

		Plugin.Log.LogInfo("[FM27] OK " + written + " строк. CSV: " + csvPath);
		Plugin.Log.LogInfo("[FM27] HTML: " + htmlPath);
	}

	public void FinishCapture() { }

	public void Cleanup()
	{
		_rows = null;
		_headers = null;
		_done = false;
	}
}
