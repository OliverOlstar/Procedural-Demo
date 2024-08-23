using System.Collections.Generic;
using UnityEngine;

namespace ODev.Camera
{
	public class SpectatorCamera : MonoBehaviour
	{
		#region Singleton
		public static SpectatorCamera s_Instance = null;
		private void Awake()
		{
			if (s_Instance != null)
			{
				Debug.LogError("[SpectatorCamera] Multiple Instances, destroying other", this);
				Destroy(s_Instance);
			}
			s_Instance = this;
		}

		private void OnDestroy()
		{
			s_Instance = null;
		}
		#endregion

		private enum Mode
		{
			None,
			FirstPerson,
			ThirdPerson,
			Freefly
		}

		//[SerializeField] private InputBridge_Spectator inputBridge = null;
		[SerializeField]
		private FollowTarget m_FirstPersonCamera = null;
		[SerializeField]
		private ThirdPersonCamera m_ThirdPersonCamera = null;
		[SerializeField]
		private FreeflyCamera m_FreeflyCamera = null;
		private Transform m_CameraTransform;

		[HideInInspector]
		public List<SpectatorTarget> Targets = new();

		private int m_TargetIndex = 0;
		private float m_CanInputTime = 0.0f;

		private Mode m_Mode = Mode.None;

		private void Start()
		{
			//inputBridge.onLook.AddListener(OnLook);
			//inputBridge.onLookDelta.AddListener(OnLookDelta);
			//inputBridge.onMove.AddListener(OnMove);
			//inputBridge.onMoveVertical.AddListener(OnMoveVertical);
			//inputBridge.onZoom.AddListener(OnZoom);
			//inputBridge.onSprint.AddListener(OnSprint);
			//inputBridge.onMode.AddListener(OnMode);
			//inputBridge.onTarget.AddListener(OnTarget);

			m_CameraTransform = UnityEngine.Camera.main.transform;

			OnEnable();
		}

		private void OnEnable()
		{
			if (m_CameraTransform == null) // Don't run before Start()
			{
				return;
			}
			m_FreeflyCamera.gameObject.SetActive(false);
			m_FirstPersonCamera.gameObject.SetActive(false);
			m_ThirdPersonCamera.gameObject.SetActive(false);

			m_Mode = Mode.None;
			SwitchMode(Mode.Freefly);
		}

		private void SwitchMode()
		{
			if (m_Mode == Mode.ThirdPerson || (Targets.Count == 0 && m_Mode == Mode.Freefly))
			{
				SwitchMode(Mode.Freefly); // To Freefly from Third
			}
			else if (m_Mode == Mode.Freefly)
			{
				SwitchMode(Mode.FirstPerson); // To FirstPerson from Free
			}
			else if (m_Mode == Mode.FirstPerson)
			{
				SwitchMode(Mode.ThirdPerson); // To ThirdPerson from First
			}
		}

		private void SwitchMode(Mode pMode)
		{
			if (m_Mode == pMode)
			{
				return;
			}

			m_Mode = pMode;

			switch (m_Mode)
			{
				case Mode.Freefly:
					m_FreeflyCamera.gameObject.SetActive(true);
					m_ThirdPersonCamera.gameObject.SetActive(false);

					if (m_CameraTransform != null)
					{
						m_FreeflyCamera.gameObject.transform.SetPositionAndRotation(m_CameraTransform.position, m_CameraTransform.rotation);
					}

					//inputBridge.ClearInputs();
					break;
				case Mode.FirstPerson:
					m_CanInputTime = Time.time + 0.4f;

					m_FirstPersonCamera.gameObject.SetActive(true);
					m_FreeflyCamera.gameObject.SetActive(false);

					Targets[m_TargetIndex].Toggle(true);

					m_FirstPersonCamera.PosTarget = Targets[m_TargetIndex].FirstPersonTarget;
					m_FirstPersonCamera.RotTarget = Targets[m_TargetIndex].FirstPersonTarget;
					break;
				case Mode.ThirdPerson:
					m_CanInputTime = Time.time + 0.4f;

					m_ThirdPersonCamera.gameObject.SetActive(true);
					m_FirstPersonCamera.gameObject.SetActive(false);

					Targets[m_TargetIndex].Toggle(false);

					m_ThirdPersonCamera.transform.rotation = Quaternion.Euler(30.0f, Targets[m_TargetIndex].ThirdPersonTarget.eulerAngles.y, 0.0f);
					m_ThirdPersonCamera.FollowTransform = Targets[m_TargetIndex].ThirdPersonTarget;

					//inputBridge.ClearInputs();
					break;
			}
		}

		private void SwitchTarget()
		{
			// TODO on if a target stop exisiting, switch to someone else

			Targets[m_TargetIndex].Toggle(false);

			m_TargetIndex++;
			if (m_TargetIndex == Targets.Count)
			{
				m_TargetIndex = 0;
			}

			if (m_Mode == Mode.Freefly)
			{
				SwitchMode();
			}

			if (m_Mode == Mode.FirstPerson)
			{
				m_FirstPersonCamera.PosTarget = Targets[m_TargetIndex].FirstPersonTarget;
				m_FirstPersonCamera.RotTarget = Targets[m_TargetIndex].FirstPersonTarget;

				Targets[m_TargetIndex].Toggle(true);
			}
			else if (m_Mode == Mode.ThirdPerson)
			{
				m_ThirdPersonCamera.FollowTransform = Targets[m_TargetIndex].ThirdPersonTarget;
			}
		}

		public void OnTargetLost(SpectatorTarget pTarget)
		{
			int index = Targets.IndexOf(pTarget);
			if (m_TargetIndex == index && m_Mode != Mode.Freefly)
			{
				SwitchMode(Mode.Freefly);
			}

			if (m_TargetIndex >= index && m_TargetIndex > 0)
			{
				m_TargetIndex--;
			}
		}

		#region Input
		protected virtual void OnLook(Vector2 pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.ThirdPerson)
			{
				m_ThirdPersonCamera.OnLook(pInput);
			}
			else if (m_Mode == Mode.Freefly)
			{
				m_FreeflyCamera.OnLook(pInput);
			}
		}
		protected virtual void OnLookDelta(Vector2 pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.ThirdPerson)
			{
				m_ThirdPersonCamera.OnLookDelta(pInput);
			}
			else if (m_Mode == Mode.Freefly)
			{
				m_FreeflyCamera.OnLookDelta(pInput);
			}
		}

		protected virtual void OnMove(Vector2 pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.Freefly)
			{
				m_FreeflyCamera.OnMoveHorizontal(pInput);
			}
		}
		protected virtual void OnMoveVertical(float pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.Freefly)
			{
				m_FreeflyCamera.OnMoveVertical(pInput);
			}
		}

		protected virtual void OnZoom(float pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.ThirdPerson)
			{
				m_ThirdPersonCamera.OnZoom(pInput);
			}
		}

		protected virtual void OnSprint(bool pInput)
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (m_Mode == Mode.Freefly)
			{
				m_FreeflyCamera.OnSprint(pInput);
			}
		}

		protected virtual void OnMode()
		{
			SwitchMode();
		}

		protected virtual void OnTarget()
		{
			if (Time.time < m_CanInputTime)
			{
				return;
			}

			if (Targets.Count > 2)
			{
				SwitchTarget();
			}
		}
	}
	#endregion
}