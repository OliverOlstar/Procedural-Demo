using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using UnityEditor;
using UnityEngine;

namespace ODev.PoseAnimator
{
    public class PoseSkeletonCreator : MonoBehaviour
    {
		[SerializeField, Asset] private SOPoseSkeleton m_Skeleton = null;
		[SerializeField] private Transform m_Root = null;

		public SOPoseSkeleton Skeleton => m_Skeleton;
		public Transform Root => m_Root;

		public void EditorCopyRootPoseToClip()
		{
			List<SOPoseSkeleton.Bone> bones = new();
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Root))
			{
				bones.Add(new SOPoseSkeleton.Bone(bone.Transform.localPosition, bone.Transform.localRotation, bone.Transform.localScale, bone.Transform.name, bone.Depth));
				this.Log(bones[^1].ToString());
			}
			m_Skeleton.SetBones(bones.ToArray());

			EditorUtility.SetDirty(m_Skeleton);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(m_Skeleton);
		}

		public void EditorValidateRootMatchesSkeleton()
		{
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				bone.Transform.Log($"Valid Bone {bone}");
			}
		}
	}
}
