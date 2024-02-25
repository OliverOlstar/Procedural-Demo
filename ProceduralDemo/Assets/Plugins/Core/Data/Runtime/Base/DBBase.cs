
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	public abstract class DBBase : DBSO
	{
		public abstract void Initialize(List<DataSourceBase> sources);
		public abstract bool ImportFromSheet { get; }
		public abstract string SheetName { get; }
		public abstract System.Type DataType { get; }
		public abstract System.Type DataStructType { get; }
		public abstract void EditorImportData(Raw.Sheet sheet, out string error);
		public abstract void PostProcess(Raw.Sheet sheet);
		public abstract void Validate(Raw.Sheet sheet);
		public abstract string SerializeDefaultServerJson();

		public virtual void AddValidationDependencies(List<System.Type> dependencies) { }

		//
		// Editor properties/methods for tools so we have some way of interacting with DBs without knowing their type
		//
		/// <summary>Count of raw rows imported from Excel</summary>
		public abstract int EditorRawDataCount { get; }
		public abstract IEnumerable<IDataDictItem> GetEditorRawData();
		/// <summary>ID's to display in UberPicker or other tools, Collections for example use a 'Group ID' instead of the main one</summary>
		public abstract IEnumerable<string> GetEditorPickerIDs();
		/// <summary>Supports direct references to foreign keys, this will always return a 'row' of data from the excel sheet</summary>
		public abstract bool EditorTryGetData(string key, out IDataDictItem data);
		/// <summary>Supports validation of the 'primary key' this is typically the same as EditorTryGetData() except for certain DB types such as Collections</summary>
		public abstract bool EditorTryGetPrimaryKey(string key, out object data);
	}
}
