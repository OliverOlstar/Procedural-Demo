
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBSingleton<TSingleton, TImporter, TDataStruct, TData> : DBBase
		where TSingleton : DBSingleton<TSingleton, TImporter, TDataStruct, TData>
		where TImporter : IDataImporter<TDataStruct, TData>, new()
		where TDataStruct : IReadOnlyCollection<TData>
		where TData : IDataDictItem
	{
		public static bool Exists { get { return DBManager.TryGet(out TSingleton instance); } }

		public static TSingleton Instance
		{
			get
			{
				if (!DBManager.TryGet(out TSingleton instance))
				{
					Debug.LogError(typeof(TSingleton) + ".Instance Unavaliable make sure DBManager is initialized before calling Instance");
				}
				return instance;
			}
		}

#if UNITY_2019_1_OR_NEWER
		[SerializeReference] // Used to serialize generic data without using nested scriptable objects, was introduced in 2019
#else
		[SerializeField]
#endif
		private TDataStruct m_RawData = default;
		sealed public override int EditorRawDataCount => m_RawData != null ? m_RawData.Count : 0;
		sealed public override IEnumerable<IDataDictItem> GetEditorRawData()
		{
			if (m_RawData == null) // Possible this is null if previous import had an exception
			{
				yield break;
			}
			foreach (TData data in m_RawData)
			{
				yield return data;
			}
		}

		public override bool ImportFromSheet => true;
		public override string SheetName => typeof(TData).Name;

		sealed public override System.Type DataType => typeof(TData);
		sealed public override System.Type DataStructType => typeof(TDataStruct);
		public virtual System.Type _EditorGenericDataBaseType => typeof(TData);

		sealed public override void Initialize(List<DataSourceBase> sources)
		{
			List<TDataStruct> data = Core.ListPool<TDataStruct>.Request();
			int count = 0;
			if (m_RawData == null)
			{
				Core.DebugUtil.DevException($"{GetType().Name}.Initialize() Object {name} serialized null RawData {typeof(TDataStruct).Name}. " +
					$"This DB could have been corrupted by deleting or renaming a script? Data classes might be missing System.Serializable attribute? Scriptable Object class name doesn't match .cs file name?");
			}
			else
			{
				data.Add(m_RawData);
				count += m_RawData.Count;
			}
			foreach (DataSourceBase src in sources)
			{
				if (src.RawData is TDataStruct rawData)
				{
					data.Add(rawData);
					count += rawData.Count;
				}
			}
			InitializeInternal(data, count);
			OnInitialize();
			Core.ListPool<TDataStruct>.Return(data);
		}

		sealed public override void EditorImportData(Raw.Sheet sheet, out string error)
		{
			if (this is IDataPreImport preDB)
			{
				preDB.OnDataPreImport();
			}
			TImporter importer = new TImporter();
			TDataStruct data = importer.ImportRawData(this, sheet, out error);
			m_RawData = data;
		}

		sealed public override void PostProcess(Raw.Sheet sheet)
		{
			if (m_RawData == null)
			{
				Core.DebugUtil.DevException($"RawData in {sheet.SheetName} is currently null");
			}
			
			foreach (TData data in m_RawData)
			{
				if (!sheet.Rows.TryGetValue(data.ID.ToString(), out Raw.Row rawRow))
				{
					continue;
				}
				Raw.ReflectionCache refCache = sheet.GetReflectionCache(data.GetType());
				foreach (Raw.ReflectionCache.ImportField field in refCache.ImportFields)
				{
					if (field.ImportAt != null)
					{
						field.ImportAt.ImportField(data, sheet, rawRow, field);
					}
				}
				foreach (Raw.ReflectionCache.ImportMethod method in refCache.ImportMethods)
				{
					if (method.ImportAt != null)
					{
						method.ImportAt.ImportMethod(data, sheet, rawRow, method);
					}
				}
				if (data is IDataPostImport post)
				{
					post.OnDataPostImport();
				}
			}
			PostProcessInternal(m_RawData);
			OnPostProcess(m_RawData);
		}

		sealed public override void Validate(Raw.Sheet sheet)
		{
			HashSet<System.Type> dataTypes = Core.HashPool<System.Type>.Request();
			foreach (TData data in m_RawData)
			{
				System.Type type = data.GetType();
				Raw.ReflectionCache refCache = sheet.GetReflectionCache(type);

				if (!dataTypes.Contains(type))
				{
					dataTypes.Add(type);
					foreach (Raw.ReflectionCache.ValidateField validateField in refCache.Validation)
					{
						validateField.ValidateAt.ValidateAttribute(sheet, validateField);
					}
				}

				foreach (Raw.ReflectionCache.ValidateField validateField in refCache.Validation)
				{
					validateField.ValidateAt.ValidateData(data, sheet, validateField);
				}
				if (data is IDataValidate validate)
				{
					string error = validate.OnDataValidate();
					if (!string.IsNullOrEmpty(error))
					{
						DataValidationUtil.Raise(sheet.SheetName, data.ID, error);
					}
				}
			}
			// Validate that all types are used
			if (typeof(TData).Is(typeof(IDataGeneric)))
			{
				foreach (System.Type t in Core.TypeUtility.GetMatchingTypes(_EditorGenericDataBaseType, IsDerivedFrom))
				{
					if (!dataTypes.Contains(t))
					{
						DataValidationUtil.Raise(sheet.SheetName, $"Data Type {t.Name} was not found in data. Ensure all possible data types are used, include dummy data if required.");
					}
				}
			}
			Core.HashPool<System.Type>.Return(dataTypes);
		}

		public static bool IsDerivedFrom(System.Type type, System.Type baseType)
		{
			return !type.IsGenericTypeDefinition && !type.IsAbstract && type.IsSubclassOf(baseType);
		}

		protected internal abstract void InitializeInternal(IEnumerable<TDataStruct> rawData, int dataCount);

		protected virtual void OnInitialize() { }

		protected internal virtual void PostProcessInternal(TDataStruct rawData) { }

		protected virtual void OnPostProcess(TDataStruct rawData) { }
	}
}
