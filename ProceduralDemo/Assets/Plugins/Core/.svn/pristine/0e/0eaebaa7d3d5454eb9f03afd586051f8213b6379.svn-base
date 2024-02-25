
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public static class DBManager
	{
		private static string s_Variant = DataVariant.Base.Name;
		public static string Variant => s_Variant;
		private static Dictionary<System.Type, DBBase> s_DataBases = new Dictionary<System.Type, DBBase>();
		private static System.Action<System.Type> s_EditorAccessAction = null;
		private static HashSet<System.Type> s_EditorDBsAccessed = new HashSet<System.Type>();
		public static HashSet<System.Type> EditorDBsAccessed => s_EditorDBsAccessed;
		// Note: Data is always available offline
		public static bool IsDataAvailable() { return (Application.isEditor && !Application.isPlaying) || s_DataBases.Count > 0; }

		public static bool TryGet<TDataBase>(out TDataBase dataBase) where TDataBase : DBBase
		{
			TryGet(typeof(TDataBase), out DBBase db);
			dataBase = db as TDataBase;
			return dataBase != null;
		}

		public static bool TryGet(System.Type dbType, out DBBase dataBase)
		{
			return Application.isEditor ? EditorTryGetDB(dbType, out dataBase) : s_DataBases.TryGetValue(dbType, out dataBase);
		}

		public static bool Exists(System.Type dbType)
		{
			return Application.isEditor ? EditorTryGetDB(dbType, out _) : s_DataBases.ContainsKey(dbType);
		}

		public static void Clear()
		{
			foreach (DBBase db in s_DataBases.Values)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(db);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(db);
				}
			}
			s_DataBases.Clear();
		}

		public static void Clear(System.Type dbType)
		{
			if (!s_DataBases.TryGetValue(dbType, out DBBase db))
			{
				return;
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(db);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(db);
			}
			s_DataBases.Remove(dbType);
		}

		public static void Clear<T>() where T : DBBase { Clear(typeof(T)); }

		public static void SetVariantAndClear(string variant)
		{
			s_Variant = variant;
			Clear();
		}

		public static void Initialize(string variant, IEnumerable<DBSO> dataSOs)
		{
			SetVariantAndClear(variant);
			List<DBBase> dbs = Core.ListPool<DBBase>.Request();
			List<DataSourceBase> srcs = Core.ListPool<DataSourceBase>.Request();
			foreach (ScriptableObject so in dataSOs)
			{
				if (so == null)
				{
					Core.DebugUtil.DevException($"DBManager.Initialize() Null data, a DB ScriptableObject is probably corrupted, ie. missing script reference");
				}
				else if (so is DBBase db)
				{
					dbs.Add(db);
				}
				else if (so is DataSourceBase src)
				{
					srcs.Add(src);
				}
				else
				{
					Core.DebugUtil.DevException($"DBManager.Initialize() {so.name} has invalid type {so.GetType()}");
				}
			}

			foreach (DBBase db in dbs)
			{
				AddDB(db, srcs);
			}
			Core.ListPool<DBBase>.Return(dbs);
			Core.ListPool<DataSourceBase>.Return(srcs);
		}

		private static void AddDB(DBBase db, List<DataSourceBase> srcs)
		{
			System.Type key = db.GetType();
			if (s_DataBases.ContainsKey(key))
			{
				Debug.LogError("DBManager.AddDB() Cannot load two DBs of the same type " + key);
				return;
			}
			DBBase instance = UnityEngine.Object.Instantiate(db);
			// Add to s_DataBases before calling Initialize() some DB's might try to access themselves through DBManager during Initialize()
			s_DataBases.Add(key, instance);
			// Not sure if this has any effect at runtime, but at edit time this stops DB's getting unloaded when changing scenes
			instance.hideFlags = HideFlags.DontSave;
			instance.Initialize(srcs);
		}

		public static void RegisterOnEditorAccessAction(System.Action<System.Type> action)
		{
			s_EditorAccessAction += action;
		}

		public static void DeregisterOnEditorAccessAction(System.Action<System.Type> action)
		{
			s_EditorAccessAction -= action;
		}

		public static void EditorClearDBsAccessed()
		{
			s_EditorDBsAccessed.Clear();
		}

		private static bool EditorTryInitializeDB(string path, out DBBase dataBase)
		{
#if UNITY_EDITOR
			string dir = System.IO.Path.GetDirectoryName(path);
			string variantDir = s_Variant.ToString();
			if (!dir.Contains(variantDir))
			{
				dataBase = null;
				return false;
			}
			// Note: Can't use AssetDatabaseUtil to load as variant Data DBs have the same name
			DBBase sourceDB = UnityEditor.AssetDatabase.LoadAssetAtPath<DBBase>(path);
			if (sourceDB == null)
			{
				dataBase = null;
				return false;
			}
			System.Type dbType = sourceDB.GetType();
			if (s_DataBases.ContainsKey(dbType))
			{
				dataBase = null;
				return false;
			}
			List<DataSourceBase> sources = Core.ListPool<DataSourceBase>.Request();
			string[] sourcePaths = Core.AssetDatabaseUtil.Find(typeof(DataSourceBase));
			foreach (string sourcePath in sourcePaths)
			{
				string sourceDir = System.IO.Path.GetDirectoryName(sourcePath);
				if (!sourceDir.Contains(variantDir))
				{
					continue;
				}
				// Note: Can't use AssetDatabaseUtil to load as variant Data Sources have the same name
				DataSourceBase source = UnityEditor.AssetDatabase.LoadAssetAtPath<DataSourceBase>(sourcePath);
				sources.Add(source);
			}
			AddDB(sourceDB, sources);
			Core.ListPool<DataSourceBase>.Return(sources);
			bool found = s_DataBases.TryGetValue(dbType, out dataBase);
			return found;
#else
			dataBase = null;
			return false;
#endif
		}

		private static void EditorAccessDB(System.Type dbType, DBBase dataBase)
		{
			if (s_EditorDBsAccessed.Contains(dbType))
			{
				return;
			}
			s_EditorAccessAction?.Invoke(dbType);
			s_EditorDBsAccessed.Add(dbType);
			if (dataBase is IDataCatalog catalog)
			{
				// All nested DB's count as accessed when we hit a catalog
				foreach (System.Type catalogDBType in catalog.GetCatalogDBTypes())
				{
					s_EditorAccessAction?.Invoke(catalogDBType);
					s_EditorDBsAccessed.Add(catalogDBType);
				}
			}
		}

		private static bool EditorTryGetDB(System.Type dbType, out DBBase dataBase)
		{
			if (s_DataBases.TryGetValue(dbType, out dataBase))
			{
				EditorAccessDB(dbType, dataBase);
				return true;
			}
#if UNITY_EDITOR
			string[] paths = Core.AssetDatabaseUtil.Find(dbType);
			foreach (string path in paths)
			{
				if (EditorTryInitializeDB(path, out dataBase))
				{
					EditorAccessDB(dbType, dataBase);
					return true;
				}
			}
#endif
			return false;
		}

		public static bool EditorTryGetData(System.Type dataType, string id, out IDataDictItem data)
		{
			foreach (DBBase db in s_DataBases.Values)
			{
				if (dataType.IsAssignableFrom(db.DataType) &&
					db.EditorTryGetData(id, out data))
				{
					return true;
				}
			}
#if UNITY_EDITOR
			string[] paths = Core.AssetDatabaseUtil.Find(dataType);
			if (paths.Length == 0)
			{
				data = null;
				return false;
			}
			foreach (string path in paths)
			{
				if (EditorTryInitializeDB(path, out DBBase dataBase) &&
					dataBase.EditorTryGetData(id, out data))
				{
					return true;
				}
			}
#endif
			data = null;
			return false;
		}
	}
}
