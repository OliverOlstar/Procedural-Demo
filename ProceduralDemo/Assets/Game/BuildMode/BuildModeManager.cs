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

	/// <returns>If inventory has more of the same building left</returns>
	public static bool PlaceNewItem(SOBuildingItem pItem, in Vector3 pPosition, in Quaternion pRotation)
		=> Instance.PlaceNewItemInternal(pItem, pPosition, pRotation);
	public static void MoveItem(int pId, in Vector3 pPosition, in Quaternion pRotation)
		=> Instance.MoveItemInternal(pId, pPosition, pRotation);
	public static void RemoveItem(int pId) => Instance.RemoveItemInternal(pId);
	public static void Clear() => Instance.ClearInternal();
	public static void Save() => Instance.SaveInternal();
	public static void Load() => Instance.LoadInternal();

	/// <returns>If inventory has more of the same building left</returns>
	private bool PlaceNewItemInternal(SOBuildingItem pItem, in Vector3 pPosition, in Quaternion pRotation)
	{
		BuildModeInstance instance = Object.Instantiate(pItem.Prefab);
		int id = m_LastId;
		m_LastId++;
		instance.Initalize(id, pItem, pPosition, pRotation);
		m_Instances.Add(id, instance);

		int remainingCount = PlayerBuildingInventory.Instance.RemoveItem(pItem);
		return remainingCount > 0;
	}

	private void RemoveItemInternal(int pId)
	{
		if (!m_Instances.TryGetValue(pId, out BuildModeInstance instance))
		{
			DevException($"Building instance of id {pId} could not be found");
			return;
		}
		PlayerBuildingInventory.Instance.AddItem(instance.Data);
		Object.Destroy(instance.gameObject);
	}

	private void MoveItemInternal(int pId, in Vector3 pPosition, in Quaternion pRotation)
	{
		if (!m_Instances.TryGetValue(pId, out BuildModeInstance instance))
		{
			DevException($"Building instance of id {pId} could not be found");
			return;
		}
		instance.Move(pPosition, pRotation);
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
