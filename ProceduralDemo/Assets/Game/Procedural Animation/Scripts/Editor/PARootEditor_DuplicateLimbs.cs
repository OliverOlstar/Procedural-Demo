using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;
using PA;
using System.Linq;

public static class PARootEditor_DuplicateLimbs
{
	public static int Count = 3;

	public static void DrawGUI(PARoot pRoot)
	{
		using (new GUILayout.VerticalScope(GUI.skin.textField))
		{
			Count = EditorGUILayout.IntField("Count ", Count);

			if (!GUILayout.Button("Duplicate"))
			{
				return;
			}

			// pRoot.Initalize();
			GameObject limb = pRoot.LimbIKs[0].gameObject;

			for (int i = 1; i < Count; i++)
			{
				GameObject newLimb = Object.Instantiate(limb, limb.transform.parent);
				newLimb.transform.SetPositionAndRotation(limb.transform.position, limb.transform.rotation);
				newLimb.transform.RotateAround(pRoot.transform.position, Vector3.up, (360 / Count) * i);
				// pRoot.AddLimb((IPALimb)newLimb);
			}
		}
	}
}
