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

			if (GUILayout.Button(nameof(instance.EditorCopyRootPoseToClip)))
			{
				instance.EditorCopyRootPoseToClip();
			}
			if (GUILayout.Button(nameof(instance.EditorValidateRootMatchesSkeleton)))
			{
				instance.EditorValidateRootMatchesSkeleton();
			}

			GUILayout.FlexibleSpace();
			foreach (var bone in PoseUtil.GetAllBones(instance.Skeleton, instance.Root))
			{
				GUILayout.Label($"[{bone.Index}]({bone.Depth}) {bone.Transform.name}");
			}
		}
	}
}
