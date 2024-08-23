using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ODev.PoseAnimator
{
	[CustomEditor(typeof(PoseSkeletonCreator))]
	public class PoseSkeletonCreatorEditor : Editor
    {
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			PoseSkeletonCreator instance = (PoseSkeletonCreator)target;
			if (instance.Root == null || instance.Skeleton == null)
			{
				return;
			}

			GUILayout.FlexibleSpace();
			foreach (var bone in PoseUtil.GetAllBones(instance.Skeleton, instance.Root))
			{
				GUILayout.Label($"[{bone.Index}]({bone.Depth}) {bone.Transform.name}");
			}
		}
	}
}
