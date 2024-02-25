using UnityEngine;

namespace Data.Validate.String
{
	public class AssetExistsAttribute : StringAttribute
	{
		private System.Type[] m_Types = default;
		private string[] m_Paths = null;

		public AssetExistsAttribute(System.Type[] assetTypes, bool allowBlanks = false, string columnNameToDisplayInErrors = null, params string[] paths) :
			base(allowBlanks, columnNameToDisplayInErrors)
		{
			m_Types = assetTypes;
			m_Paths = paths;
		}

		protected override string ValidateStringValue(string value)
		{
#if UNITY_EDITOR
			if (m_Paths == null || m_Paths.Length <= 0)
			{
				bool exists = Core.AssetDatabaseUtil.Exists(value, m_Types);
				return exists ? null : $"Could not find asset of type {m_Types}";
			}

			foreach (string path in m_Paths)
			{
				string[] assetPaths = Core.AssetDatabaseUtil.FindAll(value, m_Types);
				foreach (string assetPath in assetPaths)
				{
					if (assetPath.StartsWith(path))
					{
						return null;
					}
				}
			}
			return $"Could not find asset of type {m_Types} at any required path:{string.Join("\n", m_Paths)}";
#else
			return null;
#endif
		}
	}

	public class PrefabExistsWithComponentAttribute : StringAttribute
	{
		private System.Type m_Type = default;
		private string m_Path = null;

		public PrefabExistsWithComponentAttribute(System.Type componentType, bool allowBlanks = false, string path = null, string columnNameToDisplayInErrors = null) : 
			base(allowBlanks, columnNameToDisplayInErrors)
		{
			if (!componentType.IsInterface && !componentType.Is<Component>())
			{
				throw new System.ArgumentException($"componentType must be an interface or derive from the UnityEngine.Component class");
			}
			m_Type = componentType;
			m_Path = path;
		}

		protected override string ValidateStringValue(string value)
		{
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(m_Path))
			{
				GameObject prefab = Core.AssetDatabaseUtil.Load<GameObject>(value);
				if (!prefab)
				{
					return $"Could not find a Prefab asset named {value}";
				}
#if UNITY_2019_2_OR_NEWER
				if (!prefab.TryGetComponent(m_Type, out _))
				{
					return $"Could not find Component of type {m_Type} on Prefab named {value}";
				}
#else
				Component component = prefab.GetComponent(m_Type);
				if (!component)
				{
					return $"Could not find Component of type {m_Type} on Prefab named {value}";
				}
#endif
				return null;
			}
			else
			{
				string path = Core.AssetDatabaseUtil.Find(value, typeof(GameObject));
				if (string.IsNullOrEmpty(path))
				{
					return $"Could not find Prefab asset named {value}";
				}
				if (!path.StartsWith(m_Path))
				{
					return $"{path} is not at required path {m_Path}";
				}
				GameObject prefab = Core.AssetDatabaseUtil.Load<GameObject>(value);
				if (!prefab)
				{
					return $"Could not find a Prefab asset named {value}";
				}
#if UNITY_2019_2_OR_NEWER
				if (!prefab.TryGetComponent(m_Type, out _))
				{
					return $"Could not find Component of type {m_Type} on Prefab named {value}";
				}
#else
				Component component = prefab.GetComponent(m_Type);
				if (!component)
				{
					return $"Could not find Component of type {m_Type} on Prefab named {value}";
				}
#endif
				return null;
			}
#else
			return null;
#endif
		}
	}

	public class ForeignKeyAttribute : StringAttribute
	{
		private System.Type[] m_Type = null;

		/// <summary>
		/// Search for an ID if 'dbType' is a standard DataBase's or a CollectionID if 'dbType' is a DataBaseWithCollection
		/// </summary>
		/// <param name="columnNameToDisplayInErrors">Column name to display to designers when there is an error.
		/// When null this name will be derived from the member variable</param>
		public ForeignKeyAttribute(System.Type dbType, bool allowBlanks = false, string columnNameToDisplayInErrors = null) :
			base(allowBlanks, columnNameToDisplayInErrors)
		{
			m_Type = new System.Type[] { dbType };
		}

		/// <summary>
		/// Use this constructor if you need to search more than one db
		/// </summary>
		/// <param name="columnNameToDisplayInErrors">Column name to display to designers when there is an error.
		/// When null this name will be derived from the member variable</param>
		/// <param name="dbTypes">DB's to search</param>
		public ForeignKeyAttribute(bool allowBlanks, string columnNameToDisplayInErrors = null, params System.Type[] dbTypes) :
			base(allowBlanks, columnNameToDisplayInErrors)
		{
			m_Type = dbTypes;
		}

		protected override string OnValidateAttribute()
		{
			if (m_Type == null || m_Type.Length == 0)
			{
				return "Data.Validate.StringForeignKey() Must be provided at least one valid DB type, make sure this attribute is set up properly in code";
			}
			foreach (System.Type type in m_Type)
			{
				if (!DBManager.Exists(type))
				{
					return Core.Str.Build("Data base type ", type.Name, " does not exist");
				}
			}
			return null;
		}

		protected override string ValidateStringValue(string value)
		{
			foreach (System.Type type in m_Type)
			{
				if (!DBManager.TryGet(type, out DBBase db))
				{
					return Core.Str.Build("Data base type ", type.Name, " does not exist");
				}
				if (db.EditorTryGetPrimaryKey(value, out _))
				{
					return null;
				}
			}
			Core.Str.Add("Could not find foreign key in ", m_Type[0].Name);
			for (int i = 1; i < m_Type.Length; i++)
			{
				Core.Str.Add(" or ", m_Type[i].Name);
			}
			return Core.Str.Finish();
		}
	}

	public class ExistsInCatalogAttribute : StringAttribute
	{
		private System.Type m_CatalogType = null;
		private System.Type m_DataType = null;

		public ExistsInCatalogAttribute(System.Type catalogType, System.Type dataType = null, bool allowBlanks = false, string columnNameToDisplayInErrors = null) :
			base(allowBlanks, columnNameToDisplayInErrors)
		{
			m_CatalogType = catalogType;
			m_DataType = dataType;
		}

		protected override string OnValidateAttribute()
		{
			if (m_CatalogType == null)
			{
				return "Data.Validate.ExistsInCatalog() Catalog Type cannot be null";
			}
			if (!DBManager.Exists(m_CatalogType))
			{
				return Core.Str.Build("Catalog of type '", m_CatalogType.Name, "' does not exist");
			}
			return null;
		}

		protected override string ValidateStringValue(string value)
		{
			if (!DBManager.TryGet(m_CatalogType, out DBBase db))
			{
				return Core.Str.Build("Catalog of type '", m_CatalogType.Name, "' does not exist");
			}
			IDataCatalog catalog = db as IDataCatalog;
			if (catalog == null)
			{
				return $"Data.Validate.ExistsInCatalog() Catalog Type '{catalog.GetType().Name}' must implement IDataCatalog interface";
			}
			if (!catalog.TryGetValueOfAnyTypeFromCatalog(value, out IDataDictItem data))
			{
				return $"ID '{value}' does not exist in Catalog '{db.SheetName}'";
			}
			if (m_DataType != null)
			{
				if (!catalog.CatalogDataType.IsAssignableFrom(m_DataType))
				{
					return $"Data.Validate.ExistsInCatalog() Data Type '{m_DataType.Name}' must be derrived from Catalog '{catalog.GetType().Name}'s data type '{catalog.CatalogDataType.Name}'";
				}
				if (!m_DataType.IsAssignableFrom(data.GetType()))
				{
					return $"Catalog must contain data of type '{m_DataType.Name}' but instead found data with type '{data.GetType()}'";
				}
			}
			return null;
		}
	}

	public class NonEmptyAttribute : StringAttribute
	{
		public NonEmptyAttribute(string columnNameToDisplayInErrors = null) : base(false, columnNameToDisplayInErrors) { }

		protected override string ValidateStringValue(string value) => null;
	}
}