using System.Collections;
using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.XR;

namespace ODev.PoseAnimator
{
	public class PoseClipCreator : MonoBehaviour
	{
		[SerializeField, Asset]
		private SOPoseClip m_Clip = null;
		[SerializeField, AssetNonNull]
		private SOPoseSkeleton m_Skeleton = null;
		[SerializeField]
		private Transform m_Root = null;

		[Button]
		private void CopyRootPoseToClip()
		{
			PoseKey[] keys = new PoseKey[m_Skeleton.BoneCount];

			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				PoseKey skeletonKey = m_Skeleton.GetBone(bone.Index).Key;
				var key = new PoseKey(
					bone.Transform.localPosition - skeletonKey.Position,
					bone.Transform.localRotation.Difference(skeletonKey.Rotation),
					bone.Transform.localScale - skeletonKey.Scale);
				keys[bone.Index] = key;
				this.Log(key.ToString());
			}
			m_Clip.SetKeys(keys);

			EditorUtility.SetDirty(m_Clip);
			AssetDatabase.SaveAssetIfDirty(m_Clip);
		}

		[Button]
		private void ApplyClipToRoot()
		{
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				PoseKey skeletonKey = m_Skeleton.GetBone(bone.Index).Key;
				PoseKey key = m_Clip.GetKey(bone.Index);

				bone.Transform.SetLocalPositionAndRotation(
					skeletonKey.Position + key.Position,
					skeletonKey.Rotation.Add(key.Rotation));
				bone.Transform.localScale = skeletonKey.Scale + key.Scale;
			}
		}

		[Button]
		private void ApplySkeletonToRoot()
		{
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				this.Log(bone.ToString());
				PoseKey skeletonKey = m_Skeleton.GetBone(bone.Index).Key;

				bone.Transform.SetLocalPositionAndRotation(
					skeletonKey.Position,
					skeletonKey.Rotation);
				bone.Transform.localScale = skeletonKey.Scale;
			}
		}
	}
}
