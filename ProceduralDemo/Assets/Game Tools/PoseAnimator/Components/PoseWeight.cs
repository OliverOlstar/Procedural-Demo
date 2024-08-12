
namespace ODev.PoseAnimator
{
	[System.Serializable]
    public struct PoseWeight
	{
		public float Weight01;
		public float Progress01;

		public PoseWeight(float pWeight01, float pProgress01 = 0.0f)
		{
			Weight01 = pWeight01;
			Progress01 = pProgress01;
		}
	}
}
