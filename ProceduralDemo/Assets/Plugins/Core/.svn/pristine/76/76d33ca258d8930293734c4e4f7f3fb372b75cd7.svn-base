using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Data.Validate
{
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public abstract class BaseAttribute : System.Attribute
	{
		public static bool IsValidForField(BaseAttribute at, string columnName, System.Type dataType, FieldInfo info, Raw.Sheet sheet)
		{
			if (!sheet.ColumnNames.Contains(columnName))
			{
				if (string.IsNullOrEmpty(at.m_ColumnNameToDisplayInErrors))
				{
					DataValidationUtil.Raise(sheet.SheetName, $"Field '{info.Name}' in C# class '{dataType.Name}' has validation attribute " +
						$"[{at.GetType().Name}] but no column named '{columnName}' exists in the sheet. " +
						$"You can use column override name argument to specify the name of the column to validate when it's not implied by the field name");
					return false;
				}
				if (!sheet.ColumnNames.Contains(at.m_ColumnNameToDisplayInErrors))
				{
					DataValidationUtil.Raise(sheet.SheetName, $"Field '{info.Name}' in C# class '{dataType.Name}' has validation attribute " +
						$"[{at.GetType().Name}] with column override name '{at.m_ColumnNameToDisplayInErrors}' but no such column name exists in the sheet");
					return false;
				}
			}
			else if (!string.IsNullOrEmpty(at.m_ColumnNameToDisplayInErrors))
			{
				DataValidationUtil.Raise(sheet.SheetName, $"Field '{info.Name}' in C# class '{dataType.Name}' has validation attribute " +
					$"[{at.GetType().Name}] with column override name '{at.m_ColumnNameToDisplayInErrors}' but there is no need to override the column name. " +
					$"The column name '{columnName}' can be derrived implicitly from this field.");
				return false;
			}
			return true;
		}

		private string m_ColumnNameToDisplayInErrors = string.Empty;

		public BaseAttribute(string columnNameToDisplayInErrors)
		{
			m_ColumnNameToDisplayInErrors = columnNameToDisplayInErrors;
		}

		protected abstract System.Type GetValidationType();

		protected virtual string OnValidateAttribute() => null;
		protected abstract string OnValidateData(object obj);

		public void ValidateAttribute(Raw.Sheet sheet, Raw.ReflectionCache.ValidateField field)
		{
			string error = OnValidateAttribute();
			if (string.IsNullOrEmpty(error))
			{
				return;
			}
			DataValidationUtil.Raise(sheet.SheetName, $"{field.Info.Name} member validation has error: {error}");
		}

		public void ValidateData(IDataDictItem data, Raw.Sheet sheet, Raw.ReflectionCache.ValidateField field)
		{
			if (!GetValidationType().IsAssignableFrom(field.Info.FieldType))
			{
				DataValidationUtil.Raise(sheet.SheetName, data.ID, field.ColumnName,
					$"{GetType()} is not able to validate this field, cannot convert {field.Info.FieldType} to {GetValidationType()}");
				return;
			}
			object value = field.Info.GetValue(data);
			string error = OnValidateData(value);
			if (string.IsNullOrEmpty(error))
			{
				return;
			}
			string valueAsString = value != null ? value.ToString() : "";
			string columnName = !string.IsNullOrEmpty(m_ColumnNameToDisplayInErrors) ? m_ColumnNameToDisplayInErrors : field.ColumnName;
			DataValidationUtil.Raise(sheet.SheetName, data.ID, columnName, valueAsString, $"{GetType()} {error}");
		}
	}

	namespace Float
	{
		public abstract class FloatAttribute : BaseAttribute
		{
			public FloatAttribute(string columnNameToDisplayInErrors) : base(columnNameToDisplayInErrors)
			{

			}

			protected override System.Type GetValidationType() { return typeof(float); }

			protected override string OnValidateData(object obj)
			{
				return ValidateFloatValue((float)obj);
			}

			protected abstract string ValidateFloatValue(float value);
		}
	}

	namespace Int
	{
		public abstract class IntAttribute : BaseAttribute
		{
			public IntAttribute(string columnNameToDisplayInErrors) : base(columnNameToDisplayInErrors)
			{

			}

			protected override System.Type GetValidationType() { return typeof(int); }

			protected override string OnValidateData(object obj)
			{
				return ValidateIntValue((int)obj);
			}

			protected abstract string ValidateIntValue(int value);
		}
	}

	namespace String
	{
		public abstract class StringAttribute : BaseAttribute
		{
			private bool m_AllowBlanks = false;

			public StringAttribute(bool allowBlanks, string columnNameToDisplayInErrors) : base(columnNameToDisplayInErrors)
			{
				m_AllowBlanks = allowBlanks;
			}

			protected override System.Type GetValidationType() { return typeof(string); }

			protected sealed override string OnValidateData(object obj)
			{
				string s = (string)obj;
				if (string.IsNullOrEmpty(s))
				{
					return ValidateBlankValue(s);
				}
				return ValidateStringValue(s);
			}

			protected virtual string ValidateBlankValue(string value) => m_AllowBlanks ? null : "This field cannot be left blank";
			protected abstract string ValidateStringValue(string value);
		}
	}
}
