using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace OliverLoescher.Util
{
    public static class Anim
    {
		public interface IAnimation
		{
			bool IsComplete { get; }

			bool Tick(float pDeltaTime); // Return if complete
			void Cancel();
		}

        public delegate void TickEvent(float pValue);
        public delegate void Tick2DEvent(Vector2 pValue);

		private static readonly List<IAnimation> s_Animations = new();

		private static Mono.Updateable s_Updateable = new(Mono.Type.Early, Mono.Priorities.ModelController);
		private static bool s_IsInitalized = false;

		private static void Initalize()
		{
			if (Func.IsApplicationQuitting || s_IsInitalized)
			{
				return;
			}
			s_Animations.Clear();
			s_IsInitalized = true;
			s_Updateable.Register(Tick);
			Application.quitting += OnQuit;
		}

		private static void OnQuit()
		{
			s_Animations.Clear();
			s_IsInitalized = false;
			s_Updateable.Deregister();
			Application.quitting -= OnQuit;
		}

		private static void Tick(float pDeltaTime)
		{
			if (s_Animations.IsNullOrEmpty())
			{
				return;
			}
			Profiler.BeginSample("AnimUtil.Tick()");
			for (int i = s_Animations.Count - 1; i >= 0; i--)
			{
				if (s_Animations[i].Tick(pDeltaTime))
				{
					s_Animations.RemoveAt(i);
					continue;
				}
			}
			Profiler.EndSample();
		}

        public static void Play(Easing.EaseParams pEase, float pSeconds, TickEvent pOnTick, TickEvent pOnComplete = null)
		{
			if (pOnTick == null)
			{
				Debug2.DevException("pOnTick can not be null", nameof(Play), typeof(Anim));
				return;
			}
			Initalize();
			s_Animations.Add(new AnimUtilEase(pEase, pSeconds, pOnTick, pOnComplete));
		}
        public static void Play(Easing.Method pMethod, Easing.Direction pDirection, float pSeconds, TickEvent pOnTick, TickEvent pOnComplete = null)
			=> Play(new Easing.EaseParams(pMethod, pDirection), pSeconds, pOnTick, pOnComplete);


		public static void Play2D(Easing.EaseParams pEaseX, Easing.EaseParams pEaseY, float pSeconds, Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null)
		{
			if (pOnTick == null)
			{
				Debug2.DevException("pOnTick can not be null", nameof(Play), typeof(Anim));
				return;
			}
			Initalize();
			s_Animations.Add(new AnimUtilEase2D(pEaseX, pEaseY, pSeconds, pOnTick, pOnComplete));
		}
		public static void Play2D(Easing.Method pMethodX, Easing.Direction pDirectionX, Easing.Method pMethodY, Easing.Direction pDirectionY, float pSeconds, Tick2DEvent pOnTick = null, Tick2DEvent pOnComplete = null)
			=> Play2D(new Easing.EaseParams(pMethodX, pDirectionX), new Easing.EaseParams(pMethodY, pDirectionY), pSeconds, pOnTick, pOnComplete);
    }
}
