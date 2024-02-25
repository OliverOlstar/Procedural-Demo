
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
	public abstract class BaseBundleHandle
	{
		protected ABM m_Manager = null;

		public BaseBundleHandle(ABM manager)
		{
			m_Manager = manager;
		}

		public virtual void Update() {}

		public abstract bool IsDone();

		public virtual bool IsPersistent() { return false; }

		public virtual  void Unload() {}
	}

	public class BundleLevelHandle : BaseBundleHandle
	{
		protected bool m_SetActive = false;
		protected bool m_IsAdditive = false;
		protected string m_LevelName = string.Empty;
		protected string m_AssetBundleName = string.Empty;
		protected AsyncOperation m_Request = null;

		public BundleLevelHandle(ABM manager, string assetbundleName, string levelName, bool isAdditive, bool setActive) : base(manager)
		{
			m_SetActive = setActive;
			m_LevelName = levelName;
			m_IsAdditive = isAdditive;
			m_AssetBundleName = assetbundleName;

#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded += SceneLoaded;
#else
			SceneManager.sceneLoaded += SceneLoaded;
#endif
		}

		private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			if (!m_SetActive || !arg0.name.Equals(m_LevelName))
			{
				return;
			}

#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(m_LevelName));
			UnityEditor.SceneManagement.EditorSceneManager.sceneLoaded -= SceneLoaded;
#else
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(m_LevelName));
			SceneManager.sceneLoaded -= SceneLoaded;
#endif
		}

		public override void Update()
		{
			if (m_Request != null)
			{
				return;
			}

#if UNITY_EDITOR
			if (m_Manager.IsSimulationMode())
			{
				string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(m_AssetBundleName, m_LevelName);
				if (levelPaths.Length > 0)
				{
					if (m_IsAdditive)
					{
						m_Request = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
							levelPaths[0], new LoadSceneParameters(LoadSceneMode.Additive));
					}
					else
					{
						m_Request = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(levelPaths[0], 
							new LoadSceneParameters(LoadSceneMode.Single));
					}
				}
				else
				{
					Debug.LogError("There is no scene with name \"" + m_LevelName + "\" in " + m_AssetBundleName);
				}
			}
			else
#endif
			{
				LoadedAssetBundle bundle = m_Manager.GetLoadedAssetBundle(m_AssetBundleName);
				if (bundle != null)
				{
					m_Request = SceneManager.LoadSceneAsync(m_LevelName, m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
				}
			}
		}

		public override bool IsDone()
		{
			return m_Request != null && m_Request.isDone;
		}

		public override string ToString()
		{
			string s = m_AssetBundleName + "-LoadLevel: ";
			if (m_Request == null)
			{
				s += "Waiting";
				return s;
			}
			if (m_Request != null && m_Request.isDone)
			{
				s += "Done";
				return s;
			}
			return s + m_Request.progress.ToString("0.0");
		}
	}

	public abstract class BundlAssetHandleBase<T> : BaseBundleHandle where T : UnityEngine.Object
	{
		protected string m_AssetBundleName = Core.Str.EMPTY;
		public string GetBundleName() => m_AssetBundleName;
		protected string m_AssetName = Core.Str.EMPTY;
		public string GetAssetName() => m_AssetName;
		protected string m_FallbackAssetName = null;
		protected T m_Object = null;
		protected int m_BundleReferenceHandle = ABM.INVALID_REF;
		private bool m_Done = false;
		private bool m_Unloaded = false;

		public T GetAsset() { return m_Object; }
		public bool IsUnloaded() { return m_Unloaded; }
		public override bool IsDone() { return m_Done || m_Unloaded; }
		public override bool IsPersistent() { return m_Manager.IsPersistent(m_AssetBundleName); }
		protected void Done() { m_Done = true; }

		public BundlAssetHandleBase(
			ABM manager, 
			int referencehandle, 
			string bundleName, 
			string assetName, 
			string fallbackAssetName) : base(manager)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				Debug.LogError(Core.Str.Build(GetType().ToString() + "() Asset name cannot be null or empty"));
			}
			if (string.IsNullOrEmpty(bundleName))
			{
				Debug.LogError(Core.Str.Build(GetType().ToString() + "() Bundle name cannot be null or empty"));
			}
			// If an invalid handle is passed in take that to mean we should be persistent
			m_BundleReferenceHandle = referencehandle;

			m_AssetBundleName = bundleName;
			m_AssetName = assetName;
			m_FallbackAssetName = fallbackAssetName;
		}

		public override void Unload()
		{
			m_Unloaded = true;
			if (m_Manager != null)
			{
				m_Manager.UnloadBundle(m_AssetBundleName, m_BundleReferenceHandle);
			}
		}

		public override void Update()
		{
			if (m_Done)
			{
				return;
			}

#if UNITY_EDITOR
			if (m_Manager.IsSimulationMode())
			{
				m_Object = AssetBundleUtil.LoadAssetEditor<T>(m_AssetBundleName, m_AssetName, m_FallbackAssetName);
				if (m_Object == null)
				{
					Debug.LogError(this.ToString() + " BundleAssetHandle() Failed to load asset \"" + m_AssetName + "\" of type " + typeof(T));
				}
				m_Done = true;
				return;
			}
#endif

			LoadedAssetBundle bundle = m_Manager.GetLoadedAssetBundle(m_AssetBundleName);
			if (bundle != null)
			{
				LoadedUpdate(bundle);
			}
		}

		protected abstract void LoadedUpdate(LoadedAssetBundle bundle);
	}

	public class BundleAssetHandle<T> : BundlAssetHandleBase<T> where T : UnityEngine.Object
	{
		public BundleAssetHandle(ABM manager, int referencehandle, string bundleName, string assetName, string fallbackAssetName = "") : 
			base(manager, referencehandle, bundleName, assetName, fallbackAssetName)
		{
		}

		protected override void LoadedUpdate(LoadedAssetBundle bundle)
		{
			bool hasFallback = !Core.Str.IsEmpty(m_FallbackAssetName);
			m_Object = bundle.Load<T>(m_AssetName, hasFallback); // Tell bundle not to throw errors if we have a fallback
			if (m_Object == null && hasFallback)
			{
				// Try loading fallback if one was provided
				m_Object = bundle.Load<T>(m_FallbackAssetName);
			}
			if (m_Object == null)
			{
				Debug.LogError(this.ToString() + " BundleAssetHandle.Update() Bundle " + m_AssetBundleName
					+ " does not contain " + m_AssetName + " or fallback " + m_FallbackAssetName);
			}
			Done();
		}

		public override string ToString()
		{
			return m_AssetBundleName + "." + m_AssetName + "-Unpack: ";
		}
	}

	public class BundleAssetHandleAsync<T> : BundlAssetHandleBase<T> where T : UnityEngine.Object
	{
		public BundleAssetHandleAsync(ABM manager, int referencehandle, string bundleName, string assetName) : 
			base(manager, referencehandle, bundleName, assetName, "")
		{
		}

		protected override void LoadedUpdate(LoadedAssetBundle bundle)
		{
			bool error = false;
			m_Object = bundle.LoadAsync<T>(m_AssetName, out error);
			if (m_Object != null)
			{
				Done();
			}
			else if (error)
			{
				Debug.LogError(this.ToString() + " BundleAssetHandleAsync.Update() Failed to load " + m_AssetName + " from " + m_AssetBundleName);
				Done();
			}
		}

		public override string ToString()
		{
			return m_AssetBundleName + "." + m_AssetName + "-UnpackAsync: ";
		}
	}

	public class BundleListHandle : BaseBundleHandle
	{
		List<string> m_BundleNames = null;

		bool m_Done = false;

		public BundleListHandle(ABM manager, List<string> bundleNames) : base(manager)
		{
			m_BundleNames = new List<string>(bundleNames); // Duplicate the list to be safe, we're going to modify it
		}
		
		public override void Update()
		{
			if (m_Done)
			{
				return;
			}

#if UNITY_EDITOR
			if (m_Manager.IsSimulationMode())
			{
				m_Done = true;
				return;
			}
#endif

			for (int i = 0; i < m_BundleNames.Count; i++)
			{
				if (m_Manager.IsAssetBundleLoaded(m_BundleNames[i]))
				{
					m_BundleNames.RemoveAt(i);
					i--;
				}
			}

			m_Done = m_BundleNames.Count == 0;
		}

		public override bool IsDone()
		{
			return m_Done;
		}

		public override string ToString()
		{
			string s = Core.Str.EMPTY;
			foreach (string bundleName in m_BundleNames)
			{
				s += bundleName + ", ";
			}
			return s;
		}
	}

	public class BundleBinaryHandle<T> : BundleAssetHandle<TextAsset>
	{
		protected T m_BinObject = default(T);
		public T GetBinObject() { return m_BinObject; }

		public BundleBinaryHandle(ABM manager, int referenceHandle, string bundleName, string assetName) : 
			base(manager, referenceHandle, bundleName, assetName) {}

		public override void Update()
		{
			base.Update();
			if (IsDone() && m_BinObject == null && GetAsset() != null)
			{
				m_BinObject = AssetBundleUtil.Deserialize<T>(GetAsset());
			}
		}
	}

//	public abstract class BundleAssetsHandle<T> : BaseBundleHandle where T : UnityEngine.Object
//	{
//		public abstract T[] GetAssets();
//	}
//
//	public class BundleAssetsHandleSim<T> : BundleAssetsHandle<T> where T : UnityEngine.Object
//	{
//		T[] m_Assets;
//
//		public BundleAssetsHandleSim(T[] assets)
//		{
//			m_Assets = assets;
//		}
//
//		public override T[] GetAssets()
//		{
//			return m_Assets;
//		}
//
//		public override bool IsDone()
//		{
//			return true;
//		}
//	}
//
//	public class BundleAssetsHandleReal<T> : BundleAssetsHandle<T> where T : UnityEngine.Object
//	{
//		protected string m_AssetBundleName = string.Empty;
//		protected T[] m_Objects = null;
//		protected int m_BundleReferenceHandle = ABM.INVALID_REF;
//		bool m_Done = false;
//
//		public BundleAssetsHandleReal(string bundleName)
//		{
//			m_AssetBundleName = bundleName;
//		}
//
//		public override T[] GetAssets()
//		{
//			if (m_Objects == null)
//			{
//				Debug.LogError(this.ToString() + " AssetBundleLoadUnpackAllAssetsOperation.GetAsset() Failed asset is null");
//				return null;
//			}
//			return m_Objects;
//		}
//
//		// Returns true if more Update calls are required.
//		public override void Update()
//		{
//			if (m_Done)
//			{
//				return;
//			}
//
//			LoadedAssetBundle bundle = m_Manager.GetLoadedAssetBundle(m_AssetBundleName);
//			if (bundle != null)
//			{
//				m_Objects = bundle.LoadAll<T>();
//				Debug.Assert(m_Objects != null && m_Objects.Length > 0,
//					"AssetBundleLoadUnpackAssetOperation.Update() Bundle " + m_AssetBundleName
//					+ " does not contain any objects of type " + typeof(T));
//				m_Done = true;
//			}
//		}
//
//		public override bool IsDone()
//		{
//			return m_Done;
//		}
//
//		public override void Unload()
//		{
//			m_Done = true;
//			m_Manager.UnloadBundle(m_AssetBundleName, m_BundleReferenceHandle);
//		}
//
//		public override string ToString()
//		{
//			return m_AssetBundleName + ".all-Unpack: ";
//		}
//	}
}
