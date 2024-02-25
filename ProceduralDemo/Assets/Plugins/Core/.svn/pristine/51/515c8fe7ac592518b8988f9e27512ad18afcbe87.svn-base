using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.IO;

namespace Data
{
	public class DBHashes : Dictionary<string, DBHash>
	{
		public void Fill(IEnumerable<DBBase> dbList)
		{
			foreach (DBBase db in dbList)
			{
				string path = AssetDatabase.GetAssetPath(db);
				string key = db.GetType().Name;
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					HashAlgorithm sha2 = HashAlgorithm.Create("SHA256");
					byte[] hash = sha2.ComputeHash(stream);
					if (TryGetValue(key, out DBHash dbCache))
					{
						dbCache.Hash = hash;
						this[key] = dbCache;
					}
					else
					{
						Add(key, new DBHash { TypeName = key, Hash = hash, });
					}
				}
			}
		}

		public static DBHashes Create(IEnumerable<DBBase> dbList)
		{
			DBHashes pendingCache = new DBHashes();
			pendingCache.Fill(dbList);
			return pendingCache;
		}
	}
}
