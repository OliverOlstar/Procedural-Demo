using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using ODev.VariableSOs;
using UnityEngine;

namespace ODev.GameStats
{
	[CreateAssetMenu(menuName = "Variables/GameStat/IntGameStat", fileName = "IntGameStat", order = 0)]
	public class IntGameStat : IntVariableSO
	{
		[SerializeField, AssetNonNull]
		private List<IntVariableSO> m_AddModifiers = new();
		[SerializeField, AssetNonNull, Tooltip("-0.5 = -50%, 0 = 0%, 0.5 = 50%, 2 = 200%")]
		private List<FloatVariableSO> m_PercentModifiers = new();

		private readonly Dictionary<int, int> m_PureAddModifiers = new();
		private readonly Dictionary<int, int> m_PurePercentModifiers = new();

		private int m_NextKey = int.MinValue;
		private bool m_HadCalulated = false;

		public override int Value
		{
			get
			{
				if (!m_HadCalulated)
				{
					m_HadCalulated = true;
					int value = Mathf.FloorToInt(DefaultValue * Math.AddPercents(GetAllPercents()));
					m_Value = Math.Add(value, GetAllAdds());
				}
				return m_Value;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			foreach (var modifier in m_AddModifiers)
			{
				modifier.OnValueChanged += OnModifierChanged;
			}
			foreach (var modifier in m_PercentModifiers)
			{
				modifier.OnValueChanged += OnModifierChanged;
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			foreach (var modifier in m_AddModifiers)
			{
				modifier.OnValueChanged -= OnModifierChanged;
			}
			foreach (var modifier in m_PercentModifiers)
			{
				modifier.OnValueChanged -= OnModifierChanged;
			}
		}

		public override void SetValue(int value)
		{
			m_HadCalulated = false;
			base.SetValue(value);
		}

		public override void ResetToDefault()
		{
			m_HadCalulated = false;
			m_PureAddModifiers.Clear();
			m_PurePercentModifiers.Clear();
			base.ResetToDefault();
		}

		public IntGameStat CreateCopy()
		{
			var instance = Instantiate(this);
			return instance;
		}

		public void AddModify(IntVariableSO pDelta)
		{
			m_AddModifiers.Add(pDelta);
			pDelta.OnValueChanged += OnModifierChanged;
			m_HadCalulated = false;
		}
		public bool TryRemoveModify(IntVariableSO pDelta)
		{
			if (!m_AddModifiers.Remove(pDelta))
			{
				this.LogWarning("Failed to remove modify");
				return false;
			}
			pDelta.OnValueChanged -= OnModifierChanged;
			m_HadCalulated = false;
			return true;
		}

		public void AddPercentModify(FloatVariableSO pDelta)
		{
			m_PercentModifiers.Add(pDelta);
			pDelta.OnValueChanged += OnModifierChanged;
			m_HadCalulated = false;
		}
		public bool TryRemovePercentModify(FloatVariableSO pDelta)
		{
			if (!m_PercentModifiers.Remove(pDelta))
			{
				this.LogWarning("Failed to remove modify");
				return false;
			}
			pDelta.OnValueChanged -= OnModifierChanged;
			m_HadCalulated = false;
			return true;
		}

		public int AddModify(int pDelta)
		{
			m_NextKey++;
			m_PureAddModifiers.Add(m_NextKey, pDelta);
			m_HadCalulated = false;
			return m_NextKey;
		}
		public bool TryRemoveModify(int pKey)
		{
			if (!m_PureAddModifiers.Remove(pKey))
			{
				this.LogWarning("Failed to remove modify");
				return false;
			}
			m_HadCalulated = false;
			return true;
		}

		public int AddPercentModify(int pPercent)
		{
			m_NextKey++;
			m_PurePercentModifiers.Add(m_NextKey, pPercent);
			m_HadCalulated = false;
			return m_NextKey;
		}
		public bool TryRemovePercentModify(int pKey)
		{
			if (!m_PurePercentModifiers.Remove(pKey))
			{
				this.LogWarning("Failed to remove modify");
				return false;
			}
			m_HadCalulated = false;
			return true;
		}

		private IEnumerable<int> GetAllAdds()
		{
			foreach (var modifier in m_AddModifiers)
			{
				yield return modifier.Value;
			}
			foreach (int modifier in m_PureAddModifiers.Values)
			{
				yield return modifier;
			}
		}

		private IEnumerable<float> GetAllPercents()
		{
			foreach (var modifier in m_PercentModifiers)
			{
				yield return modifier.Value;
			}
			foreach (int modifier in m_PurePercentModifiers.Values)
			{
				yield return modifier;
			}
		}

		private void OnModifierChanged(int _)
		{
			m_HadCalulated = false;
		}

		private void OnModifierChanged(float _)
		{
			m_HadCalulated = false;
		}
	}
}