
using System.Collections.Generic;
using Data.Raw;

namespace Data
{
	public class DataImporterList2021<TData> : IDataImporter<List<TData>, TData>
		where TData : class, IDataDictItem
	{
		public List<TData> ImportRawData(DBBase dataBase, Sheet sheet, out string error)
		{
			error = null;
			return DataImportUtil2021.Import<TData>(dataBase, sheet, ref error);
		}
	}
}
