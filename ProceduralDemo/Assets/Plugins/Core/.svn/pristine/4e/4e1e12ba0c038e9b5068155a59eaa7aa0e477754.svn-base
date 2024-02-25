
using UnityEngine;
using System.Collections.Generic;

namespace Core
{
	public class LoadedAssetBundle
	{
		string m_AssetBundleName = Core.Str.EMPTY;
		public string GetBundleName() { return m_AssetBundleName; }
		AssetBundle m_AssetBundle = null;
		public AssetBundle GetAssetBundle() { return m_AssetBundle; }
		Dictionary<string, UnityEngine.Object> m_Objects = null;
		public IEnumerable<UnityEngine.Object> GetObjects() { return m_Objects.Values; }
		HashSet<string> m_RequestNames = null;
		List<KeyValuePair<string, AssetBundleRequest>> m_RequestList = null;

		public LoadedAssetBundle(AssetBundle bundle)
		{
			if (bundle == null)
			{
				Debug.LogError("ABM.LoadedAssetBundle() Bundle should never be null");
			}
			m_AssetBundle = bundle;
			m_AssetBundleName = bundle.name;
			int assetCount = m_AssetBundle.GetAllAssetNames().Length;
			m_Objects = new Dictionary<string, UnityEngine.Object>(assetCount);
			m_RequestNames = new HashSet<string>();
			m_RequestList = new List<KeyValuePair<string, AssetBundleRequest>>(assetCount);
		}

		public T Load<T>(string assetName, bool safe = false) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(assetName))
			{
				Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Load() Null or empty asset name requested from bundle ", m_AssetBundleName));
				return null;
			}
			if (m_AssetBundle == null)
			{
				if (!safe)
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Load() ", m_AssetBundleName, " not loaded"));
				}
				return null;
			}

			UnityEngine.Object obj = null;
			if (!m_Objects.TryGetValue(assetName, out obj))
			{
				T asset = m_AssetBundle.LoadAsset<T>(assetName);
				if (asset != null)
				{
					m_Objects.Add(assetName, asset);
				}
				else if (!safe)
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Load() ", m_AssetBundleName,
					 " did not contain asset \"", assetName, "\" of type ", typeof(T).ToString()));
				}
				return asset;
			}

			if (!typeof(T).IsAssignableFrom(obj.GetType()))
			{
				if (!safe)
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Load() ", m_AssetBundleName,
						" cannot covert ", obj.GetType().ToString(), " to ", typeof(T).ToString()));
				}
				return null;
			}

			return (T)obj;
		}

		public T LoadAsync<T>(string assetName, out bool error) where T : UnityEngine.Object
		{
			error = false;
			if (m_AssetBundle == null)
			{
				Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.LoadAsync() ", m_AssetBundleName, " not loaded"));
				error = true;
				return null;
			}

			UnityEngine.Object obj = null;
			if (m_Objects.TryGetValue(assetName, out obj))
			{
				if (!typeof(T).IsAssignableFrom(obj.GetType()))
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.LoadAsync() ", m_AssetBundleName,
					 " cannot covert ", obj.GetType().ToString(), " to ", typeof(T).ToString()));
					error = true;
					return null;
				}
				return (T)obj;
			}

			if (!m_RequestNames.Contains(assetName))
			{
				AssetBundleRequest req = m_AssetBundle.LoadAssetAsync<T>(assetName);
				if (req == null)
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.LoadAsync() ", m_AssetBundleName, 
						" did not contain asset \"", assetName, "\" of type ", typeof(T).ToString()));
					error = true;
				}
				else
				{
					m_RequestNames.Add(assetName); // Hash set makes sure we only request an asset once
					m_RequestList.Add(new KeyValuePair<string, AssetBundleRequest>(assetName, req)); // List tracks async operations
				}
			}
			else if (m_RequestList.Count == 0)
			{
				// We've already requested this asset and the operation has finished but the object wasn't cached
				Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.LoadAsync() ", m_AssetBundleName,
					" Bundle did not contain asset \"", assetName, "\" of type ", typeof(T).ToString()));
				error = true;
			}
			return null;
		}

		public void Update()
		{
			for (int i = 0; i < m_RequestList.Count; i++)
			{
				KeyValuePair<string, AssetBundleRequest> req = m_RequestList[i];
				if (!req.Value.isDone)
				{
					continue;
				}
				UnityEngine.Object obj = req.Value.asset;
				if (obj != null)
				{
					// TODO: Test what happens if Load() is called while a request is in progress
					// the object should already be added to the list?
					m_Objects.Add(req.Key, obj);
				}
				else
				{
					Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Update() ", m_AssetBundleName, " asset is null"));
				}
				m_RequestList.RemoveAt(i);
				i--;
			}
		}

		protected T Deserialize<T>(string assetName)
		{
			if (m_AssetBundle == null)
			{
				return default(T);
			}

			TextAsset text = Load<TextAsset>(assetName);
			if (text == null)
			{
				return default(T);
			}

			return AssetBundleUtil.Deserialize<T>(text);
		}

		public T[] LoadAll<T>() where T : UnityEngine.Object
		{
			if (m_AssetBundle == null)
			{
				return null;
			}
			T[] assets = m_AssetBundle.LoadAllAssets<T>();
			foreach (T asset in assets)
			{
				if (!m_Objects.ContainsKey(asset.name))
				{
					m_Objects.Add(asset.name, asset);
				}
			}
			return assets;
		}

		public T[] LoadAssetWithSubAssets<T>(string name) where T : UnityEngine.Object
		{
			if (m_AssetBundle == null)
			{
				return null;
			}
			T[] assets = m_AssetBundle.LoadAssetWithSubAssets<T>(name);
			foreach (T asset in assets)
			{
				if (!m_Objects.ContainsKey(asset.name))
				{
					m_Objects.Add(asset.name, asset);
				}
			}
			return assets;
		}

		// Can't unload a bundle while we are unpacking assets from it
		public bool CanUnload()
		{
			return m_RequestList.Count == 0;
		}

		public void Unload()
		{
			if (!CanUnload())
			{
				Debug.LogError(Core.Str.Build("ABM.LoadedAssetBundle.Unload() ", m_AssetBundleName,
					" cannot unload bundle while unpacking assets, need to check CanUnload() first"));
				return;
			}
            if (m_AssetBundle != null)
            {
				m_Objects.Clear();
                m_AssetBundle.Unload(true);
            }
            else
            {
				Debug.LogWarning(Core.Str.Build("ABM.LoadedAssetBundle.Unload() ", m_AssetBundleName, " unloading asset bundle while it is null"));
            }
		}
	}
}
