using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PATarget))]
public class PATargetEditor : Editor
{
	public void OnSceneGUI()
	{
		if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		foreach (PATarget target in targets)
		{
			target.TargetLocalOffset = Handles.DoPositionHandle(target.TargetLocalOffset + target.transform.position, Quaternion.identity) - target.transform.position;
		}
	}
}
