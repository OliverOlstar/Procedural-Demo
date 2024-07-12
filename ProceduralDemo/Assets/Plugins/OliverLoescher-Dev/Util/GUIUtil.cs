using UnityEngine;
using UGUI = UnityEngine.GUI;

namespace ODev.Util
{
	public static class GUI
	{
		public static int IntField(int value)
		{
			int.TryParse(GUILayout.TextField(value.ToString()), out int result);
			return result;
		}

		public static int IntField(string name, int value)
		{
			GUILayout.Label(name);
			int.TryParse(GUILayout.TextField(value.ToString()), out int result);
			return result;
		}

		public static int IntField(string name, int value, int maxLength)
		{
			GUILayout.Label(name);
			int.TryParse(GUILayout.TextField(value.ToString(), maxLength), out int result);
			return result;
		}

		public static float FloatField(string name, float value)
		{
			GUILayout.Label(name);
			float.TryParse(GUILayout.TextField(value.ToString()), out float result);
			return result;
		}

		public static int MinMaxIntField(int value, int min, int max, int increment, string label)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{label}: {value}");
			if (GUILayout.Button("<"))
			{
				value -= increment;
			}
			if (GUILayout.Button(">"))
			{
				value += increment;
			}
			GUILayout.EndHorizontal();
			return Mathf.Clamp(value, min, max);
		}

		public static float MinMaxFloatField(float value, float min, float max, float increment, string label)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{label}: {value}");
			if (GUILayout.Button("<"))
			{
				value -= increment;
			}
			if (GUILayout.Button(">"))
			{
				value += increment;
			}
			GUILayout.EndHorizontal();
			return Mathf.Clamp(value, min, max);
		}

		public static int IntSlider(int value, int min, int max, string label)
		{
			using (UsableHorizontal.Use())
			{
				GUILayout.Label(label);
				value = (int)GUILayout.HorizontalSlider(value, min, max);
			}
			return value;
		}

		public static int DrawTimeBox(string name, int value)
		{
			using (UsableHorizontal.Use())
			{
				value = IntField(name, value);
				using (UsableHorizontal.Use())
				{
					float buttonWidth = 32.0f;
					if (GUILayout.Button("+m", GUILayout.Width(buttonWidth)))
					{
						value += 60;
					}
					if (GUILayout.Button("+h", GUILayout.Width(buttonWidth)))
					{
						value += 60 * 60;
					}
					if (GUILayout.Button("+d", GUILayout.Width(buttonWidth)))
					{
						value += 60 * 60 * 24;
					}
				}
			}
			return value;
		}

		public static void GameObjectOnOff(string name)
		{
			bool on = true;
			if (OnOff(name, ref on))
			{
				GameObject obj = GameObject.Find(name);
				if (obj != null)
				{
					obj.SetActive(on);
				}
			}
		}

		public static void ShaderOnOff(string name)
		{
			bool on = true;
			if (OnOff(name, ref on))
			{
				GameObject obj = GameObject.Find(name);
				if (obj != null)
				{
					Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in renderers)
					{
						foreach (Material mat in renderer.materials)
						{
							mat.shader = Shader.Find("VertexLit");
						}
					}
				}
			}
		}

		public static bool OnOff(string name, ref bool on)
		{
			GUILayout.BeginHorizontal();
			bool changed = false;
			if (!on && GUILayout.Button($"{name} On"))
			{
				on = true;
				changed = true;
			}
			else if (on && GUILayout.Button($"{name} Off"))
			{
				on = false;
				changed = true;
			}
			GUILayout.EndHorizontal();
			return changed;
		}

		#region Usables
		// Example:
		// using (Usable.Use(...)) { }

		public class UsableHorizontal
		{
			public static GUILayout.HorizontalScope Use(params GUILayoutOption[] options)
				=> new(options);
			public static GUILayout.HorizontalScope Use(GUIStyle style, params GUILayoutOption[] options)
				=> new(style, options);
			public static GUILayout.HorizontalScope Use(string text, GUIStyle style, params GUILayoutOption[] options)
				=> new(text, style, options);
			public static GUILayout.HorizontalScope Use(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
				=> new(content, style, options);
			public static GUILayout.HorizontalScope Use(Color color, params GUILayoutOption[] options)
				=> Use(UGUI.skin.label, color, options);
			public static GUILayout.HorizontalScope Use(GUIStyle style, Color color, params GUILayoutOption[] options)
			{
				UGUI.color = color;
				GUILayout.HorizontalScope scope = new(style, options);
				UGUI.color = Color.white;
				return scope;
			}
			public static GUILayout.HorizontalScope Use(string text, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(new GUIContent(text), style, color, options);
			public static GUILayout.HorizontalScope Use(GUIContent content, GUIStyle style, Color color, params GUILayoutOption[] options)
			{
				UGUI.color = color;
				GUILayout.HorizontalScope scope = new(content, style, options);
				UGUI.color = Color.white;
				return scope;
			}
		}
		
		public class UsableVertical 
		{
			public static GUILayout.VerticalScope Use(params GUILayoutOption[] options)
				=> new(options);
			public static GUILayout.VerticalScope Use(GUIStyle style, params GUILayoutOption[] options)
				=> new(style, options);
			public static GUILayout.VerticalScope Use(string text, GUIStyle style, params GUILayoutOption[] options)
				=> new(text, style, options);
			public static GUILayout.VerticalScope Use(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
				=> new(content, style, options);
			public static GUILayout.VerticalScope Use(Color color, params GUILayoutOption[] options)
				=> Use(UGUI.skin.label, color, options);
			public static GUILayout.VerticalScope Use(GUIStyle style, Color color, params GUILayoutOption[] options)
			{
				UGUI.color = color;
				GUILayout.VerticalScope scope = new(style, options);
				UGUI.color = Color.white;
				return scope;
			}
			public static GUILayout.VerticalScope Use(string text, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(new GUIContent(text), style, color, options);
			public static GUILayout.VerticalScope Use(GUIContent content, GUIStyle style, Color color, params GUILayoutOption[] options)
			{
				UGUI.color = color;
				GUILayout.VerticalScope scope = new(content, style, options);
				UGUI.color = Color.white;
				return scope;
			}
		}

		public class UsableScrollRect
		{
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle style, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, UGUI.skin.scrollView, Color.white, options);

			// Color
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, UGUI.skin.scrollView, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, null, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, background, color, options);
			
			// Content
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle style, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, Color.white, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, UGUI.skin.scrollView, Color.white, options);

			// Color & Content
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, UGUI.skin.scrollView, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, false, false, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle style, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, UGUI.skin.horizontalScrollbar, UGUI.skin.verticalScrollbar, style, color, options);
			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, Color color, params GUILayoutOption[] options)
				=> Use(ref scrollPosition, content, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, UGUI.skin.scrollView, color, options);

			public static GUILayout.ScrollViewScope Use(ref Vector2 scrollPosition, GUIContent content, bool alwaysShowHorizontal, bool alwayShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, Color color, params GUILayoutOption[] options)
			{
				UGUI.color = color;
				GUILayout.ScrollViewScope scope = new(scrollPosition, alwaysShowHorizontal, alwayShowVertical, horizontalScrollbar, verticalScrollbar, background, options);
				scrollPosition = scope.scrollPosition;
				UGUI.color = Color.white;
				if (content != null)
				{
					GUILayout.Label(content);
				}
				return scope;
			}
		}
		
		public class UsableArea 
		{
			public static GUILayout.AreaScope Use(Rect screenRect)
				=> new(screenRect);
			public static GUILayout.AreaScope Use(Rect screenRect, GUIStyle style)
				=> new(screenRect, new GUIContent(), style);
			public static GUILayout.AreaScope Use(Rect screenRect, string text, GUIStyle style)
				=> new(screenRect, text, style);
			public static GUILayout.AreaScope Use(Rect screenRect, GUIContent content, GUIStyle style)
				=> new(screenRect, content, style);
			public static GUILayout.AreaScope Use(Rect screenRect, Color color)
				=> Use(screenRect, new GUIContent(), UGUI.skin.scrollView, color);
			public static GUILayout.AreaScope Use(Rect screenRect, GUIStyle style, Color color)
				=> Use(screenRect, new GUIContent(), style, color);
			public static GUILayout.AreaScope Use(Rect screenRect, string text, GUIStyle style, Color color)
				=> Use(screenRect, new GUIContent(text), style, color);
			public static GUILayout.AreaScope Use(Rect screenRect, GUIContent content, GUIStyle style, Color color)
			{
				UGUI.color = color;
				GUILayout.AreaScope scope = new(screenRect, content, style);
				UGUI.color = Color.white;
				return scope;
			}
		}
		#endregion
	}
}