using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
	public static class Easing
	{
		[System.Serializable]
		public struct EaseParams
		{
			[SerializeField]
			public Method Method;
			[SerializeField]
			public Direction Direction;
			
			public EaseParams(Method pMethod, Direction pDirection)
			{
				Method = pMethod;
				Direction = pDirection;
			}
		}

		public enum Method
		{
			Linear,
			Sine,
			Quad,
			Cubic,
			Quart,
			Quint,
			Expo,
			Circ,
			Back,
			Elastic,
			Bounce,
		}

		public enum Direction
		{
			In,
			Out,
			InOut
		}

		public static float InSine(float x)
		{
			return 1.0f - Mathf.Cos((x * Mathf.PI) / 2.0f);
		}

		public static float OutSine(float x)
		{
			return Mathf.Sin((x * Mathf.PI) / 2.0f);
		}

		public static float InOutSine(float x)
		{
			return -(Mathf.Cos(Mathf.PI * x) - 1.0f) / 2.0f;
		}

		public static float InQuad(float x)
		{
			return x * x;
		}

		public static float OutQuad(float x)
		{
			return 1.0f - (1.0f - x) * (1.0f - x);
		}

		public static float InOutQuad(float x)
		{
			return x < 0.5f ? 2.0f * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 2.0f) / 2.0f;
		}

		public static float InCubic(float x)
		{
			return x * x * x;
		}

		public static float OutCubic(float x)
		{
			return 1.0f - Mathf.Pow(1.0f - x, 3.0f);
		}

		public static float InOutCubic(float x)
		{
			return x < 0.5f ? 4.0f * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 3.0f) / 2.0f;
		}

		public static float InQuart(float x)
		{
			return x * x * x * x;
		}

		public static float OutQuart(float x)
		{
			return 1.0f - Mathf.Pow(1.0f - x, 4.0f);
		}

		public static float InOutQuart(float x)
		{
			return x < 0.5f ? 8.0f * x * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 4.0f) / 2.0f;
		}

		public static float InQuint(float x)
		{
			return x * x * x * x * x;
		}

		public static float OutQuint(float x)
		{
			return 1.0f - Mathf.Pow(1.0f - x, 5.0f);
		}

		public static float InOutQuint(float x)
		{
			return x < 0.5f ? 16.0f * x * x * x * x * x : 1.0f - Mathf.Pow(-2.0f * x + 2.0f, 5.0f) / 2.0f;
		}

		public static float InExpo(float x)
		{
			return x == 0.0f ? 0.0f : Mathf.Pow(2.0f, 10.0f * x - 10.0f);
		}

		public static float OutExpo(float x)
		{
			return x == 1.0f ? 1.0f : 1.0f - Mathf.Pow(2.0f, -10.0f * x);
		}

		public static float InOutExpo(float x)
		{
			return x == 0.0f ? 0.0f : x == 1.0f ? 1.0f : x < 0.5f ?
				Mathf.Pow(2.0f, 20.0f * x - 10.0f) / 2.0f :
				(2.0f - Mathf.Pow(2.0f, -20.0f * x + 10.0f)) / 2.0f;
		}

		public static float InCirc(float x)
		{
			return 1.0f - Mathf.Sqrt(1.0f - Mathf.Pow(x, 2.0f));
		}

		public static float OutCirc(float x)
		{
			return Mathf.Sqrt(1.0f - Mathf.Pow(x - 1.0f, 2.0f));
		}

		public static float InOutCirc(float x)
		{
			return x < 0.5f ? (1.0f - Mathf.Sqrt(1.0f - Mathf.Pow(2.0f * x, 2.0f))) / 2.0f :
				(Mathf.Sqrt(1.0f - Mathf.Pow(-2.0f * x + 2.0f, 2.0f)) + 1.0f) / 2.0f;
		}

		public static float InBack(float x)
		{
			const float c1 = 1.70158f;
			const float c3 = c1 + 1.0f;
			return c3 * x * x * x - c1 * x * x;
		}

		public static float OutBack(float x)
		{
			const float c1 = 1.70158f;
			const float c3 = c1 + 1.0f;
			return 1.0f + c3 * Mathf.Pow(x - 1.0f, 3.0f) + c1 * Mathf.Pow(x - 1.0f, 2.0f);
		}

		public static float InOutBack(float x)
		{
			const float c1 = 1.70158f;
			const float c2 = c1 * 1.525f;

			return x < 0.5f
			? (Mathf.Pow(2.0f * x, 2.0f) * ((c2 + 1.0f) * 2.0f * x - c2)) / 2.0f
			: (Mathf.Pow(2.0f * x - 2.0f, 2.0f) * ((c2 + 1.0f) * (x * 2.0f - 2.0f) + c2) + 2.0f) / 2.0f;
		}

		public static float InElastic(float x)
		{
			const float c4 = (2.0f * Mathf.PI) / 3.0f;
			return x == 0.0f ? 0.0f : x == 1.0f ? 1.0f :
				-Mathf.Pow(2.0f, 10.0f * x - 10.0f) * Mathf.Sin((x * 10.0f - 10.75f) * c4);
		}

		public static float OutElastic(float x)
		{
			const float c4 = (2.0f * Mathf.PI) / 3;

			return x == 0.0f ? 0.0f : x == 1.0f ? 1.0f :
				Mathf.Pow(2.0f, -10.0f * x) * Mathf.Sin((x * 10.0f - 0.75f) * c4) + 1.0f;
		}

		public static float InOutElastic(float x)
		{
			const float c5 = (2.0f * Mathf.PI) / 4.5f;

			return x == 0.0f ? 0.0f : x == 1.0f ? 1.0f : x < 0.5f ?
				-(Mathf.Pow(2.0f, 20.0f * x - 10.0f) * Mathf.Sin((20.0f * x - 11.125f) * c5)) / 2.0f :
				(Mathf.Pow(2.0f, -20.0f * x + 10.0f) * Mathf.Sin((20.0f * x - 11.125f) * c5)) / 2.0f + 1.0f;
		}

		public static float InBounce(float x)
		{
			return 1.0f - OutBounce(1.0f - x);
		}

		public static float OutBounce(float x)
		{
			const float n1 = 7.5625f;
			const float d1 = 2.75f;

			if (x < 1.0f / d1)
			{
				return n1 * x * x;
			}
			else if (x < 2.0f / d1)
			{
				return n1 * (x -= 1.5f / d1) * x + 0.75f;
			}
			else if (x < 2.5f / d1)
			{
				return n1 * (x -= 2.25f / d1) * x + 0.9375f;
			}
			else
			{
				return n1 * (x -= 2.625f / d1) * x + 0.984375f;
			}
		}

		public static float InOutBounce(float x)
		{
			return x < 0.5f ?
				(1.0f - OutBounce(1.0f - 2.0f * x)) / 2.0f :
				(1.0f + OutBounce(2.0f * x - 1.0f)) / 2.0f;
		}

		public static float Ease(float x, Method method, Direction direction)
		{
			switch (direction)
			{
				case Direction.In:
					return EaseIn(x, method);
				case Direction.Out:
					return EaseOut(x, method);
				case Direction.InOut:
					return EaseInOut(x, method);
				default:
					throw new System.NotImplementedException();
			}
		}
		public static float Ease(this EaseParams ease, float x) => Ease(x, ease.Method, ease.Direction);

		public static float EaseIn(float x, Method method)
		{
			switch (method)
			{
				case Method.Linear:
					return x;
				case Method.Sine:
					return InSine(x);
				case Method.Quad:
					return InQuad(x);
				case Method.Cubic:
					return InCubic(x);
				case Method.Quart:
					return InQuart(x);
				case Method.Quint:
					return InQuint(x);
				case Method.Expo:
					return InExpo(x);
				case Method.Circ:
					return InCirc(x);
				case Method.Back:
					return InBack(x);
				case Method.Elastic:
					return InElastic(x);
				case Method.Bounce:
					return InBounce(x);
				default:
					throw new System.NotImplementedException();
			}
		}

		public static float EaseOut(float x, Method method)
		{
			switch (method)
			{
				case Method.Linear:
					return x;
				case Method.Sine:
					return OutSine(x);
				case Method.Quad:
					return OutQuad(x);
				case Method.Cubic:
					return OutCubic(x);
				case Method.Quart:
					return OutQuart(x);
				case Method.Quint:
					return OutQuint(x);
				case Method.Expo:
					return OutExpo(x);
				case Method.Circ:
					return OutCirc(x);
				case Method.Back:
					return OutBack(x);
				case Method.Elastic:
					return OutElastic(x);
				case Method.Bounce:
					return OutBounce(x);
				default:
					throw new System.NotImplementedException();
			}
		}

		public static float EaseInOut(float x, Method method)
		{
			switch (method)
			{
				case Method.Linear:
					return x;
				case Method.Sine:
					return InOutSine(x);
				case Method.Quad:
					return InOutQuad(x);
				case Method.Cubic:
					return InOutCubic(x);
				case Method.Quart:
					return InOutQuart(x);
				case Method.Quint:
					return InOutQuint(x);
				case Method.Expo:
					return InOutExpo(x);
				case Method.Circ:
					return InOutCirc(x);
				case Method.Back:
					return InOutBack(x);
				case Method.Elastic:
					return InOutElastic(x);
				case Method.Bounce:
					return InOutBounce(x);
				default:
					throw new System.NotImplementedException();
			}
		}
	}
}