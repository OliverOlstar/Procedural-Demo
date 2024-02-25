
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
	[Core.DebugOptionList]
	public static class DebugOptions
	{
		public static readonly Core.DebugOption IncludeVariants = new Core.DebugOption.StringWithDropdown(Core.DebugOption.Group.Data, "Include Variants", GetVariants(), Core.DebugOption.DefaultSetting.OnDevice, Core.DebugOption.ReleaseSetting.AlwaysOn, 
			tooltip: "Runtime:\n  Disabled - ignore variants\n  Enabled - default\n  Input - force use x variant\n" +
			"Editor:\n  Disabled - don\'t import variants\n  Enabled - import base & all variants\n  Input - import base & variant x");

		private static string[] GetVariants()
		{
			if (!StaticDataConfig.TryGet(out StaticDataConfig dataConfig))
			{
				return null;
			}
			return dataConfig.BuildVariantsDictionary(true).Keys.ToArray();
		}
	}

	public class DataImporterUtil
	{
		private static HashSet<string> TRUE_VALUES = new HashSet<string>()
		{
			"t", "true"
		};
		private static HashSet<string> FALSE_VALUES = new HashSet<string>()
		{
			"f", "false"
		};

		public static bool TryParseValue(
			string sheetName,
			string rowName,
			string columnName,
			object rawValue,
			System.Type valueType,
			out object value)
		{
			string strValue = rawValue.ToString();
			if (string.IsNullOrEmpty(strValue))
			{
				value = default;
				return false; // Don't raise a validation error for empty values
			}
			if (strValue.StartsWith(" ") || strValue.EndsWith(" "))
			{
				DataValidationUtil.Raise(sheetName, rowName, columnName, strValue,
					"String starts or ends with a space");
				value = default;
				return false;
			}
			if (valueType == typeof(int))
			{
				if (int.TryParse(strValue, out int i))
				{
					value = i;
					return true;
				}
				else if (float.TryParse(strValue, out float f))
				{
					value = Mathf.FloorToInt(f); // Truncate decimal point
					return true;
				}
			}
			else if (valueType == typeof(float))
			{
				if (float.TryParse(strValue, out float f))
				{
					value = f;
					return true;
				}
			}
			else if (valueType == typeof(double))
			{
				if (double.TryParse(strValue, out double d))
				{
					value = d;
					return true;
				}
			}
			else if (valueType.BaseType == typeof(System.Enum))
			{
				if (System.Enum.IsDefined(valueType, strValue))
				{
					value = System.Enum.Parse(valueType, strValue);
					return true;
				}
			}
			else if (valueType == typeof(bool))
			{
				if (TRUE_VALUES.Contains(strValue.ToLower()))
				{
					value = true;
					return true;
				}
				else if (FALSE_VALUES.Contains(strValue.ToLower()))
				{
					value = false;
					return true;
				}
			}
			else if (valueType == typeof(string))
			{
				value = strValue;
				return true;
			}
			else
			{
				// No conversion was possbile
				DataValidationUtil.Raise(sheetName, rowName, columnName, rawValue.ToString(),
					valueType + " is not a supported data value type (maybe you are missing the ForeignKey attribute)");
				value = default;
				return false;
			}

			// No conversion was possbile
			DataValidationUtil.Raise(sheetName, rowName, columnName, rawValue.ToString(),
				"Cannot convert value to type " + valueType);
			value = default;
			return false;
		}

		public static readonly string FIELD_NAME_PREFIX = "m_";

		public static bool TryFieldNameToColumnName(string fieldName, out string columnName)
		{
			if (!fieldName.StartsWith(FIELD_NAME_PREFIX))
			{
				columnName = null;
				return false;
			}
			columnName = fieldName.Substring(FIELD_NAME_PREFIX.Length);
			return true;
		}

		public static void AssignField(object entry, Raw.Sheet sheet, Raw.Row row, Raw.ReflectionCache refCache, object defaultEntry = null)
		{
			foreach (Raw.ReflectionCache.ImportField f in refCache.ImportFields)
			{
				if (f.ImportAt != null)
				{
					if (defaultEntry != null)
					{
						f.Info.SetValue(entry, f.GetDefaultValue(defaultEntry));
					}
					continue; // The attribute will handle importing the value through a different process
				}
				object rawValue = null;
				if (!row.TryGetValue(f.ColumnName, out rawValue) || rawValue == null)
				{
					if (defaultEntry != null)
					{
						f.Info.SetValue(entry, f.GetDefaultValue(defaultEntry));
					}
					continue;
				}
				if (TryParseValue(
					sheet.SheetName,
					row[sheet.IDColumnName].ToString(),
					f.ColumnName,
					rawValue,
					f.Info.FieldType,
					out object value))
				{
					f.Info.SetValue(entry, value);
				}
				else if (defaultEntry != null)
				{
					f.Info.SetValue(entry, f.GetDefaultValue(defaultEntry));
				}
			}
		}

		public static readonly string METHOD_NAME_PREFIX = "Import";

		public static bool TryMethodNameToColumnName(string fieldName, out string columnName)
		{
			if (!fieldName.StartsWith(METHOD_NAME_PREFIX))
			{
				columnName = null;
				return false;
			}
			columnName = fieldName.Substring(METHOD_NAME_PREFIX.Length);
			return true;
		}

		public static void InvokeMethod(
			object entry,
			Raw.Sheet sheet,
			Raw.Row row,
			Raw.ReflectionCache refCache)
		{
			foreach (Raw.ReflectionCache.ImportMethod m in refCache.ImportMethods)
			{
				if (m.ImportAt != null)
				{
					continue; // The attribute will handle importing the value through a different process
				}
				object rawValue = null;
				if (!row.TryGetValue(m.ColumnName, out rawValue) || rawValue == null)
				{
					continue;
				}
				if (TryParseValue(
					sheet.SheetName,
					row[sheet.IDColumnName].ToString(),
					m.ColumnName,
					rawValue,
					m.Parameter.ParameterType,
					out object arg))
				{
					m.Info.Invoke(entry, new object[] { arg });
				}
				else if (m.Parameter.HasDefaultValue) // Use a default value if one is provided
				{
					m.Info.Invoke(entry, new object[] { m.Parameter.DefaultValue });
				}
			}
		}
	}
}
