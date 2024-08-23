using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ODev.Picker;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public class PoseClipMirrorCreator : MonoBehaviour
	{
		[System.Serializable]
		public struct MatchingBones
		{
			public int IndexA;
			public int IndexB;
			public bool IncludeChildren;
		}

		[System.Serializable]
		public struct OverrideBones
		{
			public int Index;
			public Mirror Mirror;
		}

		[System.Serializable]
		public struct Mirror
		{
			public bool MirrorPosition;
			public Vector3 PositionScale;
			public bool MirrorRotation;
			public Vector3 RotationNormal;
			public Vector3 RotationOffset;
		}

		[SerializeField]
		private Transform m_Root = null;
		// [SerializeField]
		// private SOPoseClip m_Clip = null;
		[SerializeField]
		public Mirror m_Mirror = new();

		[SerializeField]
		private Transform[] m_Ignores = new Transform[0];
		[SerializeField]
		private OverrideBones[] m_Overrides = new OverrideBones[0];
		[SerializeField]
		private MatchingBones[] m_Pairs = new MatchingBones[0];

		[Button]
		public void ApplyMirror()
		{
			foreach (PoseUtil.Bone bone in PoseUtil.GetAllBones(m_Root))
			{
				if (m_Ignores.Contains(bone.Transform))
				{
					continue;
				}
				Mirror mirror = m_Mirror;
				if (TryGetOverride(bone.Index, out Mirror overrideMirror))
				{
					mirror = overrideMirror;
				}
				if (mirror.MirrorPosition)
				{
					bone.Transform.localPosition = bone.Transform.localPosition.Scale(mirror.PositionScale.x, mirror.PositionScale.y, mirror.PositionScale.z);
				}
				if (mirror.MirrorRotation)
				{
					bone.Transform.localRotation = ReflectRotation(bone.Transform.localRotation, mirror.RotationNormal);
					bone.Transform.localRotation *= Quaternion.Euler(mirror.RotationOffset);
				}
			}
		}

		private bool TryGetOverride(int pIndex, out Mirror oOverride)
		{
			foreach (OverrideBones over in m_Overrides)
			{
				if (pIndex != over.Index)
				{
					continue;
				}
				oOverride = over.Mirror;
				return true;
			}
			oOverride = default;
			return false;
		}

		private Quaternion ReflectRotation(Quaternion source, Vector3 normal)
		{
			return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
		}
	}
}
