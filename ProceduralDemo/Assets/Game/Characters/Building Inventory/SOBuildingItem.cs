using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Building Item", menuName = "Scriptable Object/Buildings/Item", order = 0)]
public class SOBuildingItem : ScriptableObject
{
	[SerializeField]
	private GameObject m_Prefab = null;

	public GameObject Prefab => m_Prefab;
}
