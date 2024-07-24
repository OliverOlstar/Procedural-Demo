using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Building Item", menuName = "Scriptable Object/Buildings/Item", order = 0)]
public class SOBuildingItem : ScriptableObject, BuildModeRaycaster.IConstaints
{
	[SerializeField]
	private GameObject m_Prefab = null;
	[SerializeField]
	private BuildModePlacementMesh m_ModelPrefab = null;

	[Header("UI")]
	[SerializeField]
	private string m_Header = "";
	[SerializeField]
	private string m_Body = "";
	[SerializeField]
	private Sprite m_Icon = null;
	
	[Header("Constaints")]
	[SerializeField]
	private bool m_RotateToNormal = false;
	[SerializeField, Range(0.0f, 180.0f)]
	private float m_MaxAngle = 22.5f;
	[SerializeField, Range(0.0f, 180.0f)]
	private float m_MinAngle = 0.0f;
	[SerializeField]
	private bool CanPlaceOnTerrain = true;
	[SerializeField]
	private bool CanPlaceOnObject = false;

	public GameObject Prefab => m_Prefab;
	public BuildModePlacementMesh ModelPrefab => m_ModelPrefab;

	public string Header => m_Header;
	public string Body => m_Body;
	public Sprite Icon => m_Icon;

	bool BuildModeRaycaster.IConstaints.RotateToNormal => m_RotateToNormal;
	float BuildModeRaycaster.IConstaints.MaxAngle => m_MaxAngle;
	float BuildModeRaycaster.IConstaints.MinAngle => m_MinAngle;
	bool BuildModeRaycaster.IConstaints.CanPlaceOnTerrain => CanPlaceOnTerrain;
	bool BuildModeRaycaster.IConstaints.CanPlaceOnObject => CanPlaceOnObject;
}
