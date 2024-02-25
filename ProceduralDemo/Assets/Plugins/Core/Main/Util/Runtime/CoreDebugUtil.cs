
using System;
using UnityEngine;
using System.Text;
using UnityEditor;
using UnityEngine.Rendering;

namespace Core
{
	public static class DebugUtil
	{
		public static void DrawBounds(Bounds bounds, Color color, float duration = 0.0f)
		{
			Vector3 right = bounds.extents.x * Vector3.right;
			Vector3 up = bounds.extents.y * Vector3.up;
			Vector3 forward = bounds.extents.z * Vector3.forward;
			Vector3 a1 = bounds.center + right + up + forward;
			Vector3 a2 = bounds.center + right - up + forward;
			Vector3 a3 = bounds.center - right - up + forward;
			Vector3 a4 = bounds.center - right + up + forward;

			Vector3 b1 = bounds.center + right + up - forward;
			Vector3 b2 = bounds.center + right - up - forward;
			Vector3 b3 = bounds.center - right - up - forward;
			Vector3 b4 = bounds.center - right + up - forward;

			Debug.DrawLine(a1, a2, color, duration);
			Debug.DrawLine(a2, a3, color, duration);
			Debug.DrawLine(a3, a4, color, duration);
			Debug.DrawLine(a4, a1, color, duration);

			Debug.DrawLine(b1, b2, color, duration);
			Debug.DrawLine(b2, b3, color, duration);
			Debug.DrawLine(b3, b4, color, duration);
			Debug.DrawLine(b4, b1, color, duration);

			Debug.DrawLine(a1, b1, color, duration);
			Debug.DrawLine(a2, b2, color, duration);
			Debug.DrawLine(a3, b3, color, duration);
			Debug.DrawLine(a4, b4, color, duration);
		}


		public static void DrawBox(
			Vector3 position,
			Color color,
			float size,
			float duration = 0.0f)
		{
			float halfLength = size / 2.0f;
			Vector3 p0 = position + Vector3.forward * halfLength;
			Vector3 p1 = position + Vector3.up * halfLength;
			Vector3 p2 = position + Vector3.right * halfLength;
			Vector3 p3 = position - Vector3.forward * halfLength;
			Vector3 p4 = position - Vector3.up * halfLength;
			Vector3 p5 = position - Vector3.right * halfLength;
			Debug.DrawLine(p1, p0, color, duration);
			Debug.DrawLine(p1, p2, color, duration);
			Debug.DrawLine(p1, p3, color, duration);
			Debug.DrawLine(p1, p5, color, duration);

			Debug.DrawLine(p4, p0, color, duration);
			Debug.DrawLine(p4, p2, color, duration);
			Debug.DrawLine(p4, p3, color, duration);
			Debug.DrawLine(p4, p5, color, duration);

			Debug.DrawLine(p0, p2, color, duration);
			Debug.DrawLine(p2, p3, color, duration);
			Debug.DrawLine(p3, p5, color, duration);
			Debug.DrawLine(p5, p0, color, duration);
		}

		public static void DrawRay(
			DebugOption op,
			Vector3 start,
			Vector3 direction,
			Color color,
			float duration = 0.0f)
		{
			if (op.IsSet())
			{
				Debug.DrawRay(start, direction, color, duration);
			}
		}

		public static void DrawRay2(
			Vector2 start,
			Vector2 direction,
			float height,
			Color color,
			float duration = 0.0f)
		{
			Debug.DrawRay(Util.Vector2To3(start, height), Util.Vector2To3(direction, 0.0f), color, duration);
		}

		public static void DrawCircle(
			DebugOption op,
			Vector3 position,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			if (op.IsSet())
			{
				DrawCircle(
					position,
					radius,
					color,
					duration);
			}
		}

		public static void DrawCircle(
			Vector3 position,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			DrawCircle(position, Vector3.up, radius, color, duration);
		}


		public static void DrawCircle(
			Vector3 position,
			Vector3 normal,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			int divisions = 20;
			float inc = 360.0f / divisions;

			// Cross a plane normal with any non-parallel vector and you get a vector in the normal's plane
			float dot = Vector3.Dot(normal, Vector3.right);
			Vector3 cross = dot < 1.0f - Core.Util.EPSILON && dot > -1.0f + Core.Util.EPSILON ?
				Vector3.right :
				Vector3.forward;
			Vector3 vectorInPlane = Vector3.Cross(normal, cross);
			Core.Util.Normalize(ref vectorInPlane);

			Vector3 lastV = vectorInPlane * radius;
			for (int i = 1; i <= divisions; i++)
			{
				Vector3 v = Quaternion.AngleAxis(inc, normal) * lastV;
				Debug.DrawLine(position + lastV, position + v, color, duration);
				lastV = v;
			}
		}

		public static void DrawSphere(Vector3 position, float radius, Color color, float duration = 0.0f)
		{
			DrawCircle(position, Vector3.up, radius, color, duration);
			DrawCircle(position, Vector3.right, radius, color, duration);
			DrawCircle(position, Vector3.forward, radius, color, duration);
		}

		public static void DrawCircle2(
			Vector2 position,
			float height,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			DrawCircle(Core.Util.Vector2To3(position, height), radius, color, duration);
		}

		public static void DrawLine2(
			DebugOption op,
			Vector2 p1,
			Vector2 p2,
			float height,
			Color color,
			float duration = 0.0f)
		{
			if (op.IsSet())
			{
				DrawLine2(
					p1,
					p2,
					height,
					color,
					duration);
			}
		}

		public static void DrawLine2(
			DebugOption op,
			Vector3 p1,
			Vector3 p2,
			Color color,
			float duration = 0.0f)
		{
			if (op.IsSet()) Debug.DrawLine(p1, p2, color, duration);
		}

		public static void DrawLine2(
			Vector2 p1,
			Vector2 p2,
			float height,
			Color color,
			float duration = 0.0f)
		{
			Debug.DrawLine(Core.Util.Vector2To3(p1, height), Core.Util.Vector2To3(p2, height), color, duration);
		}

//		public static void DrawTransform(Transform transform, float duration)
//		{
//			DrawTransform(new SimpleTransform(transform), duration);
//		}

	//	public static void DrawTransform(SimpleTransform transform, float duration)
	//	{
	//		Debug.DrawRay(transform.GetPosition(), transform.GetForward(), Color.blue, duration);
	//		Debug.DrawRay(transform.GetPosition(), transform.GetUp(), Color.green, duration);
	//		Debug.DrawRay(transform.GetPosition(), transform.GetRight(), Color.red, duration);
	//	}

		public static string GetScenePath(GameObject obj)
		{
			Transform parent = obj.transform.parent;
			StringBuilder str = new StringBuilder();
			while (parent != null)
			{
				str.Insert(0, parent.name).Insert(0, ".");
				parent = parent.parent;
			}
			return str.ToString();
		}

		/// <summary>Includes Scene name as well as GameObject name in addition to path</summary>
		public static string GetFullScenePath(GameObject obj)
		{
			string sceneName = obj.scene != null ? $"{obj.scene.name}" : "";
			if (obj.transform.parent == null)
			{
				return $"{sceneName}.{obj.name}";
			}
			string path = GetScenePath(obj);
			return $"{sceneName}{path}.{obj.name}";
		}

		public static void DrawRectScreenSpace(Rect rect, Color color, float duration = 0.0f)
		{
			DrawLineScreenSpace(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), color, duration);
			DrawLineScreenSpace(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), color, duration);
			DrawLineScreenSpace(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), color, duration);
			DrawLineScreenSpace(new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin), color, duration);
		}

		public static void DrawLineScreenSpace(
			Vector3 p1,
			Vector3 p2,
			Color color,
			float duration = 0.0f)
		{
			Ray r1 = Camera.main.ScreenPointToRay(p1);
			Ray r2 = Camera.main.ScreenPointToRay(p2);
			Debug.DrawLine(
				r1.GetPoint(Camera.main.nearClipPlane),
				r2.GetPoint(Camera.main.nearClipPlane),
				color,
				duration);

		}

		public static void DrawArc2(
			Vector2 position,
			Vector2 direction,
			float degrees,
			float height,
			Color color,
			float duration = 0.0f)
		{
			DrawArc(
				Core.Util.Vector2To3(position, height),
				Vector3.up,
				Core.Util.Vector2To3(direction),
				degrees,
				color,
				duration);
		}

		public static void DrawArc(
			Vector3 position,
			Vector3 normal,
			Vector3 direction,
			float degrees,
			Color color,
			float duration = 0.0f)
		{
			int steps = (int)(degrees / 5.0f) + 1;
			float inc = degrees / steps;
			Vector3 lastPos = position;
			for (int i = 0; i <= steps; i++)
			{
				float angle = -0.5f * degrees + i * inc;
				Vector3 v = Quaternion.AngleAxis(angle, normal) * direction;
				Debug.DrawLine(lastPos, position + v, color, duration);
				lastPos = position + v;
			}
			Debug.DrawLine(lastPos, position, color, duration);
		}

		private static void DrawArcCircumferance(
			Vector3 position,
			Vector3 normal,
			Vector3 direction,
			float degrees,
			Color color,
			float duration)
		{
			int steps = (int)(degrees / 5.0f) + 1;
			float inc = degrees / steps;
			Vector3 lastPos = Vector3.zero;
			for (int i = 0; i <= steps; i++)
			{
				float angle = -0.5f * degrees + i * inc;
				Vector3 v = Quaternion.AngleAxis(angle, normal) * direction;
				if (i > 0)
				{
					Debug.DrawLine(lastPos, position + v, color, duration);
				}
				lastPos = position + v;
			}
		}

		public static void DrawSquare(Vector3 position, float halfWidth, Color color, float duration = 0.0f)
		{
			Vector3 p1 = position + halfWidth * Vector3.forward + halfWidth * Vector3.right;
			Vector3 p2 = position + halfWidth * Vector3.forward + halfWidth * Vector3.left;
			Vector3 p3 = position + halfWidth * Vector3.back + halfWidth * Vector3.left;
			Vector3 p4 = position + halfWidth * Vector3.back + halfWidth * Vector3.right;
			Debug.DrawLine(p1, p2, color, duration);
			Debug.DrawLine(p2, p3, color, duration);
			Debug.DrawLine(p3, p4, color, duration);
			Debug.DrawLine(p4, p1, color, duration);
		}

		public static void DrawRect(Vector3 position, float halfWidth, float halfLength, Color color, float duration = 0.0f)
		{
			Vector3 p1 = position + halfLength * Vector3.forward + halfWidth * Vector3.right;
			Vector3 p2 = position + halfLength * Vector3.forward + halfWidth * Vector3.left;
			Vector3 p3 = position + halfLength * Vector3.back + halfWidth * Vector3.left;
			Vector3 p4 = position + halfLength * Vector3.back + halfWidth * Vector3.right;
			Debug.DrawLine(p1, p2, color, duration);
			Debug.DrawLine(p2, p3, color, duration);
			Debug.DrawLine(p3, p4, color, duration);
			Debug.DrawLine(p4, p1, color, duration);
		}

		public static void DrawCapsule2(Vector2 p1, Vector2 p2, float height, float radius, Color color, float duration = 0.0f)
		{
			DrawCircle2(p1, height, radius, color, duration);
			DrawCircle2(p2, height, radius, color, duration);
			Vector2 forward = (p2 - p1).normalized;
			Vector2 right = radius * Core.Util.Rotate2D(forward, 90 * Mathf.Deg2Rad);

			DrawLine2(p1 + right, p2 + right, height, color, duration);
			DrawLine2(p1 - right, p2 - right, height, color, duration);
		}

		public static void DrawCapsule2(Vector2 p1, Vector2 dir, float height, float length, float radius, Color color, float duration = 0.0f)
		{
			DrawCapsule2(p1, p1 + length * dir, height, radius, color, duration);
		}

		public static void DrawCapsule2(Vector3 p1, Vector3 p2, Vector3 up, float radius, Color color, float duration = 0.0f)
		{
			DrawCircle(p1, up, radius, color, duration);
			DrawCircle(p2, up, radius, color, duration);
			Vector3 forward = Vector3.Normalize(p2 - p1);
			Vector3 right = radius * Vector3.Cross(forward, up);
			Debug.DrawLine(p1 + right, p2 + right, color, duration);
			Debug.DrawLine(p1 - right, p2 - right, color, duration);
		}

		private static void DrawCapsuleInternal(
			Vector3 p1,
			Vector3 direction,
			float length,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			DrawCylinderInternal(p1, direction, length, radius, 4, color, duration, out Vector3 p2, out Vector3 perpendicular);
			DrawArcCircumferance(p2, perpendicular, direction * radius, 180.0f, color, duration);
			DrawArcCircumferance(p1, perpendicular, direction * -radius, 180.0f, color, duration);
			Vector3 perp2 = Quaternion.AngleAxis(90.0f, direction) * perpendicular;
			DrawArcCircumferance(p2, perp2, direction * radius, 180.0f, color, duration);
			DrawArcCircumferance(p1, perp2, direction * -radius, 180.0f, color, duration);
		}
		public static void DrawCapsule(Vector3 p1, Vector3 p2, float radius, Color color, float duration = 0.0f)
		{
			Vector3 direction = p2 - p1;
			float length = Core.Util.Normalize(ref direction);
			DrawCapsuleInternal(p1, direction, length, radius, color, duration);
		}
		public static void DrawCapsule(Vector3 root, Vector3 up, float height, float radius, Color color, float duration = 0.0f)
		{
			float length = height - 2.0f * radius;
			Vector3 p1 = root + up * radius;
			DrawCapsuleInternal(p1, up, length, radius, color, duration);
		}

		public static void DrawCylinder(
			Vector3 p1,
			Vector3 direction,
			float length,
			float radius,
			Color color,
			float duration = 0.0f)
		{
			// It looks nice to draw more length-wise lines as the cylinder radius get's larger, use some magic number math that looks good
			float circumference = 2.0f * Mathf.PI * radius;
			int divisions = Mathf.CeilToInt(circumference / 12.0f);
			divisions *= 4;
			DrawCylinderInternal(p1, direction, length, radius, divisions, color, duration, out _, out _);
		}
		public static void DrawCylinder(Vector3 p1, Vector3 p2, float radius, Color color, float duration = 0.0f)
		{
			Vector3 direction = p2 - p1;
			float length = Core.Util.Normalize(ref direction);
			DrawCylinder(p1, direction, length, radius, color, duration);
		}
		private static void DrawCylinderInternal(
			Vector3 p1,
			Vector3 direction,
			float length,
			float radius,
			int divisions,
			Color color,
			float duration,
			out Vector3 p2,
			out Vector3 perpendicular)
		{
			p2 = p1 + length * direction;

			float dot = Vector3.Dot(direction, Vector3.right);
			Vector3 cross = dot < 1.0f - Core.Util.EPSILON && dot > -1.0f + Core.Util.EPSILON ?
				Vector3.right :
				Vector3.forward;
			perpendicular = Vector3.Cross(direction, cross);
			Core.Util.Normalize(ref perpendicular);

			DrawCircle(p1, direction, radius, color, duration);
			DrawCircle(p2, direction, radius, color, duration);

			float inc = 360.0f / divisions;
			for (int i = 1; i <= divisions; i++)
			{
				float angle = inc * i;
				Vector3 v = Quaternion.AngleAxis(angle, direction) * perpendicular;
				v *= radius;
				Debug.DrawLine(p1 + v, p2 + v, color, duration);
			}
		}

		public static void DrawRectCentered(Vector3 center, Vector3 forward, Vector3 up, float halfWidth, float halfDepth, Color color, float duration = 0.0f)
		{
			Vector3 origin = center - forward * halfDepth;
			DrawRect(origin, forward, up, 2.0f * halfDepth, 2.0f * halfWidth, color, duration);
		}

		public static void DrawRect(Vector3 origin, Vector3 normalizedDirection, float length, float width, Color color, float duration = 0.0f)
		{
			DrawRect(origin, normalizedDirection, Vector3.up, length, width, color, duration);
		}

		public static void DrawRect(Vector3 origin, Vector3 normalizedDirection, Vector3 normal, float length, float width, Color color, float duration = 0.0f)
		{
			Vector3 right = 0.5f * width * Vector3.Cross(normalizedDirection, normal);
			Vector3 sr = origin + right;
			Vector3 sl = origin - right;
			Vector3 er = sr + length * normalizedDirection;
			Vector3 el = sl + length * normalizedDirection;
			Debug.DrawLine(sr, sl, color, duration);
			Debug.DrawLine(er, el, color, duration);
			Debug.DrawLine(sr, er, color, duration);
			Debug.DrawLine(sl, el, color, duration);
		}

		public static void Log(string s1)
		{
			Debug.Log(Core.Str.Build(s1));
		}
		public static void Log(string s1, string s2)
		{
			Debug.Log(Core.Str.Build(s1, s2));
		}
		public static void Log(string s1, string s2, string s3)
		{
			Debug.Log(Core.Str.Build(s1, s2, s3));
		}
		public static void Log(string s1, string s2, string s3, string s4)
		{
			Debug.Log(Core.Str.Build(s1, s2, s3, s4));
		}
		public static void Log(string s1, string s2, string s3, string s4, string s5)
		{
			Debug.Log(Core.Str.Build(s1, s2, s3, s4, s5));
		}
		public static void Log(params string[] s)
		{
			Debug.Log(Core.Str.Build(s));
		}

		public static void Warning(string s1)
		{
			Debug.LogWarning(Core.Str.Build(s1));
		}
		public static void Warning(string s1, string s2)
		{
			Debug.LogWarning(Core.Str.Build(s1, s2));
		}
		public static void Warning(string s1, string s2, string s3)
		{
			Debug.LogWarning(Core.Str.Build(s1, s2, s3));
		}
		public static void Warning(string s1, string s2, string s3, string s4)
		{
			Debug.LogWarning(Core.Str.Build(s1, s2, s3, s4));
		}
		public static void Warning(string s1, string s2, string s3, string s4, string s5)
		{
			Debug.LogWarning(Core.Str.Build(s1, s2, s3, s4, s5));
		}
		public static void Warning(params string[] s)
		{
			Debug.LogWarning(Core.Str.Build(s));
		}

		public static void Error(string s1)
		{
			Debug.LogError(Core.Str.Build(s1));
		}
		public static void Error(string s1, string s2)
		{
			Debug.LogError(Core.Str.Build(s1, s2));
		}
		public static void Error(string s1, string s2, string s3)
		{
			Debug.LogError(Core.Str.Build(s1, s2, s3));
		}
		public static void Error(string s1, string s2, string s3, string s4)
		{
			Debug.LogError(Core.Str.Build(s1, s2, s3, s4));
		}
		public static void Error(string s1, string s2, string s3, string s4, string s5)
		{
			Debug.LogError(Core.Str.Build(s1, s2, s3, s4, s5));
		}
		public static void Error(params string[] s)
		{
			Debug.LogError(Core.Str.Build(s));
		}

		public static void Log(DebugOption op, string s1)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s1));
		}
		public static void Log(DebugOption op, string s1, string s2)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s1, s2));
		}
		public static void Log(DebugOption op, string s1, string s2, string s3)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s1, s2, s3));
		}
		public static void Log(DebugOption op, string s1, string s2, string s3, string s4)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s1, s2, s3, s4));
		}
		public static void Log(DebugOption op, string s1, string s2, string s3, string s4, string s5)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s1, s2, s3, s4, s5));
		}
		public static void Log(DebugOption op, params string[] s)
		{
			if (op.IsSet()) Debug.Log(Core.Str.Build(s));
		}

		private static StringBuilder s_ColorStringBuilder = new StringBuilder();

		public static void Log(Color c, params string[] strings)
		{
			if (Application.isEditor) // No point making garbage to color strings on device
			{
				s_ColorStringBuilder.Clear();
				s_ColorStringBuilder.Append("<color=#");
				s_ColorStringBuilder.Append(ColorUtility.ToHtmlStringRGBA(c));
				s_ColorStringBuilder.Append(">");
				foreach (string s in strings)
				{
					s_ColorStringBuilder.Append(s);
				}
				s_ColorStringBuilder.Append("</color>");
				Debug.Log(s_ColorStringBuilder.ToString());
				return;
			}
			Log(strings);
		}

		public static string ColorString(Color c, string s)
		{
			s_ColorStringBuilder.Clear();
			s_ColorStringBuilder.Append("<color=#");
			s_ColorStringBuilder.Append(ColorUtility.ToHtmlStringRGBA(c));
			s_ColorStringBuilder.Append(">");
			s_ColorStringBuilder.Append(s);
			s_ColorStringBuilder.Append("</color>");
			return s_ColorStringBuilder.ToString();
		}
		public static void Log(Color c, string s) // No point making garbage to color strings on device
		{
			if (Application.isEditor)
			{
				Debug.Log(ColorString(c, s));
			}
			else
			{
				Debug.Log(s);
			}
		}

		public static void Log(DebugOption op, Color c, params string[] s)
		{
			if (op.IsSet()) Log(c, Core.Str.Build(s));
		}

		public static string VectorToString(Vector3 v)
		{
			return $"({FloatToString(v.x, 4)}, {FloatToString(v.y, 4)}, {FloatToString(v.z, 4)})";
		}
		public static string VectorToStringVerbose(Vector3 v)
		{
			return $"({v.x.ToString()}, {v.y.ToString()}, {v.z.ToString()})";
		}

		public static void DevException(string formattedString)
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogError(formattedString);
			}
			else
			{
				throw new System.InvalidOperationException(formattedString);
			}
		}

		public static void ArgumentNullCheck(object argValue, string argName)
		{
			if (argValue == null)
			{
				DevException(new ArgumentNullException(argName, "passed a null reference"));
			}
		}

		public static void DevException<T>(T exception) where T : Exception
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogError(exception.Message);
			}
			else
			{
				throw exception;
			}
		}

		[System.Obsolete("Use DevExceptionFormat")]
		public static void DevException(string formattedString, params object[] args)
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogErrorFormat(formattedString, args);
			}
			else
			{
				throw new System.InvalidOperationException(string.Format(formattedString, args));
			}
		}

		public static void DevExceptionFormat(string formattedString, params object[] args)
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogErrorFormat(formattedString, args);
			}
			else
			{
				throw new System.InvalidOperationException(string.Format(formattedString, args));
			}
		}

		[System.Obsolete("Use DevExceptionReturnObject")]
		public static T DevException<T>(T obj, string formattedString, params object[] args)
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogErrorFormat(formattedString, args);
				return obj;
			}
			else
			{
				throw new System.InvalidOperationException(string.Format(formattedString, args));
			}
		}

		public static T DevExceptionReturnObject<T>(T obj, string formattedString, params object[] args)
		{
			if (Core.Util.IsRelease())
			{
				Debug.LogErrorFormat(formattedString, args);
				return obj;
			}
			else
			{
				throw new System.InvalidOperationException(string.Format(formattedString, args));
			}
		}

		public static T NullCheck<T>(T item) where T : class
		{
			if (Core.Util.IsRelease())
			{
				return item;
			}
			else
			{
				return item ?? throw new System.ArgumentNullException($"CoreDebugUtil.NullCheck() Item of type {typeof(T).Name} cannot be null");
			}
		}

		public static string TruncatedFloatToString(float f, int decimalPlaces)
		{
			return Core.Util.TruncateDecimalPlaces(f, decimalPlaces).ToString("N" + decimalPlaces);
		}

		/// <summary>
		/// Meant to be the best float format for debugging when you don't have the space to dump the entire float to string, 
		/// most importantly DOES NOT ROUND like float format strings. Format string rounding can be very harmful when debugging 
		/// because what is printed is not the actual value. There is a big difference between 0.000001 and 0, 
		/// one can cause divide be zero exceptions and the other doesn't.
		/// 
		/// Instead truncate the float to the desired precisions and appends '...' when truncation has occurred so it's clear
		/// potentially important precision has been lost.
		/// </summary>
		public static string FloatToString(float f, int decimalPlaces)
		{
			if (decimalPlaces < 1)
			{
				return Mathf.FloorToInt(f).ToString();
			}
			float truncated = Mathf.Abs(f);
			int i = 0;
			float denum = 1.0f;
			bool wasTruncated = true;
			do
			{
				i++;
				denum *= 10.0f;
				truncated *= 10.0f;
				float test = truncated - Mathf.Floor(truncated);
				if (!(test > 0.0f)) // Doing ! on purpose, I don't trust <= here
				{
					wasTruncated = false;
					break;
				}
			}
			while (i < decimalPlaces && i < TRUNCATED_FLOAT_FORMATS.Length);
			truncated = Mathf.Floor(truncated);
			truncated /= denum;
			truncated *= Mathf.Sign(f);
			string s = wasTruncated ?
				truncated.ToString(TRUNCATED_FLOAT_FORMATS[i - 1]) + "...":
				truncated.ToString(FLOAT_FORMAT);
			return s;
		}
		private static readonly string FLOAT_FORMAT = "0.0########";
		private static readonly string[] TRUNCATED_FLOAT_FORMATS =
		{
			"0.0",
			"0.00",
			"0.000",
			"0.0000",
			"0.00000",
			"0.000000",
			"0.0000000",
			"0.00000000",
			"0.000000000",
		};

		/// <summary>
		/// Will only return one word, returns "Many..." if many bits in the mask a set.
		/// Note: This function makes a big assumption that values in the enum are incremental starting with 0 and with no gaps
		/// </summary>
		public static string MaskToStringShort<T>(int mask, string noneString = "None") where T : Enum
		{
			string[] names = System.Enum.GetNames(typeof(T));
			string maskString = null;
			for (int i = 0; i < names.Length; i++)
			{
				int test = 1 << i;
				if ((mask & test) != 0)
				{
					if (maskString == null)
					{
						maskString = names[i];
					}
					else
					{
						maskString = "Many...";
						break;
					}
				}
			}
			if (maskString == null)
			{
				maskString = noneString;
			}
			return maskString;
		}

		/// <summary>
		/// Returns a string with the name of every bit that is set
		/// Note: This function makes a big assumption that values in the enum are incremental starting with 0 and with no gaps
		/// </summary>
		public static string MaskToStringLong<T>(int mask, string noneString = "None") where T : Enum
		{
			string[] names = System.Enum.GetNames(typeof(T));
			string s = string.Empty;
			for (int i = 0; i < names.Length; i++)
			{
				int test = 1 << i;
				if ((mask & test) != 0)
				{
					s += names[i] + ", ";
				}
			}
			return string.IsNullOrEmpty(s) ? noneString : s;
		}
	}
}
