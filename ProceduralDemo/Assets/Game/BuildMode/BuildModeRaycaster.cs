using System;
using ODev;
using ODev.Util;
using UnityEngine;
using IConstaints = IBuildModeRaycasterConstaints;

public class BuildModeRaycaster : MonoBehaviour
{
	public struct Result
	{
		public Vector3 Origin;
		public Vector3 Point;
		public Quaternion Rotation;
		public bool IsValid;
		public BuildModeInstance Other;

		public Result(Vector3 pOrigin, Vector3 pPoint, Quaternion pRotation, bool pIsValid, BuildModeInstance pOther = null)
		{
			Origin = pOrigin;
			Point = pPoint;
			Rotation = pRotation;
			IsValid = pIsValid;
			Other = pOther;
		}
	}

	[SerializeField]
	private LayerMask m_TerrainLayer = new();
	[SerializeField]
	private LayerMask m_BuildingLayer = new();

	private float m_RotationYOffset = 0.0f;
	private IConstaints m_Constaints = null;

	public void SetConstraints(IConstaints pConstaints)
	{
		if (pConstaints == null)
		{
			this.DevException("Constaints can not be set to null");
			return;
		}
		m_Constaints = pConstaints;
	}

	public Result DoRaycast()
	{
		if (m_Constaints == null)
		{
			this.LogError($"Must call {nameof(SetConstraints)} first");
			return new Result(default, default, default, false);
		}

		Ray ray = MainCamera.Camera.ScreenPointToRay(Input.mousePosition);
		if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, m_TerrainLayer | m_BuildingLayer))
		{
			return new Result(hit.point, ray.origin, Quaternion.identity, false);
		}
		BuildModeInstance otherBuilding = hit.collider.gameObject.GetComponentInParent<BuildModeInstance>();

		Quaternion rotation = Quaternion.identity;
		if (m_Constaints.RotateToNormal)
		{
			rotation *= Quaternion.FromToRotation(Vector3.up, hit.normal);
		}
		rotation *= Quaternion.Euler(0.0f, MainCamera.Rotation.eulerAngles.y + m_RotationYOffset, 0.0f);

		return new Result(ray.origin, hit.point, rotation, IsValidHit(hit), otherBuilding);
	}

	private bool IsValidHit(in RaycastHit pHit)
	{
		if (!pHit.collider.gameObject.isStatic)
		{
			return false;
		}
		float angle = Vector3.Angle(pHit.normal, Vector3.up);
		if (angle > m_Constaints.MaxAngle || angle < m_Constaints.MinAngle)
		{
			// this.Log($"Invalid angle {angle}");
			return false;
		}
		if (!m_Constaints.CanPlaceOnTerrain && m_TerrainLayer.ContainsLayer(pHit.collider.gameObject.layer))
		{
			// this.Log($"Invalid on terrain");
			return false;
		}
		if (!m_Constaints.CanPlaceOnBuilding && m_BuildingLayer.ContainsLayer(pHit.collider.gameObject.layer))
		{
			// this.Log($"Invalid on object");
			return false;
		}
		return true;
	}

	public void Rotate(float pValue)
	{
		m_RotationYOffset += pValue;
		m_RotationYOffset = m_RotationYOffset.Loop(360);
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Ray ray = MainCamera.Camera.ScreenPointToRay(Input.mousePosition);
		Gizmos.DrawRay(ray);
	}
}
