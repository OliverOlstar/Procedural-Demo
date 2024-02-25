using UnityEngine;

namespace Core
{
	[ExecuteInEditMode]
	public class RendererInfoStorage : MonoBehaviour
	{
		public string[] m_ShaderNames = new string[0];
		public int m_LightmapIndex = -1;
		public Vector4 m_LightmapScaleOffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
		public Color[] m_VertexColors = new Color[0];

		public static void StoreLightmapInfo<T>(Transform transform) where T : Renderer
		{
			if (transform == null)
			{
				return;
			}

			foreach (T renderer in transform.GetComponentsInChildren<T>())
			{
				RendererInfoStorage rendererInfo = renderer.gameObject.GetComponent<RendererInfoStorage>();

				if (rendererInfo == null)
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", Core.DebugUtil.GetScenePath(renderer.gameObject)));
				}

				rendererInfo.m_LightmapIndex = renderer.lightmapIndex;
				rendererInfo.m_LightmapScaleOffset = renderer.lightmapScaleOffset;
			}
		}

		public static void StoreShaderInfo<T>(Transform transform) where T : Renderer
		{
			if (transform == null)
			{
				return;
			}

			foreach (T renderer in transform.GetComponentsInChildren<T>())
			{
				RendererInfoStorage rendererInfo = renderer.gameObject.GetComponent<RendererInfoStorage>();

				if (rendererInfo == null)
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", Core.DebugUtil.GetScenePath(renderer.gameObject)));
				}

				rendererInfo.m_ShaderNames = new string[renderer.sharedMaterials.Length];
				for (int i = 0; i < renderer.sharedMaterials.Length; i++)
				{
					if (renderer.sharedMaterials[i] == null)
					{
						Debug.LogError("RendererInfoStorage.StoreShaderInfo: MeshRenderer on GameObject " + Core.DebugUtil.GetScenePath(renderer.gameObject) + " has a null material.");
						continue;
					}
					rendererInfo.m_ShaderNames[i] = renderer.sharedMaterials[i].shader.name;
				}
			}
		}

		public static void StoreVertexColorInfo<T>(Transform transform) where T : Renderer
		{
			if (transform == null)
			{
				return;
			}

			//foreach (VPaintGroup vpaint in transform.GetComponentsInChildren<VPaintGroup>())
			//{
			//	vpaint.Apply();
			//}

			foreach (T renderer in transform.GetComponentsInChildren<T>())
			{
				MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();

				if (meshFilter == null || meshFilter.sharedMesh == null)
				{
					continue;
				}

				RendererInfoStorage rendererInfo = renderer.gameObject.GetComponent<RendererInfoStorage>();

				if (rendererInfo == null)
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", Core.DebugUtil.GetScenePath(renderer.gameObject)));
				}

				rendererInfo.m_VertexColors = meshFilter.sharedMesh.colors;
			}
		}

		public void SetupMeshColliders()
		{
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				return;
			}

			MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
			if (meshCollider == null)
			{
				return;
			}

			meshCollider.sharedMesh = meshFilter.mesh;
		}

		public void SetupVertexColors()
		{
			MeshFilter filter = gameObject.GetComponent<MeshFilter>();
			if (filter == null)
			{
				return;
			}

			filter.mesh.colors = m_VertexColors;
		}

		public void SetupShaders()
		{
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null)
			{
				return;
			}

			if (renderer.materials.Length == m_ShaderNames.Length)
			{
				for (int i = 0; i < renderer.materials.Length; i++)
				{
					renderer.materials[i].shader = Shader.Find(m_ShaderNames[i]);
				}
			}
		}

		public void SetupLightmapInfo()
		{
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null || m_LightmapIndex < 0)
			{
				return;
			}
			renderer.lightmapIndex = m_LightmapIndex;
			renderer.lightmapScaleOffset = m_LightmapScaleOffset;
			renderer.gameObject.isStatic = true;
		}

		void OnEnable()
		{
			SetupLightmapInfo();
		}

		void OnDestroy()
		{
			if (Application.isPlaying)
			{
				MeshFilter meshFilter = GetComponent<MeshFilter>();
				if (meshFilter == null || meshFilter.mesh == null)
				{
					return;
				}
				Destroy(meshFilter.mesh);
			}
		}
	}
}
