using System.Collections;
using System.Collections.Generic;
using ODev.Util;

namespace ODev.GameStats
{
	[System.Serializable]
	public class FloatGameStat : GameStat<float>
	{
		public FloatGameStat(float pBaseValue) : base(pBaseValue) { }

		protected override float CalculateValueInternal(float pBase, Dictionary<int, float> pPercentModifies, Dictionary<int, float> pAddModifies)
		{
			float value = pBase * Math.AddPercents(pPercentModifies.Values);
			return Math.Add(value, pAddModifies.Values);
		}
	}
}