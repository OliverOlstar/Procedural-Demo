using OCore.Util;

namespace PA
{
	public class PALimbTrigger
	{
		private readonly PARoot m_Root;
		private readonly PALimb m_Limb;
		private readonly SOLimbTrigger m_Data;

		public PALimbTrigger(SOLimbTrigger pData, PARoot pRoot, PALimb pLimb)
		{
			m_Root = pRoot;
			m_Limb = pLimb;
			m_Data = pData;
		}

		public bool Tick(float pDeltaTime)
		{
			foreach (PALimb leg in m_Root.Limbs)
			{
				if (leg != null && leg.IsMoving)
				{
					return false;
				}
			}
			return Math.DistanceXZGreaterThan(m_Limb.Position, m_Limb.OriginalPositionWorld(), m_Data.MaxDistance);
		}

		public void DrawGizmos()
		{

		}
	}
}
