using UnityEngine;

namespace Core
{
	[ExecuteInEditMode]
	public class RendererInfoStorage : MonoBehaviour
	{
		public string[] m_ShaderNames = new string[0];
		public int m_LightmapIndex = -1;
		public Vector4 m_LightmapScaleOffset = new(1.0f, 1.0f, 0.0f, 0.0f);
		public Color[] m_VertexColors = new Color[0];

		public static void StoreLightmapInfo<T>(Transform transform) where T : Renderer
		{
			if (transform == null)
			{
				return;
			}

			foreach (T renderer in transform.GetComponentsInChildren<T>())
			{
				
				if (!renderer.gameObject.TryGetComponent<RendererInfoStorage>(out RendererInfoStorage rendererInfo))
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", DebugUtil.GetScenePath(renderer.gameObject)));
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
				
				if (!renderer.gameObject.TryGetComponent<RendererInfoStorage>(out RendererInfoStorage rendererInfo))
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", DebugUtil.GetScenePath(renderer.gameObject)));
				}

				rendererInfo.m_ShaderNames = new string[renderer.sharedMaterials.Length];
				for (int i = 0; i < renderer.sharedMaterials.Length; i++)
				{
					if (renderer.sharedMaterials[i] == null)
					{
						Debug.LogError("RendererInfoStorage.StoreShaderInfo: MeshRenderer on GameObject " + DebugUtil.GetScenePath(renderer.gameObject) + " has a null material.");
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

				
				if (!renderer.gameObject.TryGetComponent<RendererInfoStorage>(out RendererInfoStorage rendererInfo))
				{
					rendererInfo = renderer.gameObject.AddComponent<RendererInfoStorage>();
					Debug.Log(string.Format("RendererInfoStorage.StoreLightmapInfo: Added a RendererInfoStorage component to {0}.", DebugUtil.GetScenePath(renderer.gameObject)));
				}

				rendererInfo.m_VertexColors = meshFilter.sharedMesh.colors;
			}
		}

		public void SetupMeshColliders()
		{
			if (!gameObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
			{
				return;
			}

			if (!gameObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
			{
				return;
			}

			meshCollider.sharedMesh = meshFilter.mesh;
		}

		public void SetupVertexColors()
		{
			if (!gameObject.TryGetComponent<MeshFilter>(out MeshFilter filter))
			{
				return;
			}

			filter.mesh.colors = m_VertexColors;
		}

		public void SetupShaders()
		{
			if (!gameObject.TryGetComponent<Renderer>(out Renderer renderer))
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
