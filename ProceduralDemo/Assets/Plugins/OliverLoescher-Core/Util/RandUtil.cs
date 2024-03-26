using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
	public static class Random2
	{
		public static float Range(Vector2 pRange) => UnityEngine.Random.Range(pRange.x, pRange.y);
		public static int Range(Vector2Int pRange) => UnityEngine.Random.Range(pRange.x, pRange.y);
		public static float Range(float pRange) => UnityEngine.Random.Range(-pRange, pRange);
		public static int Range(int pRange) => UnityEngine.Random.Range(-pRange, pRange);

		public static Vector2 GetRandomPointInEllipse(float ellipse_width, float ellipse_height)
		{
			float t = 2 * Mathf.PI * UnityEngine.Random.value;
			float u = UnityEngine.Random.value + UnityEngine.Random.value;
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