using System;

namespace Core
{
	public static class MathUtil
	{
		public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
		{
			return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
		}

		public static T Min<T>(T a, T b) where T : IComparable<T>
		{
			return MinInternal(a, b);
		}

		public static T Min<T>(params T[] values) where T : IComparable<T>
		{
			return MinInternal(values);
		}

		public static T Max<T>(T a, T b) where T : IComparable<T>
		{
			return MaxInternal(a, b);
		}

		public static T Max<T>(params T[] values) where T : IComparable<T>
		{
			return MaxInternal(values);
		}

		public static float ConvertRange(this float value, float oldMin, float oldMax, float newMin, float newMax)
		{
			return ((value - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
		}

		private static T MinInternal<T>(params T[] values) where T : IComparable<T>
		{
			if (values == null || values.Length <= 0)
			{
				return default(T);
			}
			T min = values[0];
			for (int i = 1; i < values.Length; ++i)
			{
				if (values[i].CompareTo(min) < 0)
				{
					min = values[i];
				}
			}
			return min;
		}

		private static T MaxInternal<T>(params T[] values) where T : IComparable<T>
		{
			if (values == null || values.Length <= 0)
			{
				return default(T);
			}
			T max = values[0];
			for (int i = 1; i < values.Length; ++i)
			{
				if (values[i].CompareTo(max) > 0)
				{
					max = values[i];
				}
			}
			return max;
		}
	}
}