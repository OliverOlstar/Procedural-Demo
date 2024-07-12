using UnityEngine;
using UnityEditor;
using ODev.Util;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Spread/Circle")]
	public class SOWeaponSpreadCircle : SOWeaponSpreadExpandBase
	{
		[SerializeField]
		private float m_SpreadRadius = 0.2f;
		[SerializeField]
		private float m_SpreadRadiusMax = 0.5f;

		public override Vector3 ApplySpread(Vector3 pDirection)
		{
			float spread = Mathf.Lerp(m_SpreadRadius, m_SpreadRadiusMax, m_Spread01);
			return Quaternion.Euler(Random2.GetRandomPointOnCircle(spread)) * pDirection;
		}

#if UNITY_EDITOR
		public override void DrawGizmos(in Transform pTransform, in Transform pMuzzle)
		{
			Handles.matrix = pTransform.localToWorldMatrix;
			Vector3 localForward = pTransform.InverseTransformVector(pMuzzle.forward);

			Handles.color = Color.cyan;
			Handles.DrawWireDisc(localForward * 1.0f, localForward, m_SpreadRadius * 0.01f);

			Handles.color = Color.blue;
			Handles.DrawWireDisc(localForward * 1.0f, localForward, m_SpreadRadiusMax * 0.01f);

			if (Application.isPlaying)
			{
				Handles.color = Color.green;
				float spread = Mathf.Lerp(m_SpreadRadius, m_SpreadRadiusMax, m_Spread01);
				Handles.DrawWireDisc(localForward * 1.0f, localForward, spread * 0.01f);
			}
		}
#endif
	}
}
