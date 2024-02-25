using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Core
{
	[CustomEditor(typeof(LightmapInfoStorage))]
	public class LightmapInfoEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			LightmapInfoStorage lmis = (LightmapInfoStorage)target;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Lightmaps", GUILayout.Width(140));
			int length = EditorGUILayout.IntField(lmis.m_Lightmaps.Length, GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();

			if (length != lmis.m_Lightmaps.Length)
			{
				LightmapInfoStorage.Lightmap[] temp = new LightmapInfoStorage.Lightmap[length];
				System.Array.Copy(lmis.m_Lightmaps, temp, Mathf.Min(lmis.m_Lightmaps.Length, temp.Length));
				lmis.m_Lightmaps = temp;
			}
			
			if (GUILayout.Button("Store Lightmaps", GUILayout.Width(140.0f)))
			{
				lmis.m_Lightmaps = new LightmapInfoStorage.Lightmap[LightmapSettings.lightmaps.Length];
				for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
				{
					lmis.m_Lightmaps[i] = new LightmapInfoStorage.Lightmap();
					lmis.m_Lightmaps[i].mNear = LightmapSettings.lightmaps[i].lightmapDir;
					lmis.m_Lightmaps[i].mFar = LightmapSettings.lightmaps[i].lightmapColor;
				}
				lmis.m_LightmapsMode = LightmapSettings.lightmapsMode;
				foreach (MeshRenderer renderer in FindObjectsOfType<MeshRenderer>())
				{
					RendererInfoStorage.StoreLightmapInfo<MeshRenderer>(renderer.transform);
				}
			}
			//if (GUILayout.Button("Apply Lightmaps", GUILayout.Width(140.0f)))
			//{
			//	lmis.ApplyLightmaps();
			//	foreach (RendererInfoStorage renderer in FindObjectsOfType<RendererInfoStorage>())
			//	{
			//		renderer.SetupLightmapInfo();
			//	}
			//}
			lmis.m_LightmapsMode = (LightmapsMode)EditorGUILayout.EnumPopup("Lightmaps Mode", lmis.m_LightmapsMode);
			for (int i = 0; i < lmis.m_Lightmaps.Length; i++)
			{
				if (lmis.m_Lightmaps[i] == null)
				{
					lmis.m_Lightmaps[i] = new LightmapInfoStorage.Lightmap();
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("far", GUILayout.Width(45));
				lmis.m_Lightmaps[i].mFar = (Texture2D)EditorGUILayout.ObjectField(lmis.m_Lightmaps[i].mFar, typeof(Texture2D), true);
				EditorGUILayout.LabelField("near", GUILayout.Width(45));
				lmis.m_Lightmaps[i].mNear = (Texture2D)EditorGUILayout.ObjectField(lmis.m_Lightmaps[i].mNear, typeof(Texture2D), true);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}
	}
}
