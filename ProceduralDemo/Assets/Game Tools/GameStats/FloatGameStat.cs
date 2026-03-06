using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using ODev.VariableSOs;
using UnityEngine;

namespace ODev.GameStats
{
	[CreateAssetMenu(menuName = "Variables/GameStat/FloatGameStat", fileName = "FloatGameStat", order = 0)]
	public class FloatGameStat : FloatVariableSO
	{
		[SerializeField, AssetNonNull] private List<FloatVariableSO> m_AddModifiers = new();
		[Tooltip("-0.5 = -50%, 0 = 0%, 0.5 = 50%, 2 = 200%")]
		[SerializeField, AssetNonNull] private List<FloatVariableSO> m_PercentModifiers = new();

		private readonly Dictionary<int, float> m_PureAddModifiers = new();
		private readonly Dictionary<int, float> m_PurePercentModifiers = new();

		private int m_NextKey = int.MinValue;
		private bool m_HadCalulated = false;

		public override float Value
		{
			get
			{
				if (!m_HadCalulated)
				{
					m_HadCalulated = true;
					float value = DefaultValue * Math.AddPercents(GetAllPercents());
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

		public override void SetValue(float value)
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

		public FloatGameStat CreateCopy()
		{
			var instance = Instantiate(this);
			return instance;
		}

		public void AddModify(FloatVariableSO pDelta)
		{
			m_AddModifiers.Add(pDelta);
			pDelta.OnValueChanged += OnModifierChanged;
			m_HadCalulated = false;
		}
		public bool TryRemoveModify(FloatVariableSO pDelta)
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

		public int AddModify(float pDelta)
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

		public int AddPercentModify(float pPercent)
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

		private IEnumerable<float> GetAllAdds()
		{
			foreach (var modifier in m_AddModifiers)
			{
				yield return modifier.Value;
			}
			foreach (float modifier in m_PureAddModifiers.Values)
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
			foreach (float modifier in m_PurePercentModifiers.Values)
			{
				yield return modifier;
			}
		}

		private void OnModifierChanged(float _)
		{
			m_HadCalulated = false;
		}
	}
}