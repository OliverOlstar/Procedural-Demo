using ODev.Util;
using UnityEditor;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Clip", menuName = "PoseAnimator/Clip", order = 0)]
	public class SOPoseClip : ScriptableObject
	{
		[SerializeField]
		private PoseKey[] m_Keys;

		[SerializeField]
		private bool m_Mirror = false;

		public int KeyCount => m_Keys.Length;

		public void SetKeys(PoseKey[] pKeys)
		{
			m_Keys = pKeys;
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssetIfDirty(this);
		}

		public PoseKey GetKey(int pIndex)
		{
			PoseKey key = m_Keys[pIndex];
			if (m_Mirror)
			{
				// key.Position = key.Position.Inverse();
				// key.Rotation = key.Rotation.Inverse();
				this.DevException(new System.NotImplementedException());
			}
			return key;
		}

		public void Apply(Transform pBone, int pIndex) // TODO Remove
		{
			pBone.SetLocalPositionAndRotation(m_Keys[pIndex].Position, m_Keys[pIndex].Rotation);
			pBone.localScale = m_Keys[pIndex].Scale;
		}
	}
}
