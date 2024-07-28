using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Building Item", menuName = "Scriptable Object/Buildings/Item", order = 0)]
public class SOBuildingItem : ScriptableObject, IBuildModeRaycasterConstaints
{
	[SerializeField]
	private BuildModeInstance m_Prefab = null;
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
	private bool CanPlaceOnBuilding = false;

	public BuildModeInstance Prefab => m_Prefab;
	public BuildModePlacementMesh ModelPrefab => m_ModelPrefab;

	public string Header => m_Header;
	public string Body => m_Body;
	public Sprite Icon => m_Icon;

	bool IBuildModeRaycasterConstaints.RotateToNormal => m_RotateToNormal;
	float IBuildModeRaycasterConstaints.MaxAngle => m_MaxAngle;
	float IBuildModeRaycasterConstaints.MinAngle => m_MinAngle;
	bool IBuildModeRaycasterConstaints.CanPlaceOnTerrain => CanPlaceOnTerrain;
	bool IBuildModeRaycasterConstaints.CanPlaceOnBuilding => CanPlaceOnBuilding;
}
