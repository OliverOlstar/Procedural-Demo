using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildModeRaycasterConstaints
{
	public bool RotateToNormal { get; }
	public float MaxAngle { get; }
	public float MinAngle { get; }
	public bool CanPlaceOnTerrain { get; }
	public bool CanPlaceOnBuilding { get; }
}

public class BuildModeRaycasterConstaints : IBuildModeRaycasterConstaints
{
	public BuildModeRaycasterConstaints(
		bool pRotateToNormal = true, float pMaxAngle = float.MaxValue, float pMinAngle = float.MinValue, bool pCanPlaceOnTerrain = true, bool pCanPlaceOnBuilding = true)
	{
		RotateToNormal = pRotateToNormal;
		MaxAngle = pMaxAngle;
		MinAngle = pMinAngle;
		CanPlaceOnTerrain = pCanPlaceOnTerrain;
		CanPlaceOnBuilding = pCanPlaceOnBuilding;
	}

	public bool RotateToNormal { get; private set; }
	public float MaxAngle { get; private set; }
	public float MinAngle { get; private set; }
	public bool CanPlaceOnTerrain { get; private set; }
	public bool CanPlaceOnBuilding { get; private set; }
}
