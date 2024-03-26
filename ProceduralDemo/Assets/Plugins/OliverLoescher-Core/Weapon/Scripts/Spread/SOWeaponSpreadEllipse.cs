using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OliverLoescher.Weapon
{
	[CreateAssetMenu(menuName = "Scriptable Object/Weapon/Spread/Ellipse")]
	public class SOWeaponSpreadEllipse : SOWeaponSpreadExpandBase
	{
		public Vector2 spreadVector = new Vector2(0.2f, 0.2f);
		public Vector2 spreadVectorMax = new Vector2(0.5f, 0.5f);

		public override Vector3 ApplySpread(Vector3 pDirection)
		{
			Vector2 spread = Vector2.Lerp(spreadVector, spreadVectorMax, spread01);
			return Quaternion.Euler(Util.Random2.GetRandomPointInEllipse(spread.y * 2.0f, spread.x * 2.0f)) * pDirection;
		}

#if UNITY_EDITOR
		public override void DrawGizmos(in Transform pTransform, in Transform pMuzzle)
		{
			Handles.matrix = pTransform.localToWorldMatrix;
			Vector3 localForward = pTransform.InverseTransformVector(pMuzzle.forward);

			Handles.color = Color.cyan;
			Handles.DrawWireCube(localForward * 1.0f, new Vector3(spreadVector.x * 0.04f, spreadVector.y * 0.04f, 0.0f));

			Handles.color = Color.blue;
			Handles.DrawWireCube(localForward * 1.0f, new Vector3(spreadVectorMax.x * 0.04f, spreadVectorMax.y * 0.04f, 0.0f));

			if (Application.isPlaying)
			{
				Handles.color = Color.green;
				Vector2 spread = Vector2.Lerp(spreadVector, spreadVectorMax, spread01);
				Handles.DrawWireCube(localForward * 1.0f, new Vector3(spread.x * 0.04f, spread.y * 0.04f, 0.0f));
			}
		}
#endif
	}
}