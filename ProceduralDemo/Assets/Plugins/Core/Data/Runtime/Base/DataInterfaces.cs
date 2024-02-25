
using System.Collections.Generic;

namespace Data
{
	public interface IDataImporter<TDataStruct, TData>
		where TDataStruct : IEnumerable<TData>
		where TData : IDataDictItem
	{
		TDataStruct ImportRawData(DBBase db, Raw.Sheet sheet, out string error);
	}

	public interface IDataDictItem
	{
		string ID { get; }
		System.Type Type { get; }
	}

	public interface IDataCollectionItem { string CollectionID { get; } }

	public interface IDataGeneric
	{
		System.Type TypeNameToClassName(string type, string sheet);
	}

	public interface IDataPreImport { void OnDataPreImport(); }

	public interface IDataPostImport { void OnDataPostImport(); }

	public interface IDataValidate { string OnDataValidate(); }

	public interface IOverrideExcelToJsonSerialization { string OnSerializeServerJson(); }
}
