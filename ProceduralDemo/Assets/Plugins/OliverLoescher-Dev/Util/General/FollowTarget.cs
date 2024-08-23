using UnityEngine;

namespace ODev
{
	public class FollowTarget : MonoBehaviour
	{
		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Default, Util.Mono.Priorities.Camera);

		[Header("Position")]
		public Transform PosTarget = null;
		public Vector3 PosOffset = new();
		[Min(0)]
		public float PosDampening = 0.0f;

		[Header("Rotation")]
		public Transform RotTarget = null;
		public Vector3 RotOffset = new();
		[Min(0)]
		public float RotDampening = 0.0f;

		private void Start()
		{
			m_Updateable.Register(Tick);
		}

		private void OnDestroy()
		{
			m_Updateable.Deregister();
		}

		private void Tick(float pDeltaTime) 
		{
			if (PosTarget != null)
			{
				Vector3 pos = PosTarget.position + PosOffset;
				if (PosDampening == 0.0f)
				{
					transform.position = pos;
				}
				else
				{
					transform.position = Vector3.Lerp(transform.position, pos, pDeltaTime * PosDampening);
				}
			}
			
			if (RotTarget != null)
			{
				Quaternion rot = RotTarget.rotation * Quaternion.Euler(PosOffset);
				if (RotDampening == 0.0f)
				{
					transform.rotation = rot;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, rot, pDeltaTime * RotDampening);
				}
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (Application.isPlaying)
			{
				return;
			}

			if (PosTarget != null)
			{
				transform.position = PosTarget.position + PosOffset;
			}
			if (RotTarget != null)
			{
				transform.rotation = RotTarget.rotation * Quaternion.Euler(PosOffset);
			}
		}
	}
}