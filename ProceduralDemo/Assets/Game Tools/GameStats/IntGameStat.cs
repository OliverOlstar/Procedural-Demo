using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

namespace ODev.GameStats
{
	[System.Serializable]
	public class IntGameStat : GameStat<int>
	{
		public IntGameStat(int pBaseValue) : base(pBaseValue) { }

		protected override int CalculateValueInternal(int pBase, Dictionary<int, float> pPercentModifies, Dictionary<int, int> pAddModifies)
		{
			int value = Mathf.FloorToInt(pBase * Math.AddPercents(pPercentModifies.Values));
			return Math.Add(value, pAddModifies.Values);
		}
	}
}