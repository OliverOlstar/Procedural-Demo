using System.Collections;
using System.Collections.Generic;
using ODev;
using UnityEngine;

public class BuildModeManager : Singleton<BuildModeManager>
{
	private struct Building
	{
		public int Id;
		public System.WeakReference<SOBuildingItem> Item;
		public Vector3 Position;
		public Quaternion Rotation;
		public int Parent;
		public int[] Children;
	}

	private readonly Dictionary<int, BuildModeInstance> m_Instances = new();
	private int m_LastId = int.MinValue;
	private Transform m_Container;

	protected override void OnStart()
	{
		m_Container = new GameObject("BuildModeContainer").transform;
	}

	protected override void OnDestroy()
	{
		if (m_Container != null)
		{
			Object.Destroy(m_Container.gameObject);
		}
	}

	public static void PlaceNewItem(SOBuildingItem pItem, in Vector3 pPosition, in Quaternion pRotation)
		=> Instance.PlaceNewItemInternal(pItem, pPosition, pRotation);
	public static void MoveItem(int pId, in Vector3 pPosition, in Quaternion pRotation)
		=> Instance.MoveItemInternal(pId, pPosition, pRotation);
	public static void RemoveItem(int pId) => Instance.RemoveItemInternal(pId);
	public static void Clear() => Instance.ClearInternal();
	public static void Save() => Instance.SaveInternal();
	public static void Load() => Instance.LoadInternal();

	private void PlaceNewItemInternal(SOBuildingItem pItem, in Vector3 pPosition, in Quaternion pRotation)
	{
		BuildModeInstance instance = Object.Instantiate(pItem.Prefab);
		int id = m_LastId;
		m_LastId++;
		instance.Initalize(id, pPosition, pRotation);
		Instance.m_Instances.Add(id, instance);
	}

	private void RemoveItemInternal(int pId)
	{

	}

	private void MoveItemInternal(int pId, in Vector3 pPosition, in Quaternion pRotation)
	{

	}

	private void ClearInternal()
	{

	}

	private void SaveInternal()
	{

	}

	private void LoadInternal()
	{
		ClearInternal();
		
	}
}
