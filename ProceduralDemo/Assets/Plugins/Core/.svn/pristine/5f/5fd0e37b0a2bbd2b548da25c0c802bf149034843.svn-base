
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Data.Import
{
	[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Method)]
	public abstract class BaseAttribute : System.Attribute
	{
		//todo: better be renamed to allowNull to remove ambiguity, its also the term used in database schemes 
		private bool m_AllowBlankCells = true;
		public BaseAttribute(bool allowBlankCells = true)
		{
			m_AllowBlankCells = allowBlankCells;
		}

		protected abstract System.Type GetValueType();

		protected abstract object Import(
			string columnName,
			System.Type columnType,
			object rawValue,
			ref string importError);

		public void ImportField<T>(
			T data,
			Raw.Sheet sheet,
			Raw.Row rawRow,
			Raw.ReflectionCache.ImportField refCache)
		{
			if (TryApplyAttribute(
				sheet,
				rawRow,
				refCache.ColumnName,
				refCache.Info.FieldType,
				out object importedValue))
			{
				refCache.Info.SetValue(data, importedValue);
			}
		}
		public void ImportMethod<T>(
			T data,
			Raw.Sheet sheet,
			Raw.Row rawRow,
			Raw.ReflectionCache.ImportMethod refCache)
		{
			if (TryApplyAttribute(
				sheet,
				rawRow,
				refCache.ColumnName,
				refCache.Parameter.ParameterType,
				out object importedValue))
			{
				refCache.Info.Invoke(data, new object[] { importedValue });
			}
			else if (refCache.Parameter.HasDefaultValue) // If a default value is specified then call the method using the default
			{
				refCache.Info.Invoke(data, new object[] { refCache.Parameter.DefaultValue });
			}
		}

		private bool TryApplyAttribute(
			Raw.Sheet sheet,
			Raw.Row rawRow,
			string columnName,
			System.Type columnType,
			out object importedValue)
		{
			importedValue = null;
			if (!rawRow.TryGetValue(columnName, out object rawValue))
			{
				DataValidationUtil.Raise(sheet.SheetName, rawRow[sheet.IDColumnName].ToString(), columnName, rawValue.ToString(),
					GetType() + " Cannot find column in sheet");
				return false;
			}
			if (!DataImporterUtil.TryParseValue(
				sheet.SheetName,
				rawRow[sheet.IDColumnName].ToString(),
				columnName,
				rawValue,
				GetValueType(),
				out object value))
			{
				if (!m_AllowBlankCells)
				{
					DataValidationUtil.Raise(sheet.SheetName, rawRow[sheet.IDColumnName].ToString(), columnName, rawValue.ToString(),
						GetType() + " Value is null or could not be processed");
				}
				return false;
			}
			string importError = null;
			importedValue = Import(columnName, columnType, value, ref importError);
			if (!string.IsNullOrEmpty(importError))
			{
				DataValidationUtil.Raise(sheet.SheetName, rawRow[sheet.IDColumnName].ToString(), columnName, rawValue.ToString(),
					GetType() + " " + importError);
				return false;
			}
			if (!columnType.IsAssignableFrom(importedValue.GetType()))
			{
				DataValidationUtil.Raise(sheet.SheetName, rawRow[sheet.IDColumnName].ToString(), columnName, rawValue.ToString(),
					GetType() + " Imported value " + importedValue + " of type " + importedValue.GetType() + " does not match expected type " + columnType);
				return false;
			}
			return true;
		}
	}

	namespace String
	{
		public abstract class StringAttribute : BaseAttribute
		{
			protected override System.Type GetValueType() { return typeof(string); }
			protected override object Import(string columnName, System.Type columnType, object rawValue, ref string importError)
			{
				return ImportString(columnName, columnType, (string)rawValue, ref importError);
			}
			protected abstract string ImportString(string columnName, System.Type columnType, string rawValue, ref string importError);
		}
	}

	namespace Reference
	{
		public abstract class ReferenceAttribute : BaseAttribute
		{
			public ReferenceAttribute(bool allowBlanks = false) : base(allowBlanks) { } // Typically asset reference should not be null
			protected override System.Type GetValueType() { return typeof(string); }
			protected override object Import(string columnName, System.Type columnType, object rawValue, ref string importError)
			{
				return ImportReference(columnName, columnType, (string)rawValue, ref importError);
			}
			protected abstract UnityEngine.Object ImportReference(string columnName, System.Type columnType, string rawValue, ref string importError);
		}
	}
}
