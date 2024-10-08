﻿using UnityEngine;
using System.Collections.Generic;

namespace EZCameraShake
{
	[AddComponentMenu("EZ Camera Shake/Camera Shaker")]
	public class CameraShaker : MonoBehaviour
	{
		/// <summary>
		/// The single instance of the CameraShaker in the current scene. Do not use if you have multiple instances.
		/// </summary>
		public static CameraShaker s_Instance;
		private static readonly Dictionary<int, CameraShaker> s_InstanceList = new();

		/// <summary>
		/// The default position influcence of all shakes created by this shaker.
		/// </summary>
		public Vector3 DefaultPosInfluence = new(0.15f, 0.15f, 0.15f);
		/// <summary>
		/// The default rotation influcence of all shakes created by this shaker.
		/// </summary>
		public Vector3 DefaultRotInfluence = new(1, 1, 1);
		/// <summary>
		/// Offset that will be applied to the camera's default (0,0,0) rest position
		/// </summary>
		public Vector3 RestPositionOffset = new(0, 0, 0);
		/// <summary>
		/// Offset that will be applied to the camera's default (0,0,0) rest rotation
		/// </summary>
		public Vector3 RestRotationOffset = new(0, 0, 0);

		private Vector3 m_PosAddShake;
		private Vector3 m_RotAddShake;
		private readonly List<CameraShakeInstance> m_CameraShakeInstances = new();

		void OnEnable()
		{
			s_Instance = this;
			s_InstanceList.Add(gameObject.GetInstanceID(), this);
		}

		void OnDisable()
		{
			if (s_Instance == this)
			{
				s_Instance = null;
			}
			s_InstanceList.Remove(gameObject.GetInstanceID());
		}

		void OnDestroy()
		{
			s_InstanceList.Remove(gameObject.GetInstanceID());
		}

		void Update()
		{
			m_PosAddShake = Vector3.zero;
			m_RotAddShake = Vector3.zero;

			for (int i = 0; i < m_CameraShakeInstances.Count; i++)
			{
				if (i >= m_CameraShakeInstances.Count)
				{
					break;
				}

				CameraShakeInstance c = m_CameraShakeInstances[i];

				if (c.CurrentState == CameraShakeState.Inactive && c.DeleteOnInactive)
				{
					m_CameraShakeInstances.RemoveAt(i);
					i--;
				}
				else if (c.CurrentState != CameraShakeState.Inactive)
				{
					m_PosAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.PositionInfluence);
					m_RotAddShake += CameraUtilities.MultiplyVectors(c.UpdateShake(), c.RotationInfluence);
				}
			}

			transform.localPosition = m_PosAddShake + RestPositionOffset;
			transform.localEulerAngles = m_RotAddShake + RestRotationOffset;
		}

		/// <summary>
		/// Gets the CameraShaker with the given name, if it exists.
		/// </summary>
		/// <param name="name">The name of the camera shaker instance.</param>
		/// <returns></returns>
		public static CameraShaker GetInstance(int instanceID)
		{

			if (s_InstanceList.TryGetValue(instanceID, out CameraShaker c))
			{
				return c;
			}
			Debug.LogError($"CameraShake {instanceID} not found!");
			return null;
		}

		/// <summary>
		/// Starts a shake using the given preset.
		/// </summary>
		/// <param name="shake">The preset to use.</param>
		/// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
		public CameraShakeInstance Shake(CameraShakeInstance shake)
		{
			m_CameraShakeInstances.Add(shake);
			return shake;
		}

		/// <summary>
		/// Shake the camera once, fading in and out  over a specified durations.
		/// </summary>
		/// <param name="magnitude">The intensity of the shake.</param>
		/// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
		/// <param name="fadeInTime">How long to fade in the shake, in seconds.</param>
		/// <param name="fadeOutTime">How long to fade out the shake, in seconds.</param>
		/// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
		public CameraShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime)
		{
			CameraShakeInstance shake = new(magnitude, roughness, fadeInTime, fadeOutTime)
			{
				PositionInfluence = DefaultPosInfluence,
				RotationInfluence = DefaultRotInfluence
			};
			m_CameraShakeInstances.Add(shake);

			return shake;
		}

		/// <summary>
		/// Shake the camera once, fading in and out over a specified durations.
		/// </summary>
		/// <param name="magnitude">The intensity of the shake.</param>
		/// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
		/// <param name="fadeInTime">How long to fade in the shake, in seconds.</param>
		/// <param name="fadeOutTime">How long to fade out the shake, in seconds.</param>
		/// <param name="posInfluence">How much this shake influences position.</param>
		/// <param name="rotInfluence">How much this shake influences rotation.</param>
		/// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
		public CameraShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 posInfluence, Vector3 rotInfluence)
		{
			CameraShakeInstance shake = new(magnitude, roughness, fadeInTime, fadeOutTime)
			{
				PositionInfluence = posInfluence,
				RotationInfluence = rotInfluence
			};
			m_CameraShakeInstances.Add(shake);

			return shake;
		}

		/// <summary>
		/// Start shaking the camera.
		/// </summary>
		/// <param name="magnitude">The intensity of the shake.</param>
		/// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
		/// <param name="fadeInTime">How long to fade in the shake, in seconds.</param>
		/// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
		public CameraShakeInstance StartShake(float magnitude, float roughness, float fadeInTime)
		{
			CameraShakeInstance shake = new(magnitude, roughness)
			{
				PositionInfluence = DefaultPosInfluence,
				RotationInfluence = DefaultRotInfluence
			};
			shake.StartFadeIn(fadeInTime);
			m_CameraShakeInstances.Add(shake);
			return shake;
		}

		/// <summary>
		/// Start shaking the camera.
		/// </summary>
		/// <param name="magnitude">The intensity of the shake.</param>
		/// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
		/// <param name="fadeInTime">How long to fade in the shake, in seconds.</param>
		/// <param name="posInfluence">How much this shake influences position.</param>
		/// <param name="rotInfluence">How much this shake influences rotation.</param>
		/// <returns>A CameraShakeInstance that can be used to alter the shake's properties.</returns>
		public CameraShakeInstance StartShake(float magnitude, float roughness, float fadeInTime, Vector3 posInfluence, Vector3 rotInfluence)
		{
			CameraShakeInstance shake = new(magnitude, roughness)
			{
				PositionInfluence = posInfluence,
				RotationInfluence = rotInfluence
			};
			shake.StartFadeIn(fadeInTime);
			m_CameraShakeInstances.Add(shake);
			return shake;
		}

		/// <summary>
		/// Gets a copy of the list of current camera shake instances.
		/// </summary>
		public List<CameraShakeInstance> ShakeInstances => new(m_CameraShakeInstances);
	}
}