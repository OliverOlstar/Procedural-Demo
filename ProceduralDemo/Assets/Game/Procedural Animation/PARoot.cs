using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class PARoot : MonoBehaviour
{
	[SerializeField, DisableInPlayMode]
	private OliverLoescher.Util.Mono.Updateable Updateable = new OliverLoescher.Util.Mono.Updateable(OliverLoescher.Util.Mono.Type.Late, OliverLoescher.Util.Mono.Priorities.ModelController);

	private bool IsInitalized = false;

	public IPACharacter Character { get; private set; }
	public IPABody Body { get; private set; }
	public List<IPAPoint> Points { get; private set; } = new();
	public List<IPALimb> Limbs { get; private set; } = new();

	public void Initalize()
	{
		if (IsInitalized)
		{
			return;
		}
		IsInitalized = true;

		Character = GetComponentInChildren<IPACharacter>();
		Body = GetComponentInChildren<IPABody>();
		GetComponentsInChildren(false, Limbs);
		GetComponentsInChildren(false, Points);

		for (int i = 0; i < Points.Count; i++)
		{
			Points[i].Init(Character);
		}
		Body?.Init(this);
		for (int i = 0; i < Limbs.Count; i++)
		{
			Limbs[i].Init(this);
		}
	}

	private void Awake() => Initalize();
	private void Start()
	{
		Updateable.Register(Tick);
	}
	private void OnDestroy()
	{
		Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		Body?.Tick(pDeltaTime);

		Limbs.Sort((IPALimb a, IPALimb b) => b.GetTickPriority().CompareTo(a.GetTickPriority()));
		for (int i = 0; i < Limbs.Count; i++)
		{
			Limbs[i].Tick(pDeltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		Initalize();

		Body?.DrawGizmos();
		for (int i = 0; i < Limbs.Count; i++)
		{
			Limbs[i].DrawGizmos();
		}
		for (int i = 0; i < Points.Count; i++)
		{
			Points[i].DrawGizmos();
		}
	}

	public void AddLimb(IPALimb pLimb)
	{
		if (!IsInitalized)
		{
			OliverLoescher.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddLimb", this);
			return;
		}
		if (Limbs.Contains(pLimb))
		{
			OliverLoescher.Util.Debug.LogWarning("Limb already added", "AddLimb", this);
			return;
		}
		Limbs.Add(pLimb);
		pLimb.Init(this);
	}
	public void RemoveLimb(IPALimb pLimb)
	{
		if (!Limbs.Remove(pLimb))
		{
			OliverLoescher.Util.Debug.LogWarning("Failed to remove", "RemoveLimb", this);
		}
	}

	public void AddPoint(IPAPoint pPoint)
	{
		if (!IsInitalized)
		{
			OliverLoescher.Util.Debug.LogWarning("Not initalized yet, skipping add", "AddPoint", this);
			return;
		}
		if (Points.Contains(pPoint))
		{
			OliverLoescher.Util.Debug.LogWarning("Point already added", "AddPoint", this);
			return;
		}
		Points.Add(pPoint);
		pPoint.Init(Character);
	}
	public void RemovePoint(IPAPoint pAPoint)
	{
		if (!Points.Remove(pAPoint))
		{
			OliverLoescher.Util.Debug.LogWarning("Failed to remove", "RemovePoint", this);
		}
	}
}
