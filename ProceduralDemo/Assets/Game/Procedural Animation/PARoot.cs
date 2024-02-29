using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PARoot : MonoBehaviour
{
	[SerializeField, DisableInPlayMode]
	private OliverLoescher.Util.Mono.Updateable Updateable = new OliverLoescher.Util.Mono.Updateable(OliverLoescher.Util.Mono.UpdateType.Late, OliverLoescher.Util.Mono.Priorities.ModelController);

	private bool IsInitalized = false;

	public PACharacter Character { get; private set; }
	public IPABody Body { get; private set; }
	public IPAPoint[] Points { get; private set; } = new IPAPoint[0];
	public IPALimb[] Limbs { get; private set; } = new IPALimb[0];

	private void Awake()
	{
		if (IsInitalized)
		{
			return;
		}
		IsInitalized = true;

		Character = GetComponentInChildren<PACharacter>();
		Body = GetComponentInChildren<IPABody>();
		Limbs = GetComponentsInChildren<IPALimb>();
		Points = GetComponentsInChildren<IPAPoint>();

		for (int i = 0; i < Points.Length; i++)
		{
			Points[i].Init(Character);
		}
		Body?.Init(this);
		for (int i = 0; i < Limbs.Length; i++)
		{
			Limbs[i].Init(this);
		}
	}
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
		for (int i = 0; i < Limbs.Length; i++)
		{
			Limbs[i].Tick(pDeltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		Awake();

		Body?.DrawGizmos();
		for (int i = 0; i < Limbs.Length; i++)
		{
			Limbs[i].DrawGizmos();
		}
		for (int i = 0; i < Points.Length; i++)
		{
			Points[i].DrawGizmos();
		}
	}
}
