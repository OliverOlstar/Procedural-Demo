using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PATarget)), CanEditMultipleObjects]
public class PATargetEditor : Editor
{
	public void OnSceneGUI()
	{
		if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		if (target is PATarget target2)
		{
			Vector3 worldPosition = target2.transform.TransformPoint(target2.TargetLocalOffset);
			worldPosition = Handles.DoPositionHandle(worldPosition, target2.transform.rotation);
			target2.TargetLocalOffset = target2.transform.InverseTransformPoint(worldPosition);
		}
	}
}
