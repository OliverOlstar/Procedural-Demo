using ODev.Util;
using ODev.VariableSOs;
using UnityEngine;

namespace ODev.GameStats
{
	[CreateAssetMenu(menuName = "Variables/GameStat/FloatGameStatModifier", fileName = "FloatGameStatModifier", order = 0)]
	public class FloatGameStatModifier : FloatVariableSO
	{
		public enum Type
		{
			Addition,
			Percent
		}

		[SerializeField]
		private Type m_Method;

		public void Apply(FloatGameStat pStat)
		{
			switch (m_Method)
			{
				case Type.Addition:
					pStat.AddModify(this);
					break;
				case Type.Percent:
					pStat.AddPercentModify(this);
					break;
				default:
					this.DevException(new System.NotImplementedException(m_Method.ToString()));
					return;
			}
		}

		public void Remove(FloatGameStat pStat)
		{
			switch (m_Method)
			{
				case Type.Addition:
					pStat.TryRemoveModify(this);
					break;
				case Type.Percent:
					pStat.TryRemovePercentModify(this);
					break;
				default:
					this.DevException(new System.NotImplementedException(m_Method.ToString()));
					return;
			}
		}

		public FloatGameStatModifier CreateCopy()
		{
			var instance = Instantiate(this);
			return instance;
		}
	}
}
