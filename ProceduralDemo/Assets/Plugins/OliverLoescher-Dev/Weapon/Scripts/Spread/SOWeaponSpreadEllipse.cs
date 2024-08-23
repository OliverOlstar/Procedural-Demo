using UnityEngine;
using UnityEditor;

namespace ODev.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Spread/Ellipse")]
	public class SOWeaponSpreadEllipse : SOWeaponSpreadExpandBase
	{
		[SerializeField]
		private Vector2 m_SpreadVector = new(0.2f, 0.2f);
		[SerializeField]
		private Vector2 m_SpreadVectorMax = new(0.5f, 0.5f);

		public override Vector3 ApplySpread(Vector3 pDirection)
		{
			Vector2 spread = Vector2.Lerp(m_SpreadVector, m_SpreadVectorMax, m_Spread01);
			return Quaternion.Euler(Util.Random.GetRandomPointInEllipse(spread.y * 2.0f, spread.x * 2.0f)) * pDirection;
		}

#if UNITY_EDITOR
		public override void DrawGizmos(in Transform pTransform, in Transform pMuzzle)
		{
			Handles.matrix = pTransform.localToWorldMatrix;
			Vector3 localForward = pTransform.InverseTransformVector(pMuzzle.forward);

			Handles.color = Color.cyan;
			Handles.DrawWireCube(localForward * 1.0f, new Vector3(m_SpreadVector.x * 0.04f, m_SpreadVector.y * 0.04f, 0.0f));

			Handles.color = Color.blue;
			Handles.DrawWireCube(localForward * 1.0f, new Vector3(m_SpreadVectorMax.x * 0.04f, m_SpreadVectorMax.y * 0.04f, 0.0f));

			if (Application.isPlaying)
			{
				Handles.color = Color.green;
				Vector2 spread = Vector2.Lerp(m_SpreadVector, m_SpreadVectorMax, m_Spread01);
				Handles.DrawWireCube(localForward * 1.0f, new Vector3(spread.x * 0.04f, spread.y * 0.04f, 0.0f));
			}
		}
#endif
	}
}