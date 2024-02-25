using System.Collections.Generic;
using UnityEngine;

public class DataValidationUtil : Core.LockEditor<DataValidationUtil>
{
#if UNITY_2019_1_OR_NEWER
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
	static void Initialize()
	{
		ShowErrorReport();
	}

	private class VariantErrors : Dictionary<string, SheetErrors>
	{

	}
	private class SheetErrors : Dictionary<string, RowErrors>
	{

	}
	private class RowErrors : Dictionary<string, ColumnErrors>
	{

	}
	private class ColumnErrors : List<string>
	{

	}

	private static Dictionary<string, VariantErrors> s_Errors = new Dictionary<string, VariantErrors>();
	private static string s_Variant = DataVariant.Base.Name;
	public static string SetVariant(string value) => s_Variant = value;
	private static int s_Count = 0;
	public static bool Failed() { return s_Count > 0; }

	public static void Flush()
	{
		s_Errors.Clear();
		s_Count = 0;
		s_Variant = DataVariant.Base.Name;
	}

	public static string BuildSummaryReport()
	{
		if (s_Errors.Count == 0)
		{
			return null;
		}
		Core.Str.AddLine("Validation failed with ", s_Count.ToString(), " errors:");
		Core.Str.AddLine();
		foreach (KeyValuePair<string, VariantErrors> variantErrors in s_Errors)
		{
			foreach (KeyValuePair<string, SheetErrors> sheet in variantErrors.Value)
			{
				int count = 0;
				foreach (KeyValuePair<string, RowErrors> row in sheet.Value)
				{
					foreach (KeyValuePair<string, ColumnErrors> columns in row.Value)
					{
						count += columns.Value.Count;
					}
				}
				Core.Str.AddLine(variantErrors.Key.ToString(), "-", string.IsNullOrEmpty(sheet.Key) ? "Misc" : sheet.Key, " errors: ", count.ToString());
			}
		}
		string result = Core.Str.Finish();
		if (result.Length > 1600)
		{
			result = $"{result.Remove(1600, result.Length - 1600)}<truncated...>";
		}
		return result;
	}

	public static string BuildFullReport()
	{
		if (s_Errors.Count == 0)
		{
			return null;
		}
		Core.Str.AddLine("Validation failed with ", s_Count.ToString(), " errors:");
		foreach (KeyValuePair<string, VariantErrors> variantErrors in s_Errors)
		{
			Core.Str.AddLine(variantErrors.Key.ToString());
			foreach (KeyValuePair<string, SheetErrors> sheet in variantErrors.Value)
			{
				Core.Str.AddLine("   " + (string.IsNullOrEmpty(sheet.Key) ? "Misc" : sheet.Key));
				foreach (KeyValuePair<string, RowErrors> row in sheet.Value)
				{
					foreach (KeyValuePair<string, ColumnErrors> columns in row.Value)
					{
						foreach (string error in columns.Value)
						{
							Core.Str.AddLine("      ", error);
						}
					}
				}
				Core.Str.AddLine();
			}
		}
		return Core.Str.Finish();
	}

	public static void Raise(
		string sheetName, 
		string rowName, 
		string columnName, 
		string value,
		string error)
	{
		string s = Core.Str.Build("[", rowName, ",", columnName, "] = {", value, "} ", error);
		Add(s, sheetName, rowName, columnName);
	}

	public static void Raise(
		string sheetName,
		string rowName,
		string columnName,
		string error)
	{
		string s = Core.Str.Build("[", rowName, ",", columnName, "] ", error);
		Add(s, sheetName, rowName, columnName);
	}

	public static void Raise(
		string sheetName,
		string rowName,
		string error)
	{
		string s = Core.Str.Build("[", rowName, "] ", error);
		Add(s, sheetName, rowName);
	}

	public static void Raise(string sheetName, string error)
	{
		Add(error, sheetName);
	}

	public static void RaiseCode(System.Type type, string error)
	{
		Raise($"{type.Name}.cs", $"[Requires Code] {error}");
	}

	public static void Raise(string error)
	{
		Add(error);
	}

	private static void Add(
		string error,
		string sheetName = "",
		string rowName = "",
		string columnName = "")
	{
		if (!s_Errors.TryGetValue(s_Variant, out VariantErrors variantErrors))
		{
			variantErrors = new VariantErrors();
			s_Errors.Add(s_Variant, variantErrors);
		}
		if (!variantErrors.TryGetValue(sheetName, out SheetErrors sheet))
		{
			sheet = new SheetErrors();
			variantErrors.Add(sheetName, sheet);
		}
		if (!sheet.TryGetValue(rowName, out RowErrors row))
		{
			row = new RowErrors();
			sheet.Add(rowName, row);
		}
		if (!row.TryGetValue(columnName, out ColumnErrors column))
		{
			column = new ColumnErrors();
			row.Add(columnName, column);
		}
		s_Count++;
		column.Add(error);
	}

	public static bool HasErrors(string sheetName = "")
	{
		if (!s_Errors.TryGetValue(s_Variant, out VariantErrors variantErrors) || !variantErrors.TryGetValue(sheetName, out SheetErrors sheet))
		{
			return false;
		}
		return true;
	}

	public static void FinalizeReport()
	{
		if (Failed())
		{
			SetLock(BuildSummaryReport(), BuildFullReport());
		}
		else
		{
			ClearLock();
		}
	}

	public static void ShowResultPopup()
	{
		bool error = ShowErrorReport();
#if UNITY_EDITOR
		if (!error)
		{
			UnityEditor.EditorUtility.DisplayDialog(
					"Data Import",
					"Success!",
					"Ok");
		}
#endif
	}
}
