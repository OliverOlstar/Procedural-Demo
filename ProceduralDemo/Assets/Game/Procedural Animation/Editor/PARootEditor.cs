using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PA;

[CustomEditor(typeof(PARoot2))]
public class PARootEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		if (target is PARoot2 root)
		{
			PARootEditor_DuplicateLimbs.DrawGUI(root);
		}
	}
}
