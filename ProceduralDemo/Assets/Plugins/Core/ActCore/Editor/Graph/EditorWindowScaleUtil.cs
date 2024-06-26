using UnityEngine;

namespace ActCore
{
	public static class EditorWindowScaleUtil
	{
		public const int EDITOR_WINDOW_TAB_HEIGHT = 21;

		private static Matrix4x4? s_CachedMatrix = null;

		public static void Begin(Rect position, float scale)
		{
			if (s_CachedMatrix.HasValue)
			{
				Debug.LogWarning("GUIScaleUtil.Begin() Need to call End() before calling Begin() again");
				return;
			}
			// The gods must be good to provide this: http://martinecker.com/martincodes/unity-editor-window-zooming/
			// Basically the trick is to end the 'Group' that the EditorWindow starts under the hood and begin a new one
			// that accounts for the tab height at the top of the editor window
			GUI.EndGroup();
			Rect clippedArea = ScaleSizeBy(position, 1.0f / scale, position.min);
			clippedArea.y += EDITOR_WINDOW_TAB_HEIGHT;
			GUI.BeginGroup(clippedArea);
			s_CachedMatrix = GUI.matrix;

			Matrix4x4 transMatrix = Matrix4x4.TRS(clippedArea.min, Quaternion.identity, Vector3.one);
			Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));
			GUI.matrix = transMatrix * scaleMatrix * transMatrix.inverse * GUI.matrix;
		}

		public static Rect ScaleSizeBy(Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.x -= pivotPoint.x;
			result.y -= pivotPoint.y;
			result.xMin *= scale;
			result.xMax *= scale;
			result.yMin *= scale;
			result.yMax *= scale;
			result.x += pivotPoint.x;
			result.y += pivotPoint.y;
			return result;
		}

		public static void End()
		{
			if (!s_CachedMatrix.HasValue)
			{
				return;
			}
			GUI.matrix = s_CachedMatrix.Value;
			s_CachedMatrix = null;
			GUI.EndGroup();
			GUI.BeginGroup(new Rect(0.0f, EDITOR_WINDOW_TAB_HEIGHT, Screen.width, Screen.height));
		}
	}
}
