using ODev.Util;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Clip", menuName = "PoseAnimator/Clip", order = 0)]
	public class SOPoseClip : ScriptableObject
	{
		[System.Serializable]
		public struct PoseKey
		{
			public Vector3 Position;
			public Quaternion Rotation;
			public Vector3 Scale;

			public PoseKey(Vector3 pPosition, Quaternion pRotation, Vector3 pScale)
			{
				Position = pPosition;
				Rotation = pRotation;
				Scale = pScale;
			}

			public override readonly string ToString()
			{
				return $"[PoseKey] Position {Position}, Rotation {Rotation}, Scale {Scale}";
			}
		}

		[SerializeField]
		private PoseKey[] m_Keys;

		[SerializeField]
		private bool m_Mirror = false;

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

		public void Apply(Transform pBone, int pIndex)
		{
			pBone.SetLocalPositionAndRotation(m_Keys[pIndex].Position, m_Keys[pIndex].Rotation);
			pBone.localScale = m_Keys[pIndex].Scale;
		}
	}
}
