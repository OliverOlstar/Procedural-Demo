using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Profiling;

namespace OCore.Util
{
	public static class Anim
    {
		public interface IAnimation
		{
			bool IsComplete { get; }
			void Cancel();
		}
		public interface IAnimationInternal : IAnimation
		{
			bool Tick(float pDeltaTime); // Return if complete
		}

		public static AnimationCurve DefaultAnimationCurve => new(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        public delegate void TickEvent(float pValue);
        public delegate void Tick2DEvent(Vector2 pValue);

		private static readonly List<IAnimationInternal> s_Animations = new();

		private static Mono.Updateable s_Updateable = new(Mono.Type.Early, Mono.Priorities.ModelController);
		private static bool s_IsInitalized = false;

		private static void Initalize()
		{
			if (Func.s_IsApplicationQuitting || s_IsInitalized)
			{
				return;
			}
			s_Animations.Clear();
			s_IsInitalized = true;
			s_Updateable.Register((float pDeltaTime) => Tick(pDeltaTime, s_Animations));
			Application.quitting += OnQuit;
		}

		private static void OnQuit()
		{
			s_Animations.Clear();
			s_IsInitalized = false;
			s_Updateable.Deregister();
			Application.quitting -= OnQuit;
		}

		private static void Tick(float pDeltaTime, in IList<IAnimationInternal> pAnimations)
		{
			if (pAnimations.IsNullOrEmpty())
			{
				return;
			}
			Profiler.BeginSample("AnimUtil.Tick()");
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

		private static IAnimation PlayInternal(IAnimationInternal pAnim)
		{
			Initalize();
			s_Animations.Add(pAnim);
			return pAnim;
		}

        public static IAnimation Play(Easing.EaseParams pEase, float pSeconds, TickEvent pOnTick, TickEvent pOnComplete = null, float pDelay = 0.0f)
			=> PlayInternal(new AnimUtilEase(pEase, pSeconds, pOnTick, pOnComplete, pDelay));
        public static IAnimation Play(Easing.Method pMethod, Easing.Direction pDirection, float pSeconds, TickEvent pOnTick, TickEvent pOnComplete = null, float pDelay = 0.0f)
			=> Play(new Easing.EaseParams(pMethod, pDirection), pSeconds, pOnTick, pOnComplete, pDelay);

		public static IAnimation Play2D(Easing.EaseParams pEaseX, Easing.EaseParams pEaseY, float pSeconds, Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null, float pDelay = 0.0f)
			=> PlayInternal(new AnimUtilEase2D(pEaseX, pEaseY, pSeconds, pOnTick, pOnComplete, pDelay));
		public static IAnimation Play2D(Easing.Method pMethodX, Easing.Direction pDirectionX, Easing.Method pMethodY, Easing.Direction pDirectionY, float pSeconds, Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null, float pDelay = 0.0f)
			=> Play2D(new Easing.EaseParams(pMethodX, pDirectionX), new Easing.EaseParams(pMethodY, pDirectionY), pSeconds, pOnTick, pOnComplete, pDelay);
    }
}
