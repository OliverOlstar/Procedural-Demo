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
		private void CopyRootPoseToClipNoSkel()
		{
			Stack<Transform> transforms = new();
			List<PoseKey> keys = new();

			transforms.Push(m_Root);
			while (transforms.Count > 0)
			{
				Transform t = transforms.Pop();
				for (int i = 0; i < t.childCount; i++)
				{
					transforms.Push(t.GetChild(i));
				}

				keys.Add(new PoseKey(t.localPosition, t.localRotation, t.localScale));
				this.Log(keys[^1].ToString());
			}

			m_Clip.SetKeys(keys.ToArray());
			EditorUtility.SetDirty(m_Clip);
			AssetDatabase.SaveAssetIfDirty(m_Clip);
		}

		[Button]
		private void ApplyClipToRootNoSkel()
		{
			Queue<Transform> transforms = new();
			int index = 0;

			transforms.Enqueue(m_Root);
			while (transforms.Count > 0)
			{
				// Current
				Transform t = transforms.Dequeue();
				m_Clip.Apply(t, index);
				t.localScale = Vector3.one;

				// Next
				for (int i = 0; i < t.childCount; i++)
				{
					transforms.Enqueue(t.GetChild(i));
				}

				index++;
			}
		}


		[Button]
		private void CopyRootPoseToClip()
		{
			PoseKey[] keys = new PoseKey[m_Skeleton.BoneCount];

			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Skeleton, m_Root))
			{
				PoseKey skeletonKey = m_Skeleton.GetBone(bone.Index).Key;
				var key = new PoseKey(
					bone.Transform.localPosition - skeletonKey.Position,
					skeletonKey.Rotation.Difference(bone.Transform.localRotation),
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
					skeletonKey.Rotation * key.Rotation);
				bone.Transform.localScale = skeletonKey.Scale + key.Scale;
			}
		}
	}
}
