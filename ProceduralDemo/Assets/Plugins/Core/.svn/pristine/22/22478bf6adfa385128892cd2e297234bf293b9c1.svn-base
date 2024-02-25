using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Data
{
	[System.Serializable]
	public struct DBHash
	{
		public byte[] Hash;
		public string TypeName;
	}

	/// <summary> Loading a single scriptable object (SO) which references many other SOs from an asset bundle loads each of those referenced objects.
	/// This means loading many SOs which reference eachother is very slow. 
	/// This master data SO exists to ensure each data SO is only loaded once as a dependancy of a master data SO. </summary>
	public class DBMasterSO : ScriptableObject
	{
		public List<DBSO> m_DataSOs = new List<DBSO>();
		public List<DBHash> m_Hashes = new List<DBHash>();
		public IReadOnlyList<DBHash> Hashes => m_Hashes;

		public bool TryGet<T>(out T db) where T : DBSO
		{
			foreach (DBSO dbSO in m_DataSOs)
			{
				if (dbSO is T)
				{
					db = dbSO as T;
					return true;
				}
			}
			db = null;
			return false;
		}

		public void _EditorImport(IEnumerable<DBBase> dbList, IEnumerable<DataSourceBase> sourceList, IEnumerable<DBHash> hashes)
		{
			m_DataSOs.Clear();
			foreach (DBSO so in dbList)
			{
				m_DataSOs.Add(so);
			}
			foreach (DBSO so in sourceList)
			{
				m_DataSOs.Add(so);
			}
			m_DataSOs.Sort(CompareDB); // Make sure order is determinisitic

			m_Hashes.Clear();
			m_Hashes.AddRange(hashes);
			m_Hashes.Sort(CompareHash); // Make sure order is determinisitic
		}

		private int CompareDB(DBSO db1, DBSO db2)
		{
			return db1.name.CompareTo(db2.name);
		}

		private int CompareHash(DBHash db1, DBHash db2)
		{
			return db1.TypeName.CompareTo(db2.TypeName);
		}
	}
}
