using ODev.Util;
using UnityEngine;

namespace ODev.GameStats
{
	[System.Serializable]
	public class IntGameStatModifier : GameStatModifier<IntGameStat, int> { }
	[System.Serializable]
	public class FloatGameStatModifier : GameStatModifier<FloatGameStat, float> { }

	public abstract class GameStatModifier<TStat, TValue> where TStat : GameStat<TValue>
    {
		public enum Type
		{
			Addition,
			Percent
		}

		[SerializeField]
		private Type m_Type;
		[SerializeField]
		private TValue m_Add = default;
		[SerializeField]
		private float m_Percent = 1.0f;

		private int? m_ModifyKey;

		public void Apply(TStat pStat)
		{
			if (m_ModifyKey.HasValue)
			{
				this.LogError("Tried adding modify when we already have one added");
				return;
			}
			switch (m_Type)
			{
				case Type.Addition:
					m_ModifyKey = pStat.AddModify(m_Add);
					break;
				case Type.Percent:
					m_ModifyKey = pStat.AddPercentModify(m_Percent);
					break;
				default:
					this.DevException(new System.NotImplementedException(m_Type.ToString()));
					return;
			}
		}

		public void Remove(TStat pStat)
		{
			if (!m_ModifyKey.HasValue)
			{
				this.LogError("Tried removing modify when don't have one");
				return;
			}
			bool success;
			switch (m_Type)
			{
				case Type.Addition:
					success = pStat.TryRemoveModify(m_ModifyKey.Value);
					break;
				case Type.Percent:
					success = pStat.TryRemovePercentModify(m_ModifyKey.Value);
					break;
				default:
					this.DevException(new System.NotImplementedException(m_Type.ToString()));
					return;
			}
			if (!success)
			{
				this.LogError("Failed to remove modify");
			}
			m_ModifyKey = null;
		}

		public static T CreateCopy<T>(T pCopyFrom) where T : GameStatModifier<TStat, TValue>, new()
		{
			return new T()
			{
				m_Type = pCopyFrom.m_Type,
				m_Add = pCopyFrom.m_Add,
				m_Percent = pCopyFrom.m_Percent
			};
		}
    }
}
