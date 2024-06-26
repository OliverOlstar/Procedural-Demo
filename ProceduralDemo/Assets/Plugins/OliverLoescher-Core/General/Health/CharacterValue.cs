using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace OCore
{
	[DisallowMultipleComponent]
	public class CharacterValue : MonoBehaviour
	{
		[FoldoutGroup("Events")]
		public UnityEventsUtil.DoubleFloatEvent OnValueChangedEvent;
		[FoldoutGroup("Events")]
		public UnityEventsUtil.DoubleFloatEvent OnValueLoweredEvent;
		[FoldoutGroup("Events")]
		public UnityEventsUtil.DoubleFloatEvent OnValueRaisedEvent;
		[FoldoutGroup("Events"), ShowIf("@canRunOut")]
		public UnityEvent OnValueOutEvent;
		[FoldoutGroup("Events"), ShowIf("@canRunOut && doRecharge && canRechargeBackIn")]
		public UnityEvent OnValueInEvent;
		[FoldoutGroup("Events")]
		public UnityEventsUtil.DoubleFloatEvent OnMaxValueChangedEvent;

		public bool IsOut { get; protected set; } = false;

		// [Tooltip("Zero counts as infinite")]
		[SerializeField, Min(0.0f)]
		protected float m_Value = 100.0f;
		protected float m_MaxValue = 100.0f;

		[SerializeField]
		protected bool m_DoRecharge = false;
		[SerializeField]
		private bool m_CanRunOut = true;
		[SerializeField, ShowIf("@canRunOut && doRecharge")]
		private bool m_CanRechargeBackIn = true;

		[Header("Recharge")]
		[SerializeField, Min(0.0f), ShowIf("@doRecharge")]
		private float m_RechargeValueTo = 100.0f;
		[SerializeField, Min(0.0f), ShowIf("@doRecharge")]
		private float m_RechargeDelay = 1.0f;
		[SerializeField, Min(0.0f), ShowIf("@doRecharge")]
		private float m_RechargeRate = 20.0f;

		[Header("UI")]
		[SerializeField]
		protected List<BarValue> m_UIBars = new();

		protected virtual void Start()
		{
			m_MaxValue = m_Value;

			for (int i = 0; i < m_UIBars.Count; i++)
			{
				if (m_UIBars[i] == null)
				{
					m_UIBars.RemoveAt(i);
					i--;
				}
				else
				{
					m_UIBars[i].InitValue(1.0f);
				}
			}
		}

		public float Value => m_Value;
		public void Modify(float pValue)
		{
			Set(m_Value + pValue);
		}
		public void Set(float pValue)
		{
			float change = pValue - m_Value;
			bool lower = pValue < m_Value;
			m_Value = Mathf.Clamp(pValue, 0.0f, m_MaxValue);

			if (lower)
			{
				if (m_CanRunOut && !IsOut && m_Value == 0.0f)
				{
					OnValueOut();
				}
				else
				{
					OnValueLowered(m_Value, change);
				}
			}
			else
			{
				if (m_CanRechargeBackIn && IsOut && m_Value == m_MaxValue)
				{
					OnValueIn();
				}
				else
				{
					OnValueRaised(m_Value, change);
				}
			}

			if (m_DoRecharge)
			{
				StopAllCoroutines();
				StartCoroutine(RechargeRoutine());
			}

			OnValueChanged(m_Value, change);
		}

		public float MaxValue => m_MaxValue;
		public void ModifyMax(float pValue)
		{
			SetMax(m_MaxValue + pValue);
		}
		public void SetMax(float pValue)
		{
			float change = Mathf.Abs(m_MaxValue - pValue);

			m_MaxValue = pValue;
			m_Value = Mathf.Clamp(pValue, 0.0f, m_MaxValue);

			OnMaxValueChanged(m_MaxValue, change);
		}

		private IEnumerator RechargeRoutine()
		{
			yield return new WaitForSeconds(m_RechargeDelay);

			while (m_Value < Mathf.Min(m_MaxValue, m_RechargeValueTo))
			{
				m_Value += Time.deltaTime * m_RechargeRate;
				m_Value = Mathf.Min(m_Value, m_MaxValue);

				foreach (BarValue bar in m_UIBars)
				{
					bar.SetValue(m_Value / m_MaxValue);
				}

				yield return null;
			}

			if (m_CanRechargeBackIn && IsOut)
			{
				OnValueIn();
			}
		}

		public virtual void OnValueChanged(float pValue, float pChange)
		{
			foreach (BarValue bar in m_UIBars)
			{
				bar.SetValue(pValue / m_MaxValue);
			}
			OnValueChangedEvent?.Invoke(pValue, pChange);
		}

		public virtual void OnValueLowered(float pValue, float pChange)
		{
			OnValueLoweredEvent?.Invoke(pValue, pChange);
		}

		public virtual void OnValueRaised(float pValue, float pChange)
		{
			OnValueRaisedEvent?.Invoke(pValue, pChange);
		}

		public virtual void OnValueOut()
		{
			IsOut = true;
			foreach (BarValue bar in m_UIBars)
			{
				bar.SetToggled(true);
			}
			OnValueOutEvent?.Invoke();
		}

		public virtual void OnValueIn()
		{
			IsOut = false;
			foreach (BarValue bar in m_UIBars)
			{
				bar.SetToggled(false);
			}
			OnValueInEvent?.Invoke();
		}

		public virtual void OnMaxValueChanged(float pMaxValue, float pChange)
		{
			foreach (BarValue bar in m_UIBars)
			{
				bar.SetValue(m_Value / pMaxValue);
			}
			OnMaxValueChangedEvent?.Invoke(pMaxValue, pChange);
		}
	}
}