using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    public class StreamingBundlesManager : MonoBehaviour
    {
		private const string ENABLED_PREFS_KEY = "SBM_ENABLED";

        private static StreamingBundlesManager s_Instance;

        public static StreamingBundlesManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject singleton = new GameObject("StreamingAssetsManager", typeof(StreamingBundlesManager));
                    DontDestroyOnLoad(singleton);
                    s_Instance = singleton.GetComponent<StreamingBundlesManager>();
                }
                return s_Instance;
            }
        }

		public static bool Enabled
		{
			get
			{
#if !UNITY_EDITOR
				return true;
#else
				return EditorPrefs.GetBool(ENABLED_PREFS_KEY, false);
#endif
			}
			set
			{
#if UNITY_EDITOR
				EditorPrefs.SetBool(ENABLED_PREFS_KEY, value);
#endif
			}
		}
		public static bool IsReady
		{
			get
			{
				if (!Enabled)
				{
					return true;
				}
				// Just because we encountered an error doesn't mean we shouldn't count as finished
				return Instance.State == PreloadState.Finished || Instance.State == PreloadState.ErrorOnFinish;
			}
		}
		public static bool EnabledAndReady
		{
			get
			{
				return Enabled && IsReady;
			}
		}
		// This means StartPreloadingStreamingAssetBundles() has been called but it's not safe to access the manager yet
		public static bool InProgress
		{
			get
			{
				if (!Enabled)
				{
					return false;
				}
				return Instance.State == PreloadState.InProgress || Instance.State == PreloadState.ReadyToComplete;
			}
		}

		public enum PreloadState
        {
            NotStarted,
            InProgress,
			ReadyToComplete,
			ErrorOnStart,
			ErrorOnFinish,
            Finished,
        }
        
        public PreloadState State { get; private set; }

        private StreamingBundlesManifest m_Manifest;
		private Dictionary<string, byte[]> m_LoadedBundleBytes;
		private Dictionary<string, AssetBundle> m_LoadedBundles;
		private AssetBundleCatalogue m_Catalog;
        private HashSet<string> m_LoadingBundles;
        private HashSet<Action<bool>> m_OnStartedLoadingCompleteListeners;
		private HashSet<Action<bool>> m_OnFinishLoadingCompleteListeners;

        StreamingBundlesManager()
        {
            State = PreloadState.NotStarted;
			m_LoadedBundleBytes = new Dictionary<string, byte[]>();
            m_LoadedBundles = new Dictionary<string, AssetBundle>();
            m_LoadingBundles = new HashSet<string>();
			m_Catalog = new AssetBundleCatalogue();
            m_OnStartedLoadingCompleteListeners = new HashSet<Action<bool>>();
			m_OnFinishLoadingCompleteListeners = new HashSet<Action<bool>>();
        }

        public void StartPreloadingStreamingAssetBundles(Action<bool> onComplete)
        {
            if (onComplete != null)
            {
                m_OnStartedLoadingCompleteListeners.Add(onComplete);
            }
			switch (State)
			{
				case PreloadState.NotStarted:
					StartCoroutine(StartPreloadingStreamingAssetBundlesCoroutine());
					break;
				case PreloadState.Finished:
					InvokeOnStartCompleteListeners(true);
					break;
				case PreloadState.ErrorOnStart:
				case PreloadState.ErrorOnFinish:
					InvokeOnStartCompleteListeners(false);
					break;
				default:
				case PreloadState.InProgress:
				case PreloadState.ReadyToComplete:
					break;
			}
        }

		public void FinishPreloadingStreamingAssetBundles(Action<bool> onComplete)
		{
			if (onComplete != null)
			{
				m_OnFinishLoadingCompleteListeners.Add(onComplete);
			}
			switch (State)
			{
				case PreloadState.InProgress:
				case PreloadState.ReadyToComplete:
					StartCoroutine(FinishLoadingStreamingAssetBundlesCoroutine());
					break;
				case PreloadState.ErrorOnStart:
				case PreloadState.ErrorOnFinish:
					InvokeOnFinishCompleteListeners(false);
					break;
				case PreloadState.Finished:
					InvokeOnFinishCompleteListeners(true);
					break;
				case PreloadState.NotStarted:
				default:
					break;
			}
		}

        public void Restart()
        {
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("Restarting. Enabled: {0}", Enabled);
			State = PreloadState.NotStarted;
			m_Catalog.Clear();
			m_LoadedBundleBytes.Clear();
            UnloadStreamingAssetBundles();
        }

		public void UnloadStreamingBundleBytes(Action<bool> onComplete)
		{
			m_LoadedBundleBytes.Clear();
			CompleteFinishLoading(true);
		}

        public void UnloadStreamingAssetBundles()
        {
			foreach (var kvp in m_LoadedBundles)
            {
                kvp.Value.Unload(true);
            }
            m_LoadedBundles.Clear();
        }

		internal bool IsBundleLoaded(string bundleName)
        {
			if (InProgress)
			{
				LogError("Trying to check if bundle {0} is loaded but manager is not ready", bundleName);
				return false;
			}
			return m_LoadedBundles.ContainsKey(bundleName);
        }

		internal string GetBundleNameForAsset(string assetName)
        {
			m_Catalog.TryGetBundleName(assetName, out string bundleName);
			return bundleName;
        }

        internal bool IsAssetLoaded(string assetName)
        {
			return m_Catalog.ContainsAsset(assetName);
        }

        internal void UnloadBundle(string bundleName)
        {
			AssetBundle bundle = null;
            if (m_LoadedBundles.TryGetValue(bundleName, out bundle))
            {
                bundle.Unload(false);
                m_LoadedBundles.Remove(bundleName);
            }
        }

		internal AssetBundle GetBundle(string bundleName)
        {
			AssetBundle bundle = null;
            m_LoadedBundles.TryGetValue(bundleName, out bundle);
            return bundle;
        }

		internal T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
			if (InProgress)
			{
				LogError("Trying to load {0} from {1} but manager is not ready yet", assetName, bundleName);
				return null;
			}
            AssetBundle bundle = null;
            if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
            {
                return null;
            }
            return bundle.LoadAsset<T>(assetName);
        }

		internal T[] LoadAllAssets<T>(string bundleName) where T : UnityEngine.Object
		{
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("Loading all assets from bundle {0}", bundleName);
			if (InProgress)
			{
				LogError("Trying to load all assets from {1} but manager is not ready yet", bundleName);
				return null;
			}
			AssetBundle bundle = null;
			if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
			{
				return null;
			}
			return bundle.LoadAllAssets<T>();
		}

		internal BundleAssetHandle<T> LoadAssetAsync<T>(string bundleName, string assetName, string fallbackAssetName = null) where T : UnityEngine.Object
        {
			if (InProgress)
			{
				LogError("Trying to load {0} from {1} but manager is not ready yet", assetName, bundleName);
				return null;
			}
			AssetBundle bundle = null;
            if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
            {
                return null;
            }
            BundleAssetHandle<T> handle = new BundleAssetHandle<T>(null, ABM.INVALID_REF, bundleName, assetName, fallbackAssetName);
            StartCoroutine(LoadAssetCoroutine(handle, bundle, assetName, fallbackAssetName));
            return handle;
        }

		internal BundleAssetHandleAsync<T> LoadAssetAsyncSlow<T>(string bundleName, string assetName) where T : UnityEngine.Object
		{
			if (InProgress)
			{
				LogError("Trying to load {0} from {1} but manager is not ready yet", assetName, bundleName);
				return null;
			}
			AssetBundle bundle = null;
			if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
			{
				return null;
			}
			BundleAssetHandleAsync<T> handle = new BundleAssetHandleAsync<T>(null, ABM.INVALID_REF, bundleName, assetName);
			StartCoroutine(LoadAssetSlowCoroutine(handle, bundle, assetName));
			return handle;
		}

		internal BundleBinaryHandle<T> LoadBinaryAssetAsync<T>(string bundleName, string assetName)
		{
			if (InProgress)
			{
				LogError("Is not ready");
				return null;
			}
			AssetBundle bundle = null;
			if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
			{
				return null;
			}
			BundleBinaryHandle<T> handle = new BundleBinaryHandle<T>(null, ABM.INVALID_REF, bundleName, assetName);
			StartCoroutine(LoadBinaryAssetCoroutine(handle, bundle, assetName));
			return handle;
		}

        internal BundleLevelHandle LoadSceneAsync(string bundleName, string sceneName, bool additive, bool setActive)
        {
			if (InProgress)
			{
				LogError("Trying to load {0} from {1} but manager is not ready yet", sceneName, bundleName);
				return null;
			}
			AssetBundle bundle = null;
            if (!m_LoadedBundles.TryGetValue(bundleName, out bundle))
            {
                return null;
            }
            BundleLevelHandle handle = new BundleLevelHandle(null, bundleName, sceneName, additive, setActive);
            StartCoroutine(LoadSceneCoroutine(handle, bundle, sceneName, additive));
            return handle;
        }

        public string GetAssetPath(string assetName)
        {
            return Str.Build(Application.streamingAssetsPath, "/AssetBundles/", assetName);
        }

		public bool TryGetBundleName(string assetName, out string bundleName)
		{
			if (InProgress)
			{
				LogError("Trying to find bundle name for asset {0} but manager is not ready ", assetName);
				bundleName = string.Empty;
				return false;
			}
			return m_Catalog.TryGetBundleName(assetName, out bundleName);
		}

		public bool TryGetBundleNameWithFallback(string assetName, string fallbackAssetName, out string bundleName)
		{
			if (InProgress)
			{
				LogError("Trying to find bundle name for asset {0} but manager is not ready ", assetName);
				bundleName = string.Empty;
				return false;
			}
			return m_Catalog.TryGetBundleName(assetName, out bundleName) ||
				(!string.IsNullOrEmpty(fallbackAssetName) && m_Catalog.TryGetBundleName(fallbackAssetName, out bundleName));
		}

		public bool TryGetAssetNamesInBundle(string bundleName, out IReadOnlyList<string> assetNames)
		{
			if (InProgress)
			{
				LogError("Trying to find assets names in in bundle {0} but manager is not ready ", bundleName);
				assetNames = default;
				return false;
			}
			return m_Catalog.TryGetAssetNames(bundleName, out assetNames);
		}

		private IEnumerator StartPreloadingStreamingAssetBundlesCoroutine()
        {
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("START PRELOADING STREAMING ASSET BUNDLES");
			State = PreloadState.InProgress;
			if (AssetBundleUtil.IsSimMode())
			{
				if (Core.DebugOptions.ABMLogs.IsSet()) Log("Nah, we're actually in simulation mode!");
				CompleteStartLoading(true);
				yield break;
			}
            yield return StartCoroutine(LoadManifestCoroutine());
            if (m_Manifest == null)
            {
                CompleteStartLoading(false);
                yield break;
            }
            List<TaskManager.Task> tasks = new List<TaskManager.Task>();
            foreach (string bundleName in m_Manifest.GetBundleNames())
            {
				if (Core.DebugOptions.ABMLogs.IsSet()) Log("LOADING BUNDLE FROM STREAMING ASSETS: {0}", bundleName);
                var task = TaskManager.Instance.CreateTask(StartLoadingBundleCoroutine(bundleName));
                task.Start();
                tasks.Add(task);
			}
			yield return TaskManager.Instance.WaitForTasks(tasks);
			CompleteStartLoading(true);
        }

		private IEnumerator FinishLoadingStreamingAssetBundlesCoroutine()
		{
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("FINISH LOADING STREAMING ASSET BUNDLES");
			if (AssetBundleUtil.IsSimMode())
			{
				CompleteFinishLoading(true);
				yield break;
			}
			yield return new WaitUntil(() => State == PreloadState.ReadyToComplete);
			List<TaskManager.Task> tasks = new List<TaskManager.Task>();
			List<string> keys = new List<string>(m_LoadedBundleBytes.Keys);
			for (int i = 0; i < keys.Count; ++i)
			{
				string key = keys[i];
				var task = TaskManager.Instance.CreateTask(FinishLoadingBundleCoroutine(key));
				task.Start();
				tasks.Add(task);
			}
			yield return TaskManager.Instance.WaitForTasks(tasks);
			CompleteFinishLoading(true);
		}

        private IEnumerator LoadManifestCoroutine()
        {
            byte[] bytes = null;
            yield return StartCoroutine(LoadFileCoroutine(StreamingBundlesManifest.MANIFEST_NAME, (result) => bytes = result));
            if (bytes.Length > 0)
            {
                string json = Encoding.UTF8.GetString(bytes);
				m_Manifest = StreamingBundlesManifest.FromJson(json);
            }
            yield break;
        }

		private IEnumerator StartLoadingBundleCoroutine(string bundleName)
		{
			if (m_LoadedBundles.ContainsKey(bundleName) || m_LoadingBundles.Contains(bundleName))
			{
				yield break;
			}
			m_LoadingBundles.Add(bundleName);
			byte[] bytes = null;
			yield return StartCoroutine(LoadFileCoroutine(GetBundleNameWithPlatform(bundleName), (result) => bytes = result));
			if (bytes.Length > 0)
			{
				m_LoadedBundleBytes.Add(bundleName, bytes);
			}
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("LOADED BUNDLE FROM STREAMING ASSETS: {0}", bundleName);
			yield break;
		}

		private IEnumerator FinishLoadingBundleCoroutine(string bundleName)
		{
			byte[] bytes = null;
			if (!m_LoadedBundleBytes.TryGetValue(bundleName, out bytes))
			{
				LogWarning("Tried finishing a bundle load for {0} but no bytes exist for it", bundleName);
				yield break;
			}
			if (ABMs.IsBundleLoaded(bundleName)) // The bundle was already download so we don't need to load it from here
			{
				m_LoadedBundleBytes.Remove(bundleName);
				m_LoadingBundles.Remove(bundleName);
				if (Core.DebugOptions.ABMLogs.IsSet()) Log("FINISHED LOADING BUNDLE FROM STREAMING ASSETS: {0}", bundleName);
				yield break;
			}
			AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(bytes);
			yield return request;
			if (request.assetBundle == null)
			{
				LogError("Couldn't load bundle with name {0}", bundleName);
				yield break;
			}
			HashSet<string> fileNames = new HashSet<string>();
			string[] assetNames = request.assetBundle.GetAllAssetNames();
			for (int i = 0; i < assetNames.Length; ++i)
			{
				string fileName = Path.GetFileNameWithoutExtension(assetNames[i]);
				fileNames.Add(fileName);
				if (m_Catalog.ContainsAsset(fileName))
				{
					LogWarning("Catalog already contains asset name {0}, skipping", fileName);
					continue;
				}
				m_Catalog.Add(fileName, request.assetBundle.name);
			}
			string[] scenePaths = request.assetBundle.GetAllScenePaths();
			for (int i = 0; i < scenePaths.Length; ++i)
			{
				string fileName = Path.GetFileNameWithoutExtension(scenePaths[i]);
				fileNames.Add(fileName);
				if (m_Catalog.ContainsAsset(fileName))
				{
					LogWarning("Catalog already contains asset name {0}, skipping", fileName);
					continue;
				}
				m_Catalog.Add(fileName, request.assetBundle.name);
			}
			m_LoadingBundles.Remove(bundleName);
			m_LoadedBundleBytes.Remove(bundleName);
			m_LoadedBundles.Add(bundleName, request.assetBundle);
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("FINISHED LOADING BUNDLE FROM STREAMING ASSETS: {0}", bundleName);
			yield break;
		}

		private IEnumerator LoadFileCoroutine(string fileName, Action<byte[]> onComplete)
		{
			string filePath = GetAssetPath(fileName);
			if (Core.DebugOptions.ABMLogs.IsSet()) Log("Loading file at path {0}", fileName);
			byte[] result = new byte[0];
			if (Str.Contains(filePath, "://"))
			{
				UnityWebRequest request = UnityWebRequest.Get(filePath);
				yield return request.SendWebRequest();
				if (Str.IsEmpty(request.error))
				{
					result = request.downloadHandler.data;
				}
				else
				{
					LogError("Could not load file vial UnityWebRequest from path {0}", filePath);
				}
				request.Dispose();
			}
			else
			{
				if (File.Exists(filePath))
				{
					result = File.ReadAllBytes(filePath);
				}
				else
				{
					LogError("Could not load file via File.ReadAllBytes from path {0}", filePath);
				}
			}
			if (onComplete != null)
			{
				onComplete(result);
			}
		}

		private IEnumerator LoadAssetCoroutine<T>(BundleAssetHandle<T> handle, AssetBundle bundle, string assetName, string fallbackAssetName = null) where T : UnityEngine.Object
		{
			AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
			while (!request.isDone)
			{
				yield return null;
			}
			if (request.asset == null && !string.IsNullOrEmpty(fallbackAssetName))
			{
				request = bundle.LoadAssetAsync<T>(fallbackAssetName);
				while (!request.isDone)
				{
					yield return null;
				}
			}
			LoadAssetOperation.ReflectionFuckery(handle, request);
		}

		private IEnumerator LoadAssetSlowCoroutine<T>(BundleAssetHandleAsync<T> handle, AssetBundle bundle, string assetName) where T : UnityEngine.Object
		{
			AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
			while (!request.isDone)
			{
				yield return null;
			}
			LoadAssetOperation.ReflectionFuckery(handle, request);
			yield break;
		}

		private IEnumerator LoadBinaryAssetCoroutine<T>(BundleBinaryHandle<T> handle, AssetBundle bundle, string assetName)
		{
			AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
			while (!request.isDone)
			{
				yield return null;
			}
			LoadBinaryAssetOperation.ReflectionFuckery<T>(handle);
			yield break;
		}

        private IEnumerator LoadSceneCoroutine(BundleLevelHandle handle, AssetBundle bundle, string sceneName, bool additive)
		{
			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			while (!operation.isDone)
            {
                yield return null;
            }
			LoadSceneOperation.ReflectionFuckery(handle, operation);
            yield break;
        }

		private string GetBundleNameWithPlatform(string bundleName)
		{
#if UNITY_IOS
			return Str.Build("iOS/", bundleName);
#elif UNITY_ANDROID
			return Str.Build("Android/", bundleName);
#else
			return Str.Build("Windows/", bundleName);
#endif
		}

		private void CompleteStartLoading(bool success)
		{
			State = success ? PreloadState.ReadyToComplete : PreloadState.ErrorOnStart;
			InvokeOnStartCompleteListeners(success);
			m_OnStartedLoadingCompleteListeners.Clear();
			m_LoadingBundles.Clear();
		}

		private void CompleteFinishLoading(bool success)
		{
			State = success ? PreloadState.Finished : PreloadState.ErrorOnFinish;
			InvokeOnFinishCompleteListeners(success);
			m_OnFinishLoadingCompleteListeners.Clear();
			m_LoadedBundleBytes.Clear();
		}

		private void InvokeOnStartCompleteListeners(bool success)
		{
			foreach (Action<bool> listener in m_OnStartedLoadingCompleteListeners)
			{
				if (listener != null)
				{
					listener.Invoke(success);
				}
			}
		}

		private void InvokeOnFinishCompleteListeners(bool success)
		{
			foreach (Action<bool> listener in m_OnFinishLoadingCompleteListeners)
			{
				if (listener != null)
				{
					listener.Invoke(success);
				}
			}
		}

		private void Log(string message, params object[] parameters)
		{
			string formattedMessage = string.Format(message, parameters);
			Debug.LogFormat("<color=maroon>[StreamingBundlesManager] {0}</color>", formattedMessage);
		}

		private void LogWarning(string message, params object[] parameters)
		{
			string formattedMessage = string.Format(message, parameters);
			Debug.LogWarningFormat("<color=maroon>[StreamingBundlesManager] {0}</color>", formattedMessage);
		}

		private void LogError(string message, params object[] parameters)
		{
			string formattedMessage = string.Format(message, parameters);
			Debug.LogErrorFormat("<color=maroon>[StreamingBundlesManager] {0}</color>", formattedMessage);
		}

		private class LoadAssetOperation
		{
			private const string OBJECT_FIELD_NAME = "m_Object";
			private const string DONE_FIELD_NAME = "m_Done";

			private static Dictionary<Type, FieldInfo> s_ObjectInfo;
			private static Dictionary<Type, FieldInfo> s_DoneInfo;

			static LoadAssetOperation()
			{
				s_ObjectInfo = new Dictionary<Type, FieldInfo>();
				s_DoneInfo = new Dictionary<Type, FieldInfo>();
			}

			public static void ReflectionFuckery(BaseBundleHandle handle, AssetBundleRequest request)
			{
				Type handleType = handle.GetType();
				if (!s_ObjectInfo.TryGetValue(handleType, out FieldInfo objectInfo))
				{
					objectInfo = handleType.GetField(OBJECT_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
					s_ObjectInfo.Add(handleType, objectInfo);
				}
				objectInfo.SetValue(handle, request.asset);
				if (!s_DoneInfo.TryGetValue(handleType, out FieldInfo doneInfo))
				{
					doneInfo = handleType.GetField(DONE_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
					s_DoneInfo.Add(handleType, doneInfo);
				}
				doneInfo.SetValue(handle, true);
			}
		}

        private class LoadSceneOperation
        {
			private const string REQUEST_FIELD_NAME = "m_Request";

			private static FieldInfo s_RequestInfo;

			static LoadSceneOperation()
			{
				s_RequestInfo = typeof(BundleLevelHandle).GetField(REQUEST_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
			}

            public static void ReflectionFuckery(BundleLevelHandle handle, AsyncOperation operation)
            {
				s_RequestInfo.SetValue(handle, operation);
            }
        }

		private class LoadBinaryAssetOperation
		{
			private const string BIN_OBJECT_FIELD_NAME = "m_BinObject";

			private static Dictionary<Type, FieldInfo> s_BinObjectInfo;

			static LoadBinaryAssetOperation()
			{
				s_BinObjectInfo = new Dictionary<Type, FieldInfo>();
			}

			public static void ReflectionFuckery<T>(BundleBinaryHandle<T> handle)
			{
				Type handleType = handle.GetType();
				if (!s_BinObjectInfo.TryGetValue(handleType, out FieldInfo binObjectInfo))
				{
					binObjectInfo = handleType.GetField(BIN_OBJECT_FIELD_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
					s_BinObjectInfo.Add(handleType, binObjectInfo);
				}
				TextAsset asset = handle.GetAsset();
				T value = AssetBundleUtil.Deserialize<T>(asset);
				binObjectInfo.SetValue(handle, value);
			}
		}
	}
}
