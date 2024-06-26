
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Core
{
	public static class EditorUtil
	{
		[MenuItem("Core/Search Scene By Layer")]
		static void SearchByLayer()
		{
			Dictionary<int, List<GameObject>> objectsByLayer = new();

			foreach (GameObject gameObject in Object.FindObjectsOfType<GameObject>())
			{
				if (!objectsByLayer.ContainsKey(gameObject.layer))
				{
					objectsByLayer.Add(gameObject.layer, new List<GameObject>());
				}
				objectsByLayer[gameObject.layer].Add(gameObject);
			}

			foreach (int layer in objectsByLayer.Keys)
			{
				string log = "layer: " + layer + "-" + LayerMask.LayerToName(layer);
				foreach (GameObject gameObject in objectsByLayer[layer])
				{
					log += "\n" + DebugUtil.GetScenePath(gameObject);
				}
				Debug.Log(log);
			}
		}

#if UNITY_EDITOR_WIN
		[MenuItem("Core/Mesh Combiner/Combine Selection")]
#else
		[MenuItem("Core/Mesh Combiner/Mac Combine Selection")]
#endif
		static void CombineSelection()
		{
			try
			{
				foreach (Transform transform in Selection.transforms)
				{
					string folder = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
					folder = folder.Remove(folder.LastIndexOf('.'));
					folder += "/CombinedMeshes/" + transform.name + "/";
					if (!Directory.Exists(folder))
					{
						Debug.Log("Created folder " + folder + ".");
						Directory.CreateDirectory(folder);
					}
					Debug.Log(folder);

					List<Mesh> meshes = MeshCombiner.CombineAllChildMeshes(transform);
					for (int i = 0; i < meshes.Count; i++)
					{
						float percent = (float)i / (float)(meshes.Count - 1.0f);
						EditorUtility.DisplayProgressBar(
							"Mesh Combiner",
							"Creating combined mesh assets. " + i + "/" + (meshes.Count - 1) + " complete.\n" + meshes[i].name,
							percent);

						Mesh mesh = meshes[i];
						AssetDatabase.CreateAsset(mesh, folder + mesh.name + ".asset");
						Debug.Log("Created Asset " + folder + mesh.name + ".asset");
					}
				}
				EditorUtility.ClearProgressBar();
			}
			catch (System.Exception e)
			{
				EditorUtility.ClearProgressBar();
				throw (e);
			}
		}

#if UNITY_EDITOR_WIN
		[MenuItem("Core/Time Manager/Pause &-")] // alt + -
#else
		[MenuItem("Core/Time Manager/Mac Pause _F1")] // alt + -
#endif
		static void EditorPause()
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}

#if UNITY_EDITOR_WIN
		[MenuItem("Core/Time Manager/Step &=")] // alt + =
#else
		[MenuItem("Core/Time Manager/Mac Step _F2")]
#endif
		static void EditorTimeStep()
		{
			EditorApplication.Step();
		}

#if UNITY_EDITOR_WIN
		[MenuItem("Core/Time Manager/Increment &]")]
#else
		[MenuItem("Core/Time Manager/Mac Increment _F4")]
#endif
		static void EditorTimeInc()
		{
			TimeScaleManager tem = TimeScaleManager.Get();
			if (tem == null)
			{
				GameObject obj = new("TimeScaleManager");
				tem = obj.AddComponent<TimeScaleManager>();
			}
			tem.EditorInc();
		}

#if UNITY_EDITOR_WIN
		[MenuItem("Core/Time Manager/Decrement &[")] // alt + [
#else
		[MenuItem("Core/Time Manager/Mac Decrement _F3")]
#endif
		static void EditorTimeDec()
		{
			TimeScaleManager tem = TimeScaleManager.Get();
			if (tem == null)
			{
				GameObject obj = new("TimeScaleManager");
				tem = obj.AddComponent<TimeScaleManager>();
			}
			tem.EditorDec();
		}

		//	This makes it easy to create, name and place unique new ScriptableObject asset files.
		public static T CreateAsset<T>() where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T>();

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

			AssetDatabase.CreateAsset(asset, assetPathAndName);

			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			return asset;
		}

		public delegate void ApplyOrRevert(GameObject _goCurrentGo, Object _ObjPrefabParent);
		[MenuItem("Core/Selection/Apply all selected prefabs")]
		static void ApplyPrefabs()
		{
			SearchPrefabConnections(ApplyToSelectedPrefabs);
		}

		[MenuItem("Core/Selection/Revert all selected prefabs")]
		static void ResetPrefabs()
		{
			SearchPrefabConnections(RevertToSelectedPrefabs);
		}

		//Look for connections
		static void SearchPrefabConnections(ApplyOrRevert _applyOrRevert)
		{
			GameObject[] tSelection = Selection.gameObjects;

			if (tSelection.Length > 0)
			{
				GameObject goPrefabRoot;
				GameObject goCur;
				bool bTopHierarchyFound;
				int iCount = 0;
				PrefabInstanceStatus prefabType;
				bool bCanApply;
				//Iterate through all the selected gameobjects
				foreach (GameObject go in tSelection)
				{
					prefabType = PrefabUtility.GetPrefabInstanceStatus(go);
					//Is the selected gameobject a prefab?
#pragma warning disable CS0618 // PrefabInstanceStatus.Disconnected obsolete in 2022, need to find an alternate solution if we decide to rewrite
					if (prefabType == PrefabInstanceStatus.Connected || prefabType == PrefabInstanceStatus.Disconnected)
#pragma warning disable
					{
						//Prefab Root;
						goPrefabRoot = ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(go)).transform.root.gameObject;
						goCur = go;
						bTopHierarchyFound = false;
						bCanApply = true;
						//We go up in the hierarchy to apply the root of the go to the prefab
						while (goCur.transform.parent != null && !bTopHierarchyFound)
						{
							//Are we still in the same prefab?
							if (goPrefabRoot == ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(goCur.transform.parent.gameObject)).transform.root.gameObject)
							{
								goCur = goCur.transform.parent.gameObject;
							}
							else
							{
								//The gameobject parent is another prefab, we stop here
								bTopHierarchyFound = true;
								if (goPrefabRoot != ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(goCur)))
								{
									//Gameobject is part of another prefab
									bCanApply = false;
								}
							}
						}

						if (_applyOrRevert != null && bCanApply)
						{
							iCount++;
							_applyOrRevert(goCur, PrefabUtility.GetCorrespondingObjectFromSource(goCur));
						}
					}
				}
				Debug.Log(iCount + " prefab" + (iCount > 1 ? "s" : "") + " updated");
			}
		}

		//Apply      
		static void ApplyToSelectedPrefabs(GameObject _goCurrentGo, Object _ObjPrefabParent)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			PrefabUtility.ReplacePrefab(_goCurrentGo, _ObjPrefabParent, ReplacePrefabOptions.ConnectToPrefab);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		//Revert
		static void RevertToSelectedPrefabs(GameObject _goCurrentGo, Object _ObjPrefabParent)
		{
			PrefabUtility.RevertPrefabInstance(_goCurrentGo, InteractionMode.UserAction);
		}

		[MenuItem("Core/Profiling/Find Materials")]
		static void CountMaterials()
		{
			Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
			List<Material> materials = new List<Material>(renderers.Length);
			List<int> counts = new List<int>(renderers.Length);
			foreach (Renderer r in renderers)
			{
				//Debug.Log(r.name);
				foreach (Material m in r.sharedMaterials)
				{
					if (m == null)
					{
						continue;
					}
					bool unique = true;
					for (int i = 0; i < materials.Count; i++)
					{
						if (m.GetInstanceID() == materials[i].GetInstanceID())
						{
							unique = false;
							counts[i]++;
							break;
						}
					}
					if (unique)
					{
						materials.Add(m);
						counts.Add(1);
					}
				}
			}

			for (int i = 0; i < materials.Count; i++)
			{
				Debug.Log(AssetDatabase.GetAssetPath(materials[i].GetInstanceID()) + " references: " + counts[i]);
			}
			Debug.Log("Found " + materials.Count + " unique materials.");
		}

		[MenuItem("Core/Profiling/Find Meshes")]
		static void CountMeshes2()
		{
			MeshFilter[] filters = GameObject.FindObjectsOfType<MeshFilter>();
			List<Mesh> meshes = new List<Mesh>(filters.Length);
			foreach (MeshFilter f in filters)
			{
				//Debug.Log(r.name);
				if (f.sharedMesh == null)
				{
					continue;
				}
				bool unique = true;
				for (int i = 0; i < meshes.Count; i++)
				{
					if (f.sharedMesh.GetInstanceID() == meshes[i].GetInstanceID())
					{
						unique = false;
						break;
					}
				}
				if (unique)
				{
					meshes.Add(f.sharedMesh);
				}
			}

			meshes = meshes.OrderBy(o => o.triangles.Length).ToList();
			for (int i = 0; i < meshes.Count; i++)
			{
				Debug.Log(AssetDatabase.GetAssetPath(meshes[i].GetInstanceID()) + " " + meshes[i].name + " tris: " + (meshes[i].triangles.Length / 3));
			}
			Debug.Log("Found " + meshes.Count + " unique meshes.");
		}

		[MenuItem("Core/Profiling/Triangle Counts Per Mesh")]
		static void CountMeshes()
		{
			MeshFilter[] filters = GameObject.FindObjectsOfType<MeshFilter>();
			List<Mesh> meshes = new List<Mesh>(filters.Length);
			List<int> counts = new List<int>(filters.Length);
			foreach (MeshFilter f in filters)
			{
				//Debug.Log(r.name);
				if (f.sharedMesh == null)
				{
					continue;
				}
				bool unique = true;
				for (int i = 0; i < meshes.Count; i++)
				{
					if (f.sharedMesh.GetInstanceID() == meshes[i].GetInstanceID())
					{
						unique = false;
						counts[i]++;
						break;
					}
				}
				if (unique)
				{
					meshes.Add(f.sharedMesh);
					counts.Add(1);
				}
			}

			int average = 0;
			for (int i = 0; i < meshes.Count; i++)
			{
				average += counts[i] * meshes[i].triangles.Length;
			}
			average /= counts.Count;

			for (int i = 0; i < meshes.Count; i++)
			{
				int tris = counts[i] * meshes[i].triangles.Length;
				if (tris < average)
				{
					continue;
				}
				Debug.Log(
					AssetDatabase.GetAssetPath(meshes[i].GetInstanceID()) + " " +
					meshes[i].name + " placed: " +
					counts[i] + " total tris: " +
					(tris / 3));
			}
			Debug.Log("Found " + meshes.Count + " unique meshes. Average total tris: " + (average / 3));
		}

		public static ulong EstimateTextureSize(Texture2D tex)
		{
			return EstimateTextureSize(tex, AssetDatabase.GetAssetPath(tex));
		}
		public static ulong EstimateTextureSize(string path)
		{
			Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			if (tex == null)
			{
				return 0;
			}
			return EstimateTextureSize(tex, path);
		}
		public static ulong EstimateTextureSize(Texture2D tex, string path)
		{
			int pixels = Mathf.RoundToInt(tex.width * tex.height);
			TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			bool squaredPOT = tex.width == tex.height && Mathf.IsPowerOfTwo(tex.width);
#pragma warning disable CS0618 // spritePackingTag obsolete in 2022, need to find an alternate solution if we decide to rewrite
			bool atlassed = !string.IsNullOrEmpty(texImporter.spritePackingTag);
#pragma warning restore CS0618
			bool compressed = (atlassed || squaredPOT) &&
				texImporter.textureCompression != TextureImporterCompression.Uncompressed;
			bool hasAlpha = texImporter.DoesSourceTextureHaveAlpha() &&
				texImporter.alphaSource == TextureImporterAlphaSource.FromInput;
			int bitsPerPixel = compressed ?
				(hasAlpha ? 8 : 4) :
				(hasAlpha ? 32 : 24);
			ulong size = (ulong)(pixels * bitsPerPixel) / 8;
			return size;
		}

#region Scripting Defines
		private static Dictionary<BuildTargetGroup, List<string>> s_Defines = new Dictionary<BuildTargetGroup, List<string>>();
		public static BuildTargetGroup GetActiveBuildTargetGroup() => BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

		public static void SaveScriptingDefinesForGroup(BuildTargetGroup targetGroup)
		{
			if (!s_Defines.ContainsKey(targetGroup))
			{
				return; // Never modified
			}
			Debug.Log("saving defines before: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup));
			string defines = string.Join(";", s_Defines[targetGroup]);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
			s_Defines.Remove(targetGroup);
			Debug.Log("saving defines after: " + PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup));
		}
		public static void SaveScriptingDefinesForActiveGroup()
			=> SaveScriptingDefinesForGroup(GetActiveBuildTargetGroup());

		#region Scripting Defines Getters
		public static List<string> GetScriptingDefinesForGroup(BuildTargetGroup targetGroup)
		{
			if (s_Defines.TryGetValue(targetGroup, out List<string> defines))
			{
				return defines;
			}
			return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').ToList();
		}

		// Check if scripting define is added wether it is saved or not
		public static bool ScriptingDefinesForGroupContains(BuildTargetGroup targetGroup, in string define)
		{
			return GetScriptingDefinesForGroup(targetGroup).Contains(define);
		}
		public static bool ScriptingDefinesForActiveGroupContains(in string define)
			=> ScriptingDefinesForGroupContains(GetActiveBuildTargetGroup(), define);
		#endregion Scripting Defines Getters

		#region Scripting Defines Modify
		public static void SetScriptingDefinesForGroup(BuildTargetGroup targetGroup, List<string> defines)
		{
			if (s_Defines.ContainsKey(targetGroup))
			{
				s_Defines[targetGroup] = defines;
				return;
			}
			s_Defines.Add(targetGroup, defines);
		}
		public static void SetScriptingDefinesForGroup(BuildTargetGroup targetGroup, string defines)
			=> SetScriptingDefinesForGroup(targetGroup, defines.Split(';').ToList());

		public static bool AddScriptingDefineForGroup(BuildTargetGroup targetGroup, in string define)
		{
			List<string> defines = GetScriptingDefinesForGroup(targetGroup);
			if (defines.Contains(define))
			{
				return false;
			}
			defines.Add(define);
			SetScriptingDefinesForGroup(targetGroup, defines);
			return true;
		}
		public static bool AddScriptingDefineForActiveGroup(in string define)
			=> AddScriptingDefineForGroup(GetActiveBuildTargetGroup(), define);

		public static bool RemoveScriptingDefineForGroup(BuildTargetGroup targetGroup, in string define)
		{
			List<string> defines = GetScriptingDefinesForGroup(targetGroup);
			bool removedDefine = false;
			for (int i = 0; i < defines.Count; i++)
			{
				if (defines[i].Equals(define))
				{
					defines.RemoveAt(i);
					removedDefine = true;
					break;
				}
			}
			if (removedDefine)
			{
				SetScriptingDefinesForGroup(targetGroup, defines);
			}
			return removedDefine;
		}
		public static bool RemoveScriptingDefineForActiveGroup(in string define)
			=> RemoveScriptingDefineForGroup(GetActiveBuildTargetGroup(), define);
		#endregion Scripting Defines Modify
#endregion Scripting Defines
	}
}