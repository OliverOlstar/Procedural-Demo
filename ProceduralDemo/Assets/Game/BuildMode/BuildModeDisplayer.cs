using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class BuildModeDisplayer : MonoBehaviour
{
    [SerializeField]
	private LineRenderer m_Line = null;
	[SerializeField]
	private float m_OriginOffset = -2.0f;
	[SerializeField]
	private float m_RotationDampening = 10.0f;

	[Header("Colours")]
	[SerializeField]
	private string m_ColourParameterName = "_TintColor";
	[SerializeField]
	private Color m_ValidColour = Color.green;
	[SerializeField]
	private Color m_InvalidColour = Color.red;

	private BuildModePlacementMesh m_ModelInstance = null;
	private Renderer[] m_ModelInstanceRenderers = null;
	private Quaternion m_Rotation = Quaternion.identity;

	public void SetModel(BuildModePlacementMesh pModelPrefab)
	{
		if (m_ModelInstance != null)
		{
			Destroy(m_ModelInstance.gameObject);
		}
		if (pModelPrefab == null)
		{
			m_Line.enabled = false;
			return;
		}
		m_Line.enabled = true;
		m_ModelInstance = Instantiate(pModelPrefab.gameObject).GetComponent<BuildModePlacementMesh>();
		m_ModelInstance.transform.rotation = Quaternion.Euler(0.0f, m_Rotation.eulerAngles.y, 0.0f);
		m_ModelInstanceRenderers = m_ModelInstance.GetComponentsInChildren<Renderer>();
	}

	public void UpdateVisuals(BuildModeRaycaster.Result pContext, float pDeltaTime)
	{
		m_Line.startColor = pContext.IsValid ? Color.green : Color.red;
		pContext.Origin.y += m_OriginOffset;
		m_Line.SetPosition(0, pContext.Origin);
		m_Line.SetPosition(1, pContext.Point);

		foreach (Renderer renderer in m_ModelInstanceRenderers)
		{
			renderer.material.SetColor(m_ColourParameterName, pContext.IsValid ? m_ValidColour : m_InvalidColour);
		}
		m_Rotation = Quaternion.Lerp(m_ModelInstance.transform.rotation, pContext.Rotation, m_RotationDampening * pDeltaTime);
		m_ModelInstance.transform.SetPositionAndRotation(pContext.Point, m_Rotation);
	}

	public bool IsOverlapping => m_ModelInstance.IsOverlapping;
}
