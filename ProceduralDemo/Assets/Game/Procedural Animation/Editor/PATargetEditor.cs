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
			target2.TargetLocalOffset = Handles.DoPositionHandle(target2.TargetLocalOffset + target2.transform.position, Quaternion.identity) - target2.transform.position;
		}
	}
}
