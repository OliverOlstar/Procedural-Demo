/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.
 
This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder.
N.B. there may be a bug so if the custom option doesn't come up refer to this thread http://answers.unity3d.com/questions/317951/how-to-use-editorobjexporter-obj-saving-script-fro.html */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEditor.SceneManagement;

namespace Core
{
	struct ObjMaterial
	{
		public string name;
		public string textureName;
	}

	public static class EditorObjExporter
	{
		static readonly string FOLDER_PATH_KEY = "OBJFolderPath";
		static readonly string DEFAULT_FOLDER_PATH = "Assets/Exported";

		private static int vertexOffset = 0;
		private static int normalOffset = 0;
		private static int uvOffset = 0;

		//User should probably be able to change this. It is currently left as an excercise for
		//the reader.

		static string BuildHeirarchyString(Transform transform)
		{
			if (transform.parent == null)
			{
				return transform.gameObject.name;
			}

			return BuildHeirarchyString(transform.parent) + "." + transform.gameObject.name;
        }

        private static string SkinnedMeshRendererToString(SkinnedMeshRenderer smr, Dictionary<string, ObjMaterial> materialList, bool exportMats)
        {
            Mesh m = smr.sharedMesh;

            if (m == null)
            {
                Debug.LogError(DebugUtil.GetScenePath(smr.gameObject) + "." + "EditorObjExporter.MeshToString: Mesh is null!!!");
                return string.Empty;
            }

            Material[] mats = smr.sharedMaterials;

            return MeshToString(m, smr.transform, mats, materialList, exportMats);
        }

        private static string MeshFilterToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList, bool exportMats, bool useLightmapUVs)
        {
            Mesh m = mf.sharedMesh;

            if (m == null)
            {
                Debug.LogError(DebugUtil.GetScenePath(mf.gameObject) + "." + "EditorObjExporter.MeshToString: Mesh is null!!!");
                return string.Empty;
            }

            if (useLightmapUVs && m.vertices.Length != m.uv2.Length)
            {
                Debug.Log("Skipped " + BuildHeirarchyString(mf.transform));
                return string.Empty;
            }

            Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

            Vector2[] uv2 = null;
            if (useLightmapUVs)
            {
                uv2 = new Vector2[m.uv2.Length];
                Renderer renderer = mf.GetComponent<Renderer>();
                for (int i = 0; i < uv2.Length; i++)
                {
                    uv2[i] = new Vector2(
                        (m.uv2[i].x * renderer.lightmapScaleOffset.x) + renderer.lightmapScaleOffset.z,
                        (m.uv2[i].y * renderer.lightmapScaleOffset.y) + renderer.lightmapScaleOffset.w);
                }
            }

            return MeshToString(m, mf.transform, mats, materialList, exportMats, uv2);
        }

        private static string MeshToString(Mesh m, Transform t, Material[] mats, Dictionary<string, ObjMaterial> materialList, bool exportMats, Vector2[] uv2 = null)
		{
			StringBuilder sb = new();

			int count = 0;
			sb.Append("g ").Append(t.name).Append("\n");
			foreach (Vector3 lv in m.vertices)
			{
				count++;
				Vector3 wv = t.TransformPoint(lv);

				//This is sort of ugly - inverting x-component since we're in
				//a different coordinate system than "everyone" is "used to".
				sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
			}

			sb.Append("\n");

			foreach (Vector3 lv in m.normals)
			{
				Vector3 wv = t.TransformDirection(lv);

				sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
			}
			sb.Append("\n");

            if (uv2 == null)
            {
                foreach (Vector3 v in m.uv)
                {
                    sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
                }
			}
			else
            {
                foreach (Vector3 v in uv2)
                {
                    sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
                }
			}

			for (int material = 0; material < m.subMeshCount; material++)
			{
				sb.Append("\n");
				if (exportMats)
				{
					sb.Append("usemtl ").Append(mats[material].name).Append("\n");
					sb.Append("usemap ").Append(mats[material].name).Append("\n");

					//See if this material is already in the materiallist.
					try
					{
						ObjMaterial objMaterial = new();

						objMaterial.name = mats[material].name;

						if (mats[material].mainTexture)
						{
							objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
						}
						else
						{
							objMaterial.textureName = null;
						}

						materialList.Add(objMaterial.name, objMaterial);
					}
					catch (ArgumentException)
					{
						//Already in the dictionary
					}
				}
				else
				{
					sb.Append("usemtl ").Append("Default-Material").Append("\n");
					sb.Append("usemap ").Append("Default-Material").Append("\n");
				}
				
				int[] triangles = m.GetTriangles(material);
				for (int i = 0; i < triangles.Length; i += 3)
				{
					//Because we inverted the x-component, we also needed to alter the triangle winding.
					sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
											triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
				}
			}

			vertexOffset += m.vertices.Length;
			normalOffset += m.normals.Length;
			uvOffset += m.uv.Length;

			return sb.ToString();
		}

		private static void Clear()
		{
			vertexOffset = 0;
			normalOffset = 0;
			uvOffset = 0;
		}

		private static Dictionary<string, ObjMaterial> PrepareFileWrite()
		{
			Clear();

			return new Dictionary<string, ObjMaterial>();
		}

		private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
		{
			using (StreamWriter sw = new(folder + "/" + filename + ".mtl"))
			{
				foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
				{
					sw.Write("\n");
					sw.Write("newmtl {0}\n", kvp.Key);
					sw.Write("Ka  0.6 0.6 0.6\n");
					sw.Write("Kd  0.6 0.6 0.6\n");
					sw.Write("Ks  0.9 0.9 0.9\n");
					sw.Write("d  1.0\n");
					sw.Write("Ns  0.0\n");
					sw.Write("illum 2\n");

					if (kvp.Value.textureName != null)
					{
						string destinationFile = kvp.Value.textureName;


						int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;

						if (stripIndex >= 0)
						{
							destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
						}

						string relativeFile = destinationFile;

						destinationFile = folder + "/" + destinationFile;

						Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

						try
						{
							//Copy the source file
							File.Copy(kvp.Value.textureName, destinationFile);
						}
						catch
						{

						}


						sw.Write("map_Kd {0}", relativeFile);
					}

					sw.Write("\n\n\n");
				}
			}
        }

        private static void SkinnedMeshRendererToFile(SkinnedMeshRenderer smr, string folder, string filename, bool exportMats)
        {
            Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
            MeshStringToFile(
                SkinnedMeshRendererToString(smr, materialList, exportMats), 
                materialList,
                folder,
                filename,
                exportMats);
        }

        private static void SkinnedMeshRenderersToFile(SkinnedMeshRenderer[] smr, string folder, string filename, bool exportMats)
        {
            Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
            string[] meshStrings = new string[smr.Length];
            for (int i = 0; i < meshStrings.Length; i++)
            {
                meshStrings[i] = SkinnedMeshRendererToString(smr[i], materialList, exportMats);
            }
            MeshStringsToFile(
                meshStrings, 
                materialList,
                folder,
                filename,
                exportMats);
        }

        private static void MeshFilterToFile(MeshFilter mf, string folder, string filename, bool exportMats, bool useLightmapUVs)
		{
			Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
            MeshStringToFile(
                MeshFilterToString(mf, materialList, useLightmapUVs, exportMats), 
                materialList,
                folder,
                filename,
                exportMats);
		}

		private static void MeshFiltersToFile(MeshFilter[] mf, string folder, string filename, bool exportMats, bool useLightmapUVs)
		{
			Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
            string[] meshStrings = new string[mf.Length];
            for (int i = 0; i < meshStrings.Length; i++)
            {
                meshStrings[i] = MeshFilterToString(mf[i], materialList, useLightmapUVs, exportMats);
            }
            MeshStringsToFile(
                meshStrings, 
                materialList,
                folder,
                filename,
                exportMats);
        }

        private static void SkinnedMeshRenderersAndMeshFiltersToFile(SkinnedMeshRenderer[] smr, MeshFilter[] mf, string folder, string filename, bool exportMats, bool useLightmapUVs)
        {
            Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
            string[] meshStrings = new string[smr.Length + mf.Length];
            for (int i = 0; i < smr.Length; i++)
            {
                meshStrings[i] = SkinnedMeshRendererToString(smr[i], materialList, exportMats);
            }
            for (int i = 0; i < mf.Length; i++)
            {
                meshStrings[smr.Length + i] = MeshFilterToString(mf[i], materialList, useLightmapUVs, exportMats);
            }
            MeshStringsToFile(
                meshStrings, 
                materialList,
                folder,
                filename,
                exportMats);
        }

        private static void MeshStringToFile(string meshString, Dictionary<string, ObjMaterial> materialList, string folder, string filename, bool exportMats)
        {
            string filePath = folder + "/" + filename + ".obj";
            using (StreamWriter sw = new(filePath))
            {
                if (exportMats)
				{
					sw.Write("mtllib ./" + filename + ".mtl\n");
				}

				sw.Write(meshString);
            }

            if (exportMats)
			{
				MaterialsToFile(materialList, folder, filename);
			}

			AssetDatabase.Refresh();
        }

        private static void MeshStringsToFile(string[] meshStrings, Dictionary<string, ObjMaterial> materialList, string folder, string filename, bool exportMats)
        {
            string filePath = folder + "/" + filename + ".obj";
            using (StreamWriter sw = new(filePath))
            {
                if (exportMats)
				{
					sw.Write("mtllib ./" + filename + ".mtl\n");
				}

				for (int i = 0; i < meshStrings.Length; i++)
                {
                    sw.Write(meshStrings[i]);
                }
            }

            if (exportMats)
			{
				MaterialsToFile(materialList, folder, filename);
			}

			AssetDatabase.Refresh();
        }

		private static bool CreateTargetFolder()
		{
			try
			{
				Directory.CreateDirectory(EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH));
			}
			catch
			{
				EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "ok");
				return false;
			}

			return true;
		}

		[MenuItem("Core/Export/Change destination folder")]
		static void ChangeTargetFolder()
		{
			if (!CreateTargetFolder())
			{
				return;
			}

			string folder = EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH);
			if (!Directory.Exists(folder))
			{
				folder = DEFAULT_FOLDER_PATH;
			}
           folder =  EditorUtility.OpenFolderPanel(
				"Target Folder",
				folder.Remove(folder.LastIndexOf('/')),
				folder.Remove(0, folder.LastIndexOf('/') + 1));

			if (Directory.Exists(folder))
			{
				EditorPrefs.SetString(FOLDER_PATH_KEY, folder);
			}
		}

		//[MenuItem("Core/Export/Export all MeshFilters in scene with StaticEditorFlag LightmapStatic")]
		//static void ExportAllLightmapStaticObjectsInScene()
		//{
		//	if (!CreateTargetFolder())
		//		return;

		//	List<MeshFilter> meshes = new List<MeshFilter>(GameObject.FindObjectsOfType<MeshFilter>());

		//	Dictionary<int, List<MeshFilter>> meshesByLightmap = new Dictionary<int, List<MeshFilter>>();

		//	foreach (MeshFilter mesh in meshes)
		//	{
		//		if (!GameObjectUtility.AreStaticEditorFlagsSet(mesh.gameObject, StaticEditorFlags.LightmapStatic))
		//		{
		//			continue;
		//		}
		//		Renderer renderer = mesh.gameObject.GetComponent<Renderer>();
		//		if (renderer == null || renderer.lightmapIndex < 0)
		//		{
		//			continue;
		//		}

		//		if (!meshesByLightmap.ContainsKey(renderer.lightmapIndex))
		//		{
		//			meshesByLightmap.Add(renderer.lightmapIndex, new List<MeshFilter>());
		//		}
		//		meshesByLightmap[renderer.lightmapIndex].Add(mesh);
		//	}

		//	if (meshesByLightmap.Keys.Count == 0)
		//	{
		//		EditorUtility.DisplayDialog("No source object selected!", "No objects in scene with StaticEditorFlag LightmapStatic and Lightmap Index > 0!", "ok");
		//		return;
		//	}

		//	foreach (int key in meshesByLightmap.Keys)
		//	{

		//		string filename = EditorSceneManager.GetActiveScene().path.Replace(".unity", string.Empty) + "_" + meshesByLightmap[key].Count + "_LightmapIndex" + key;

		//		int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator

		//		if (stripIndex >= 0)
		//			filename = filename.Substring(stripIndex + 1).Trim();

		//		MeshFiltersToFile(meshesByLightmap[key].ToArray(), EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH), filename, true, true);

		//		EditorUtility.DisplayDialog("Objects exported", "Exported " + meshesByLightmap[key].Count + " objects to " + filename, "ok");
		//	}
		//}

		[MenuItem("Core/Export/Export all MeshFilters in selection to separate OBJs")]
		static void ExportSelectionToSeparate()
		{
			if (!CreateTargetFolder())
			{
				return;
			}

			Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

			if (selection.Length == 0)
			{
				EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "ok");
				return;
			}

			int exportedObjects = 0;

			for (int i = 0; i < selection.Length; i++)
			{
				Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

				for (int m = 0; m < meshfilter.Length; m++)
				{
					exportedObjects++;
					MeshFilterToFile((MeshFilter)meshfilter[m], EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH), selection[i].name + "_" + i + "_" + m, true, false);
				}
			}

			if (exportedObjects > 0)
			{
				EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "ok");
			}
			else
			{
				EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "ok");
			}
		}

        static void ExportWholeSelectionToSingle(bool exportMaterials)
        {
            if (!CreateTargetFolder())
			{
				return;
			}

			Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

            if (selection.Length == 0)
            {
                EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "ok");
                return;
            }

            List<SkinnedMeshRenderer> smrList = new();
            List<MeshFilter> mfList = new();

            for (int i = 0; i < selection.Length; i++)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = selection[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderers != null)
                {
                    smrList.AddRange(skinnedMeshRenderers);
                }

                MeshFilter[] meshfilters = selection[i].GetComponentsInChildren<MeshFilter>();
                if (meshfilters != null)
                {
                    mfList.AddRange(meshfilters);
                }
            }

            if (smrList.Count + mfList.Count > 0)
            {
                string filename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path + "_" + (smrList.Count + mfList.Count);

                int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator

                if (stripIndex >= 0)
				{
					filename = filename.Substring(stripIndex + 1).Trim();
				}

				SkinnedMeshRenderersAndMeshFiltersToFile(smrList.ToArray(), mfList.ToArray(), EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH), filename, exportMaterials, false);


                EditorUtility.DisplayDialog("Objects exported", "Exported " + (smrList.Count + mfList.Count) + " objects to " + filename, "ok");
            }
            else
			{
				EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "ok");
			}
		}

        static void ExportEachSelectionToSingle(bool exportMaterials)
        {
            if (!CreateTargetFolder())
			{
				return;
			}

			Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

            if (selection.Length == 0)
            {
                EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "ok");
                return;
            }

            int exportedObjects = 0;

            for (int i = 0; i < selection.Length; i++)
            {
                SkinnedMeshRenderer[] skinnedMeshRenderers = selection[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshFilter[] meshFilters = selection[i].GetComponentsInChildren<MeshFilter>();

                exportedObjects += skinnedMeshRenderers.Length + meshFilters.Length;

                SkinnedMeshRenderersAndMeshFiltersToFile(skinnedMeshRenderers, meshFilters, EditorPrefs.GetString(FOLDER_PATH_KEY, DEFAULT_FOLDER_PATH), selection[i].name + "_" + i, exportMaterials, false);
            }

            if (exportedObjects > 0)
            {
                EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "ok");
            }
            else
			{
				EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "ok");
			}
		}

		[MenuItem("Core/Export/Export whole selection to single OBJ")]
		static void ExportWholeSelectionToSingleWithMats()
		{
            ExportWholeSelectionToSingle(true);
		}


		[MenuItem("Core/Export/Export whole selection to single OBJ(no material)")]
		static void ExportWholeSelectionToSingleNoMats()
        {
            ExportWholeSelectionToSingle(false);
        }

        [MenuItem("Core/Export/Export each selected to single OBJ")]
        static void ExportEachSelectionToSingleWithMats()
        {
            ExportEachSelectionToSingle(true);
        }

        [MenuItem("Core/Export/Export each selected to single OBJ (no material)")]
        static void ExportEachSelectionToSingleNoMats()
        {
            ExportEachSelectionToSingle(false);
        }
	}
}
