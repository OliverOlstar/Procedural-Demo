using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev
{
	public class StateMachine : MonoBehaviour
	{
		[Header("StateMachine")]
		[SerializeField, DisableInPlayMode]
		private BaseState m_DefaultState = null;
		[SerializeField, DisableInPlayMode, HideInEditorMode]
		private BaseState m_CurrState = null;
		[SerializeField, DisableInPlayMode, HideInEditorMode]
		private BaseState[] m_States = new BaseState[0];

		[SerializeField]
		private bool m_PrintDebugs = false;

		private void Start()
		{
			m_States = GetComponentsInChildren<BaseState>();

			// Initizalize
			foreach (BaseState state in m_States)
			{
				state.Init(this);
			}

			// Enter first state
			SwitchState(m_DefaultState);
		}

		private void FixedUpdate()
		{
			foreach (BaseState state in m_States)
			{
				if (state != m_CurrState)
				{
					if (state.CanEnter())
					{
						SwitchState(state);
						Log(StateName(state) + " CanEnter() == true");
					}
				}
				else
				{
					if (state.CanExit())
					{
						ReturnToDefault();
						Log(StateName(state) + " CanExit() == true");
					}
				}
			}

			if (m_CurrState != null)
			{
				m_CurrState.OnFixedUpdate();
			}
		}

		private void Update()
		{
			if (m_CurrState != null)
			{
				m_CurrState.OnUpdate();
			}
		}

		public void SwitchState(BaseState pState)
		{
			Log("SwitchState: from " + StateName(m_CurrState) + " - to " + StateName(pState));

			if (m_CurrState != null)
			{
				m_CurrState.OnExit();
			}

			m_CurrState = pState;

			if (m_CurrState != null)
			{
				m_CurrState.OnEnter();
			}
		}

		public void ReturnToDefault()
		{
			SwitchState(m_DefaultState);
		}

		public bool IsState(BaseState pState)
		{
			return m_CurrState == pState;
		}
		public bool IsDefaultState()
		{
			return m_CurrState == m_DefaultState;
		}

		private void Log(string pString)
		{
			if (!m_PrintDebugs)
			{
				return;
			}
			Debug.Log("[StateMachine.cs] " + pString, this);
		}
		private string StateName(BaseState pState) => pState == null ? "Null" : pState.ToString();
	}
}