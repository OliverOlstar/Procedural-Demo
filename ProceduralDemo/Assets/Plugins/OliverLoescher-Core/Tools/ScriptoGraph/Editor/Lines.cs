using UnityEditor;
using UnityEngine;

namespace GraphEditor
{
	public class Lines
	{
		public static readonly Vector2 ArrowSize = new(8f, -4f);

		public static void DrawLine(Vector2 p1, Vector2 p2)
		{
			DrawLine(p1, p2, Color.black);
		}

		public static void DrawLine(Vector2 p1, Vector2 p2, Color color)
		{
			Handles.BeginGUI();
			Handles.color = color;
			Handles.DrawLine(p1, p2);
			Handles.EndGUI();
		}

		public static void DrawLineWithArrows(Vector2 from, Vector2 to, int count, Color color)
		{
			DrawLineWithArrows(from, to, count, color, ArrowSize);
		}

		public static void DrawLineWithArrows(Vector2 from, Vector2 to, int count, Color color, Vector2 arrowSize)
		{
			DrawLine(from, to, color);
			if (count <= 0)
			{
				return;
			}
			Vector2 difference = from - to;
			float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
			float increment = 1f / (count + 1);
			float current = increment;
			for (int i = 0; i < count; ++i)
			{
				Vector2 point = GetPointOnLine(from, to, current);
				GUIUtility.RotateAroundPivot(angle, point);
				Vector2 left = point - new Vector2(-arrowSize.x, arrowSize.y);
				Vector2 right = point + new Vector2(arrowSize.x, arrowSize.y);
				DrawLine(point, left, color);
				DrawLine(point, right, color);
				GUIUtility.RotateAroundPivot(-angle, point);
				current += increment;
			}
		}

		public static Vector2 GetPointOnLine(Vector2 p1, Vector2 p2, float normalizedLength)
		{
			return p1 + ((p2 - p1).magnitude * normalizedLength * (p2 - p1).normalized);
		}
	}
}
