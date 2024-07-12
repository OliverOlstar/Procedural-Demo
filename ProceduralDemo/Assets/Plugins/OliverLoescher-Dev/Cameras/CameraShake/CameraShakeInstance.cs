using UnityEngine;

namespace EZCameraShake
{
    public enum CameraShakeState { FadingIn, FadingOut, Sustained, Inactive }

    public class CameraShakeInstance
    {
        /// <summary>
        /// The intensity of the shake. It is recommended that you use ScaleMagnitude to alter the magnitude of a shake.
        /// </summary>
        public float Magnitude;

        /// <summary>
        /// Roughness of the shake. It is recommended that you use ScaleRoughness to alter the roughness of a shake.
        /// </summary>
        public float Roughness;

        /// <summary>
        /// How much influence this shake has over the local position axes of the camera.
        /// </summary>
        public Vector3 PositionInfluence;

        /// <summary>
        /// How much influence this shake has over the local rotation axes of the camera.
        /// </summary>
        public Vector3 RotationInfluence;

        /// <summary>
        /// Should this shake be removed from the CameraShakeInstance list when not active?
        /// </summary>
        public bool DeleteOnInactive = true;


        private float m_RoughMod = 1.0f;
		private float m_MagnMod = 1.0f;
		private float m_FadeOutDuration;
		private float m_FadeInDuration;
		private bool m_Sustain;
		private float m_CurrentFadeTime;
		private float m_Tick = 0.0f;
		private Vector3 m_Amt;

        /// <summary>
        /// Will create a new instance that will shake once and fade over the given number of seconds.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="fadeOutTime">How long, in seconds, to fade out the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        public CameraShakeInstance(float magnitude, float roughness, float fadeInTime, float fadeOutTime)
        {
            this.Magnitude = magnitude;
            m_FadeOutDuration = fadeOutTime;
            m_FadeInDuration = fadeInTime;
            this.Roughness = roughness;
            if (fadeInTime > 0)
            {
                m_Sustain = true;
                m_CurrentFadeTime = 0;
            }
            else
            {
                m_Sustain = false;
                m_CurrentFadeTime = 1;
            }

            m_Tick = Random.Range(-100, 100);
        }

        /// <summary>
        /// Will create a new instance that will start a sustained shake.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        public CameraShakeInstance(float magnitude, float roughness)
        {
            this.Magnitude = magnitude;
            this.Roughness = roughness;
            m_Sustain = true;

            m_Tick = Random.Range(-100, 100);
        }

        public Vector3 UpdateShake()
        {
            m_Amt.x = Mathf.PerlinNoise(m_Tick, 0) - 0.5f;
            m_Amt.y = Mathf.PerlinNoise(0, m_Tick) - 0.5f;
            m_Amt.z = Mathf.PerlinNoise(m_Tick, m_Tick) - 0.5f;

            if (m_FadeInDuration > 0 && m_Sustain)
            {
                if (m_CurrentFadeTime < 1)
				{
					m_CurrentFadeTime += Time.deltaTime / m_FadeInDuration;
				}
				else if (m_FadeOutDuration > 0)
				{
					m_Sustain = false;
				}
			}

            if (!m_Sustain)
			{
				m_CurrentFadeTime -= Time.deltaTime / m_FadeOutDuration;
			}

			if (m_Sustain)
			{
				m_Tick += Time.deltaTime * Roughness * m_RoughMod;
			}
			else
			{
				m_Tick += Time.deltaTime * Roughness * m_RoughMod * m_CurrentFadeTime;
			}

			return m_CurrentFadeTime * Magnitude * m_MagnMod * m_Amt;
        }

        /// <summary>
        /// Starts a fade out over the given number of seconds.
        /// </summary>
        /// <param name="fadeOutTime">The duration, in seconds, of the fade out.</param>
        public void StartFadeOut(float fadeOutTime)
        {
            if (fadeOutTime == 0)
			{
				m_CurrentFadeTime = 0;
			}

			m_FadeOutDuration = fadeOutTime;
            m_FadeInDuration = 0;
            m_Sustain = false;
        }

        /// <summary>
        /// Starts a fade in over the given number of seconds.
        /// </summary>
        /// <param name="fadeInTime">The duration, in seconds, of the fade in.</param>
        public void StartFadeIn(float fadeInTime)
        {
            if (fadeInTime == 0)
			{
				m_CurrentFadeTime = 1;
			}

			m_FadeInDuration = fadeInTime;
            m_FadeOutDuration = 0;
            m_Sustain = true;
        }

        /// <summary>
        /// Scales this shake's roughness while preserving the initial Roughness.
        /// </summary>
        public float ScaleRoughness
        {
            get { return m_RoughMod; }
            set { m_RoughMod = value; }
        }

        /// <summary>
        /// Scales this shake's magnitude while preserving the initial Magnitude.
        /// </summary>
        public float ScaleMagnitude
        {
            get { return m_MagnMod; }
            set { m_MagnMod = value; }
        }

        /// <summary>
        /// A normalized value (about 0 to about 1) that represents the current level of intensity.
        /// </summary>
        public float NormalizedFadeTime
        { get { return m_CurrentFadeTime; } }

        bool IsShaking
        { get { return m_CurrentFadeTime > 0 || m_Sustain; } }

        bool IsFadingOut
        { get { return !m_Sustain && m_CurrentFadeTime > 0; } }

        bool IsFadingIn
        { get { return m_CurrentFadeTime < 1 && m_Sustain && m_FadeInDuration > 0; } }

        /// <summary>
        /// Gets the current state of the shake.
        /// </summary>
        public CameraShakeState CurrentState
        {
            get
            {
                if (IsFadingIn)
				{
					return CameraShakeState.FadingIn;
				}
				else if (IsFadingOut)
				{
					return CameraShakeState.FadingOut;
				}
				else if (IsShaking)
				{
					return CameraShakeState.Sustained;
				}
				else
				{
					return CameraShakeState.Inactive;
				}
			}
        }
    }
}