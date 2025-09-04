using System.Diagnostics;
using UnityEngine;

namespace ODev.Util
{
	public static class Gizmos2
	{
		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pVectorA, Vector3 pVectorB, float pRadius)
		{
            Gizmos.DrawWireSphere(pVectorA, pRadius);
            Gizmos.DrawLine(pVectorA + (Vector3.forward * pRadius), pVectorB + (Vector3.forward * pRadius));
            Gizmos.DrawLine(pVectorA + (Vector3.left * pRadius), pVectorB + (Vector3.left * pRadius));
            Gizmos.DrawLine(pVectorA + (Vector3.right * pRadius), pVectorB + (Vector3.right * pRadius));
            Gizmos.DrawLine(pVectorA + (Vector3.back * pRadius), pVectorB + (Vector3.back * pRadius));
            Gizmos.DrawWireSphere(pVectorB, pRadius);
		}

		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pCenter, float pRadius, float pHeight)
		{
			pHeight -= pRadius * 2.0f;
			if (pHeight <= 0)
			{
                Gizmos.DrawWireSphere(pCenter, pRadius);
				return;
			}
			Vector3 top = pCenter + (0.5f * pHeight * Vector3.up);
			Vector3 bottem = pCenter + (0.5f * pHeight * Vector3.down);
			GizmoCapsule(top, bottem, pRadius);
		}

		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pVectorA, Vector3 pVectorB, float pRadius, Matrix4x4 pMatrix)
		{
            Gizmos.matrix = pMatrix;
			GizmoCapsule(pVectorA, pVectorB, pRadius);
            Gizmos.matrix = Matrix4x4.identity;
		}

		[Conditional("ENABLE_DEBUG_GIZMOS")]
		public static void GizmoCapsule(Vector3 pCenter, float pRadius, float pHeight, Matrix4x4 pMatrix)
		{
            Gizmos.matrix = pMatrix;
			GizmoCapsule(pCenter, pRadius, pHeight);
            Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
