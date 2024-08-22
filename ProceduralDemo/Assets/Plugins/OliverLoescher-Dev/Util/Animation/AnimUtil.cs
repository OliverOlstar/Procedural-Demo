using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace ODev.Util
{
	public static class Anim
	{
		public interface IAnimation
		{
			bool IsComplete { get; }
			void Cancel(bool pCallOnComplete = true);
		}
		public interface IAnimationInternal : IAnimation
		{
			bool Tick(float pDeltaTime); // Return if complete
		}

		public enum Type
		{
			Physics,
			Visual
		}

		public static AnimationCurve DefaultAnimationCurve => new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

		public delegate void TickEvent(float pValue);
		public delegate void Tick2DEvent(Vector2 pValue);

		private static readonly List<IAnimationInternal> s_VisualAnimations = new();
		private static readonly List<IAnimationInternal> s_PhysicsAnimations = new();

		private static Mono.Updateable s_Updateable = new(Mono.Type.Late, Mono.Priorities.ModelController);
		private static Mono.Updateable s_FixedUpdateable = new(Mono.Type.Fixed, Mono.Priorities.World);
		private static bool s_IsInitalized = false;

		private static void Initalize()
		{
			if (Func.IsApplicationQuitting || s_IsInitalized)
			{
				return;
			}
			s_VisualAnimations.Clear();
			s_PhysicsAnimations.Clear();
			s_IsInitalized = true;
			s_Updateable.Register((float pDeltaTime) => Tick(pDeltaTime, s_VisualAnimations, "VisualTick"));
			s_FixedUpdateable.Register((float pDeltaTime) => Tick(pDeltaTime, s_PhysicsAnimations, "PhysicsTick"));
			Application.quitting += OnQuit;
		}

		private static void OnQuit()
		{
			s_VisualAnimations.Clear();
			s_PhysicsAnimations.Clear();
			s_IsInitalized = false;
			s_Updateable.Deregister();
			s_FixedUpdateable.Deregister();
			Application.quitting -= OnQuit;
		}

		private static void Tick(float pDeltaTime, in IList<IAnimationInternal> pAnimations, string pDebugMethodName)
		{
			if (pAnimations.IsNullOrEmpty())
			{
				return;
			}
			Profiler.BeginSample($"AnimUtil.{pDebugMethodName}()");
			for (int i = pAnimations.Count - 1; i >= 0; i--)
			{
				if (pAnimations[i].Tick(pDeltaTime))
				{
					pAnimations.RemoveAt(i);
					continue;
				}
			}
			Profiler.EndSample();
		}

		private static IAnimation PlayInternal(IAnimationInternal pAnim, Type pType)
		{
			Initalize();
			switch (pType)
			{
				case Type.Physics:
					s_PhysicsAnimations.Add(pAnim);
					break;
				case Type.Visual:
					s_VisualAnimations.Add(pAnim);
					break;
				default:
					Debug.DevException(typeof(Anim), new NotImplementedException());
					break;
			}
			return pAnim;
		}

		public static IAnimation Play(Easing.EaseParams pEase, float pSeconds, Type pType,
			TickEvent pOnTick, TickEvent pOnComplete = null, float pDelay = 0.0f)
			=> PlayInternal(new AnimUtilEase(pEase, pSeconds, pOnTick, pOnComplete, pDelay), pType);
		public static IAnimation Play(Easing.Method pMethod, Easing.Direction pDirection, float pSeconds, Type pType,
			TickEvent pOnTick, TickEvent pOnComplete = null, float pDelay = 0.0f)
			=> Play(new Easing.EaseParams(pMethod, pDirection), pSeconds, pType, pOnTick, pOnComplete, pDelay);

		public static IAnimation Play2D(Easing.EaseParams pEaseX, Easing.EaseParams pEaseY, float pSeconds, Type pType,
			Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null, float pDelay = 0.0f)
			=> PlayInternal(new AnimUtilEase2D(pEaseX, pEaseY, pSeconds, pOnTick, pOnComplete, pDelay), pType);
		public static IAnimation Play2D(Easing.Method pMethodX, Easing.Direction pDirectionX, Easing.Method pMethodY, Easing.Direction pDirectionY, float pSeconds, Type pType,
			Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null, float pDelay = 0.0f)
			=> Play2D(new Easing.EaseParams(pMethodX, pDirectionX), new Easing.EaseParams(pMethodY, pDirectionY), pSeconds, pType, pOnTick, pOnComplete, pDelay);
	}
}
