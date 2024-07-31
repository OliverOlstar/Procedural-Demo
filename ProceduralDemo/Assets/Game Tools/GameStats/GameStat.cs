using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ODev.GameStats
{
	[System.Serializable]
	public abstract class GameStat<T>
	{
		[UnityEngine.SerializeField, HideInPlayMode]
		private T m_BaseValue;
		[UnityEngine.SerializeField, HideInEditorMode]
		private T m_CurrValue;

		private readonly Dictionary<int, float> m_PercentModifies = new();
		private readonly Dictionary<int, T> m_AddModifies = new();
		private int m_NextKey = int.MinValue;
		private bool m_HadCalulated = false;

		protected abstract T CalculateValueInternal(T pBase, Dictionary<int, float> pPercentModifies, Dictionary<int, T> pAddModifies);

		public T Value
		{
			get
			{
				if (!m_HadCalulated)
				{
					m_HadCalulated = true;
					m_CurrValue = CalculateValueInternal(m_BaseValue, m_PercentModifies, m_AddModifies);
				}
				return m_CurrValue;
			}
		}

		public GameStat(T pBaseValue)
		{
			m_BaseValue = pBaseValue;
			m_CurrValue = m_BaseValue;
		}

		public void SetBaseValue(T pBaseValue)
		{
			m_BaseValue = pBaseValue;
			m_HadCalulated = false;
		}

		public int AddModify(T pDelta)
		{
			m_NextKey++;
			m_AddModifies.Add(m_NextKey, pDelta);
			m_HadCalulated = false;
			return m_NextKey;
		}

		public bool TryRemoveModify(int pKey)
		{
			if (!m_AddModifies.Remove(pKey))
			{
				return false;
			}
			m_HadCalulated = false;
			return true;
		}

		public int AddPercentModify(float pPercent)
		{
			m_NextKey++;
			m_PercentModifies.Add(m_NextKey, pPercent);
			m_HadCalulated = false;
			return m_NextKey;
		}

		public bool TryRemovePercentModify(int pKey)
		{
			if (!m_PercentModifies.Remove(pKey))
			{
				return false;
			}
			m_HadCalulated = false;
			return true;
		}
	}
}