using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Skeleton", menuName = "PoseAnimator/Skeleton", order = 0)]
	public class SOPoseSkeleton : ScriptableObject
	{
		[System.Serializable]
		public struct Bone
		{
			public PoseKey Key;
			public int Depth;

			public Bone(Vector3 pPosition, Quaternion pRotation, Vector3 pScale, int pDepth)
			{
				Key = new PoseKey(pPosition, pRotation, pScale);
				Depth = pDepth;
			}

			public override readonly string ToString()
			{
				return $"[Bone] Depth {Depth}, Key {Key}";
			}
		}

		[SerializeField]
		private Bone[] m_Bones;

		public int BoneCount => m_Bones.Length;

#if UNITY_EDITOR
		public void SetBones(Bone[] pBones)
		{
			m_Bones = pBones;
		}
#endif

		public Bone GetBone(int pIndex)
		{
			Bone key = m_Bones[pIndex];
			return key;
		}
	}
}
