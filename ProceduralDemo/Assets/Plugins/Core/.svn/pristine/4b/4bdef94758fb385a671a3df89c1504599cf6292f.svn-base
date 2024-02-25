using UnityEngine;

namespace Core
{
	public sealed partial class Chrono : MonoBehaviour
	{
		private const float MAX_SHADER_TIME = 300f;

		private UpdatableCollection<IEarlyUpdatable> m_EarlyUpdatables;
		private UpdatableCollection<IUpdatable> m_Updatables;
		private UpdatableCollection<ILateUpdatable> m_LateUpdatables;
		private UpdatableCollection<IFixedUpdatable> m_FixedUpdatables;
		private float m_ShaderTimeSinceStart;

		private Chrono()
		{
			m_EarlyUpdatables = new UpdatableCollection<IEarlyUpdatable>(Update);
			m_Updatables = new UpdatableCollection<IUpdatable>(Update);
			m_LateUpdatables = new UpdatableCollection<ILateUpdatable>(Update);
			m_FixedUpdatables = new UpdatableCollection<IFixedUpdatable>(Update);
		}

		void Awake()
		{
			if (s_Instance != null)
			{
				Debug.LogWarningFormat("[Chrono] Another instance was created. Destroying the new instance.");
				Destroy(s_Instance);
				return;
			}
			s_Instance = this;
			UtcStartTime = UtcNow;
			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
		}

		void Update()
		{
			SetShaderTime(m_ShaderTimeSinceStart + Time.unscaledDeltaTime);
			if (m_ShaderTimeSinceStart >= MAX_SHADER_TIME)
			{
				SetShaderTime(-MAX_SHADER_TIME);
			}
			Shader.SetGlobalFloat("_TimeSinceStart", m_ShaderTimeSinceStart);
			m_EarlyUpdatables.Update();
			m_Updatables.Update();
		}

		void LateUpdate()
		{
			m_LateUpdatables.Update();
		}

		void FixedUpdate()
		{
			m_FixedUpdatables.Update();
		}

		private void SetShaderTime(float time)
		{
			m_ShaderTimeSinceStart = time;
		}

		private void OnReset()
		{
			m_ShaderTimeSinceStart = 0f;
			m_EarlyUpdatables.Reset();
			m_Updatables.Reset();
			m_LateUpdatables.Reset();
			m_FixedUpdatables.Reset();
		}

		private void Update(IEarlyUpdatable updatable)
		{
			updatable?.OnEarlyUpdate(Time.deltaTime);
		}

		private void Update(IUpdatable updatable)
		{
			updatable?.OnUpdate(Time.deltaTime);
		}

		private void Update(ILateUpdatable updatable)
		{
			updatable?.OnLateUpdate(Time.deltaTime);
		}

		private void Update(IFixedUpdatable updatable)
		{
			updatable?.OnFixedUpdate(Time.fixedDeltaTime);
		}
	}
}
