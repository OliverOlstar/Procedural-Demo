
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Data.Raw
{
	public class Row : Dictionary<string, object>
	{

	}

	public class Rows : Dictionary<string, Row>
	{
		public Rows(int count) : base(count) { }
	}

	public class ReflectionCache
	{
		public class ImportField
		{
			public string ColumnName;
			public FieldInfo Info;
			public Import.BaseAttribute ImportAt;

			bool m_InitDefaultValue = false;
			object m_DefaultValue = null;
			public object GetDefaultValue(object defaultObject)
			{
				if (!m_InitDefaultValue)
				{
					m_InitDefaultValue = true;
					m_DefaultValue = Info.GetValue(defaultObject);
				}
				return m_DefaultValue;
			}
		}
		public class ImportMethod
		{
			public string ColumnName;
			public MethodInfo Info;
			public ParameterInfo Parameter;
			public Import.BaseAttribute ImportAt;
		}
		public class ValidateField
		{
			public string ColumnName;
			public FieldInfo Info;
			public Validate.BaseAttribute ValidateAt;
		}
		public List<ImportField> ImportFields = new List<ImportField>();
		public List<ImportMethod> ImportMethods = new List<ImportMethod>();
		public List<ValidateField> Validation = new List<ValidateField>();
	}

	public class Sheet
	{
		public static readonly Raw.Sheet EMPTY = new Raw.Sheet()
		{
			SheetName = "Empty",
			Rows = new Raw.Rows(0),
			IDColumnName = "ID",
			ColumnNames = new HashSet<string>() { "ID" },
		};

		public string SheetName;
		public Rows Rows;
		public string IDColumnName;
		public HashSet<string> ColumnNames;

		private Dictionary<System.Type, ReflectionCache> m_ReflectionCache = new Dictionary<System.Type, ReflectionCache>();
		public ReflectionCache GetReflectionCache<TData>()
		{
			return GetReflectionCache(typeof(TData));
		}
		public ReflectionCache GetReflectionCache(System.Type type)
		{
			ReflectionCache cache = null;
			if (m_ReflectionCache.TryGetValue(type, out cache))
			{
				return cache;
			}
			cache = new ReflectionCache();
			m_ReflectionCache.Add(type, cache);
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			int fieldCount = fields.Length;
			for (int i = 0; i < fieldCount; i++)
			{
				FieldInfo field = fields[i];
				if (!DataImporterUtil.TryFieldNameToColumnName(field.Name, out string columnName))
				{
					continue;
				}
				Import.BaseAttribute imp = field.GetCustomAttribute<Import.BaseAttribute>();
				if (ColumnNames.Contains(columnName))
				{
					ReflectionCache.ImportField fieldCache = new ReflectionCache.ImportField()
					{
						ColumnName = columnName,
						Info = field,
						ImportAt = imp,
					};
					cache.ImportFields.Add(fieldCache);

				}
				else if (imp != null)
				{
					DataValidationUtil.Raise(SheetName, $"Field '{field.Name}' in C# class {type.Name} has import attribute " +
						$"[{imp.GetType().Name}] attached but no matching column '{columnName}' exists in sheet {SheetName}");
				}
				Validate.BaseAttribute val = field.GetCustomAttribute<Validate.BaseAttribute>();
				if (val != null && Validate.BaseAttribute.IsValidForField(val, columnName, type, field, this))
				{
					ReflectionCache.ValidateField validate = new ReflectionCache.ValidateField()
					{
						ColumnName = columnName,
						Info = field,
						ValidateAt = val,
					};
					cache.Validation.Add(validate);
				}
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
			int methodCount = methods.Length;
			for (int i = 0; i < methodCount; i++)
			{
				MethodInfo method = methods[i];
				if (!DataImporterUtil.TryMethodNameToColumnName(method.Name, out string columnName))
				{
					continue;
				}
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length != 1)
				{
					continue;
				}
				Import.BaseAttribute imp = method.GetCustomAttribute<Import.BaseAttribute>();
				if (ColumnNames.Contains(columnName))
				{
					ReflectionCache.ImportMethod methodCache = new ReflectionCache.ImportMethod()
					{
						ColumnName = columnName,
						Info = method,
						Parameter = parameters[0],
						ImportAt = imp,
					};
					cache.ImportMethods.Add(methodCache);
				}
				// Oko's stat system relies heavily on having many import functions for fields that may or may not exist
				// It might be good to try to turn this validation on eventually, but it's too much work right now
				//else if (imp != null)
				//{
				//	DataValidationUtil.Raise(SheetName, $"Method '{method.Name}()' in C# class {type.Name} has import attribute " +
				//		$"[{imp.GetType().Name}] attached but no matching column '{columnName}' exists in sheet {SheetName}");
				//}
			}
			return cache;
		}

		public const string VARIANT_OPERATOR_COLUMN_NAME = "Op";
		public const string VARIANT_OPERATOR_ADD = "+";
		public const string VARIANT_OPERATOR_SUBTRACT = "-";
		public const string VARIANT_OPERATOR_MODIFY = "x";

		public void ApplyVariant(Raw.Sheet variantSheet)
		{
			if (!variantSheet.ColumnNames.Contains(VARIANT_OPERATOR_COLUMN_NAME))
			{
				DataValidationUtil.Raise(variantSheet.SheetName,
					$"All variant sheets must contain {VARIANT_OPERATOR_COLUMN_NAME} column name");
				return;
			}
			foreach (KeyValuePair<string, Row> variantRowKvp in variantSheet.Rows)
			{
				string ID = variantRowKvp.Key;
				Row variantRow = variantRowKvp.Value;
				if (!variantRow.TryGetValue(VARIANT_OPERATOR_COLUMN_NAME, out object operatorValue) || operatorValue == null)
				{
					DataValidationUtil.Raise(variantSheet.SheetName, ID, VARIANT_OPERATOR_COLUMN_NAME, "Cannot be empty");
					continue;
				}
				switch (operatorValue)
				{
					case VARIANT_OPERATOR_ADD:
						ApplyAddVariant(ID, variantRow);
						break;
					case VARIANT_OPERATOR_SUBTRACT:
						ApplySubtractVariant(ID, variantRow);
						break;
					case VARIANT_OPERATOR_MODIFY:
						ApplyModifyVariant(ID, variantRow);
						break;
					default:
						DataValidationUtil.Raise(variantSheet.SheetName, ID, VARIANT_OPERATOR_COLUMN_NAME, operatorValue.ToString(), "Invalid value");
						break;
				}
			}
		}

		private void ApplyAddVariant(string ID, Row variantRow)
		{
			if (Rows.ContainsKey(ID))
			{
				DataValidationUtil.Raise(SheetName, ID,
					$"Variant operator {VARIANT_OPERATOR_ADD} cannot be applied, a row with ID {ID} already exist in {DataVariant.Base.Name} {SheetName}");
				return;
			}
			foreach (string columnName in ColumnNames)
			{
				if (!variantRow.ContainsKey(columnName))
				{
					DataValidationUtil.Raise(SheetName, ID, columnName,
						$"Variant operator {VARIANT_OPERATOR_ADD} cannot be applied, column is missing in variant sheet");
					return;
				}
			}
			Rows.Add(ID, variantRow);
		}

		private void ApplySubtractVariant(string ID, Row variantRow)
		{
			if (!Rows.ContainsKey(ID))
			{
				DataValidationUtil.Raise(SheetName, ID,
					$"Variant operator {VARIANT_OPERATOR_SUBTRACT} cannot be applied, a row with ID {ID} does not exist in {DataVariant.Base.Name} {SheetName}");
				return;
			}
			Rows.Remove(ID);
		}

		private void ApplyModifyVariant(string ID, Row variantRow)
		{
			if (!Rows.TryGetValue(ID, out Row baseRow))
			{
				DataValidationUtil.Raise(SheetName, ID,
					$"Variant operator {VARIANT_OPERATOR_MODIFY} requires a row with matching ID {ID} in {DataVariant.Base.Name} {SheetName}");
				return;
			}
			foreach (KeyValuePair<string, object> pair in variantRow)
			{
				if (string.Equals(pair.Value.ToString(), "\""))
				{
					continue; // Special character to skip column and use base value
				}
				string columnName = pair.Key;
				if (string.Equals(columnName, VARIANT_OPERATOR_COLUMN_NAME))
				{
					continue;
				}
				if (!baseRow.ContainsKey(columnName))
				{
					DataValidationUtil.Raise(SheetName, ID, columnName,
						$"Variant operator {VARIANT_OPERATOR_MODIFY} is trying modify a column {columnName} that doesn't exist in {DataVariant.Base.Name} {SheetName}");
					continue;
				}
				baseRow[columnName] = pair.Value;
			}
		}
	}
}
