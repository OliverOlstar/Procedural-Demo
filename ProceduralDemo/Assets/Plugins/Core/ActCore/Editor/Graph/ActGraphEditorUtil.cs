using UnityEditor;
using UnityEngine;

namespace ActCore
{
	public static class ActGraphEditorUtil
	{
		private static Texture s_Grid = null;
		public static Texture GridTexture
		{
			get
			{
				if (s_Grid == null)
				{
					s_Grid = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/Core/ActCore/Editor/Graph/ActBoardGrid.png");
				}
				return s_Grid;
			}
		}

		private static Texture s_GridOverlay = null;
		public static Texture GridOverlayTexture
		{
			get
			{
				if (s_GridOverlay == null)
				{
					s_GridOverlay = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/Core/ActCore/Editor/Graph/ActBoardGridOverlay.png");
				}
				return s_GridOverlay;
			}
		}

		public static void DrawGrid(Rect position, Vector2 offset, float scale)
		{
			Texture gridTex = GridTexture;
			float zoom = 1.0f / scale;
			float density = 2;
			offset *= density;
			Vector2 tiling = new Vector2(
				density * position.width / gridTex.width,
				density * position.height / gridTex.height);
			Rect texCoords = new Rect(
				-offset.x / gridTex.width,
				(offset.y / gridTex.height) - zoom * tiling.y,
				zoom * tiling.x,
				zoom * tiling.y);
			GUI.DrawTextureWithTexCoords(position, GridTexture, texCoords);
		}

		public static void DrawGradient(Rect position, float alpha = 0.7f)
		{
			Color col = GUI.color;
			GUI.color = new Color(1, 1, 1, alpha);
			GUI.DrawTexture(position, GridOverlayTexture, ScaleMode.StretchToFill, true);
			GUI.color = col;
		}
	}
}
