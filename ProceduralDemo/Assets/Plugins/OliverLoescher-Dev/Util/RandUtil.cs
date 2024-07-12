using UnityEngine;
using Rand = UnityEngine.Random;

namespace ODev.Util
{
	public static class Random
	{
		public static float Range(float minInclusive, float maxInclusive) => Rand.Range(minInclusive, maxInclusive);
		public static int Range(int minInclusive, int maxExclusive) => Rand.Range(minInclusive, maxExclusive);
		public static float Range(Vector2 pRange) => Rand.Range(pRange.x, pRange.y);
		public static int Range(Vector2Int pRange) => Rand.Range(pRange.x, pRange.y);
		public static float Range(float pRange) => Rand.Range(-pRange, pRange);
		public static int Range(int pRange) => Rand.Range(-pRange, pRange);

		public static Vector2 GetRandomPointInEllipse(float ellipse_width, float ellipse_height)
		{
			float t = 2 * Mathf.PI * Rand.value;
			float u = Rand.value + Rand.value;
			float r;
			if (u > 1)
			{
				r = 2 - u;
			}
			else
			{
				r = u;
			}
			return new Vector2(ellipse_width * r * Mathf.Cos(t) / 2, ellipse_height * r * Mathf.Sin(t) / 2);
		}
		public static Vector2 GetRandomPointOnCircle(float pRadius) => GetRandomPointInEllipse(pRadius, pRadius);
	}
}