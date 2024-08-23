using UnityEngine;

namespace ODev
{
	public class SmoothLocalOffset : MonoBehaviour
	{
		public Transform Transform = null;
		public Vector3 Offset = Vector3.zero;
		private Vector3 m_InitalOffset = Vector3.zero;
		[SerializeField]
		private float m_Dampening = 5.0f;

		private void Reset()
		{
			Transform = transform;
			Offset = Transform.localPosition;
		}

		private void Start()
		{
			m_InitalOffset = Offset;
		}

		private void Update()
		{
			Transform.localPosition = Vector3.Lerp(Transform.localPosition, Offset, m_Dampening * Time.deltaTime);
		}

		public void SetOffset(Vector3 pOffset) => Offset = pOffset;
		public void SetOffsetY(float pHeight) => Offset = new Vector3(0.0f, pHeight, 0.0f);
		public void ModifyOffsetY(float pHeight) => Offset.y += pHeight;
		public void ModifyInitialOffsetY(float pHeight) => Offset.y = m_InitalOffset.y + pHeight;
		public void ResetOffset() => Offset = m_InitalOffset;
	}
}
