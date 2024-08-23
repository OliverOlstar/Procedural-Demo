using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ModelLocator
{
	public class AssetPickerRenameWindow : EditorWindow
	{
		public class Rigs : List<(Animator Animator, Transform Transform, GUIContent Content)> { }

		public static bool TryOpen(SOModelLocator locator, Vector2 position)
		{
			Animator[] rigs = FindObjectsOfType<Animator>();
			if (rigs.Length < 2)
			{
				return false;
			}

			float width = 0.0f;
			float height = 0.0f;

			Rigs dict = new();
			foreach (Animator rig in rigs)
			{
				Transform joint = Core.Util.FindInTransformChildren(rig.transform, locator.ParentName);
				if (joint != null)
				{
					GUIContent content = EditorGUIUtility.ObjectContent(rig, rig.GetType());
					Vector2 size = GUI.skin.button.CalcSize(content);
					if (size.x > width)
					{
						width = size.x;
					}
					height += size.y + 2.0f;
					dict.Add((rig, joint, new GUIContent(content)));
				}
			}
			if (dict.Count < 2)
			{
				return false;
			}

			AssetPickerRenameWindow window = CreateInstance<AssetPickerRenameWindow>();
			window.titleContent = new GUIContent("Select Locator Parent");
			window.Initialize(locator, dict);
			window.ShowUtility();

			Rect r = window.position;
			r.position = position;
			r.width = width;
			r.height = height;
			window.position = r;
			return true;
		}

		private static Animator s_LastSelection = null;

		private Rigs m_Rigs = new();
		private SOModelLocator m_Locator = null;
		private int m_SelectedIndex = 0;

		private void Initialize(SOModelLocator locator, Rigs rigs)
		{
			m_Locator = locator;
			m_Rigs = rigs;
			m_SelectedIndex = 0;

			// Favor previous selection if we have one
			bool found = false;
			if (s_LastSelection != null)
			{
				for (int i = 0; i < m_Rigs.Count; i++)
				{
					(Animator Animator, Transform Transform, GUIContent Content) rig = m_Rigs[i];
					if (rig.Animator == s_LastSelection)
					{
						m_SelectedIndex = i;
						break;
					}
				}
			}
			if (!found)
			{
				// Try to auto select a clever model based on camera position
				Transform cameraTransform = SceneView.lastActiveSceneView.camera.transform;
				Ray ray = new(cameraTransform.position, cameraTransform.forward);
				float best = float.MaxValue;
				for (int i = 0; i < m_Rigs.Count; i++)
				{
					(Animator Animator, Transform Transform, GUIContent Content) rig = m_Rigs[i];
					float modelHeight = 2.0f; // Most models are ~2m tall
					Vector3 modelPosition = rig.Animator.transform.position + 0.5f * modelHeight * Vector3.up;
					Vector3 toModel = modelPosition - ray.origin;
					float dot = Vector3.Dot(toModel, ray.direction);
					if (dot < 0.0f)
					{
						continue;
					}
					Vector3 pointOnRay = ray.origin + dot * ray.direction;
					float sqrDist = Core.Util.SqrDistance(modelPosition, pointOnRay);
					if (sqrDist < best)
					{
						m_SelectedIndex = i;
						best = sqrDist;
					}
				}
			}
		}

		private void OnGUI()
		{
			Event e = Event.current;
			if (e.type == EventType.KeyDown)
			{
				switch (e.keyCode)
				{
					case KeyCode.Escape:
						Close();
						break;
					case KeyCode.UpArrow:
						m_SelectedIndex = m_SelectedIndex - 1 >= 0 ? m_SelectedIndex - 1 : m_Rigs.Count - 1;
						GetWindow<SceneView>().LookAt(m_Rigs[m_SelectedIndex].Transform.position);
						GetWindow<AssetPickerRenameWindow>();
						break;
					case KeyCode.DownArrow:
						m_SelectedIndex = m_SelectedIndex + 1 < m_Rigs.Count ? m_SelectedIndex + 1 : 0;
						GetWindow<SceneView>().LookAt(m_Rigs[m_SelectedIndex].Transform.position);
						GetWindow<AssetPickerRenameWindow>();
						break;
					case KeyCode.Return:
						s_LastSelection = m_Rigs[m_SelectedIndex].Animator;
						ModelLocatorEditor.Edit(m_Locator, m_Rigs[m_SelectedIndex].Transform.gameObject);
						Close();
						break;
				}
			}

			for (int i = 0; i < m_Rigs.Count; i++)
			{
				(Animator Animator, Transform Transform, GUIContent Content) rig = m_Rigs[i];
				if (i != m_SelectedIndex)
				{
					//GUI.color = Color.Lerp(new Color32(127, 214, 252, 255), Color.white, 0.5f);
					GUI.color = 0.75f * Color.white;
				}
				if (GUILayout.Button(rig.Content))
				{
					s_LastSelection = rig.Animator;
					ModelLocatorEditor.Edit(m_Locator, rig.Transform.gameObject);
					Close();
				}
				GUI.color = Color.white;
			}
		}
	}
}
