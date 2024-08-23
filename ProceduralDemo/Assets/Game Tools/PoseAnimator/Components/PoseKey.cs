using UnityEngine;

namespace ODev.PoseAnimator
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

		public PoseKey Set(Vector3 pPosition, Quaternion pRotation, Vector3 pScale)
		{
			Position = pPosition;
			Rotation = pRotation;
			Scale = pScale;
			return this;
		}

		public override readonly string ToString()
		{
			return $"[PoseKey] Position {Position}, Rotation {Rotation}, Scale {Scale}";
		}
	}
}