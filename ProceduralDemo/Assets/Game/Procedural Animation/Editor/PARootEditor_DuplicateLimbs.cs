using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;

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

			pRoot.Initalize();
			IPALimb limb;
			if (pRoot.Limbs.IsNullOrEmpty())
			{
				limb = pRoot.GetComponentInChildren<IPALimb>();
				if (limb == null)
				{
					OliverLoescher.Util.Debug2.LogError("Could not find a limb", "DuplicateLimbs.DrawGUI", pRoot);
					return;
				}
			}
			else
			{
				limb = pRoot.Limbs[0];
			}
			if (limb is not MonoBehaviour limbBehaviour)
			{
				OliverLoescher.Util.Debug2.LogError("The selected limb is not a monoBehaviour", "DuplicateLimbs.DrawGUI", pRoot);
				return;
			}

			for (int i = 1; i < Count; i++)
			{
				MonoBehaviour newLimb = GameObject.Instantiate(limbBehaviour, limbBehaviour.transform.parent);
				newLimb.transform.SetPositionAndRotation(limbBehaviour.transform.position, limbBehaviour.transform.rotation);
				newLimb.transform.RotateAround(pRoot.transform.position, Vector3.up, (360 / Count) * i);
				// pRoot.AddLimb((IPALimb)newLimb);
			}
		}
	}
}
