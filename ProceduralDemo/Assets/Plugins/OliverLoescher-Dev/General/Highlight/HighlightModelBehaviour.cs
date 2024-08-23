using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;

namespace ODev
{
	public class HighlightModelBehaviour : MonoBehaviour
    {
		[SerializeField]
		private MeshFilter[] m_MeshRenderers = new MeshFilter[1];
		[SerializeField, Required]
		private Material m_Material = null;

		private void Reset()
		{
			m_MeshRenderers = GetComponentsInChildren<MeshFilter>();
		}

		private void Awake()
		{
			m_Material = Instantiate(m_Material);
		}

		public void Set(Color pColor)
		{
			m_Material.SetColor("_EmissionColor", pColor);
			enabled = true;
		}

		public void Clear()
		{
			enabled = false;
		}

		private void LateUpdate()
		{
			foreach (MeshFilter renderer in m_MeshRenderers)
			{
				for (int i = 0; i < renderer.sharedMesh.subMeshCount; i++)
				{
					Graphics.DrawMesh(renderer.sharedMesh, renderer.transform.localToWorldMatrix, m_Material, 1, MainCamera.Camera, i, null, ShadowCastingMode.Off, receiveShadows: false, null, LightProbeUsage.Off, null);
				}
			}
		}
	}
}
