using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FreeCamera : MonoBehaviour
{
	public float m_RotateSpeed = 45.0f;
	public float m_ZoomSpeed = 1.0f;

#if UNITY_EDITOR
	Quaternion m_Rotation = Quaternion.identity;
	bool m_Animate = false;
	public bool IsAnimating() { return m_Animate; }
	float m_Distance = 0.0f;
	Transform m_Following = null;

	Rect GetArea()
	{
		return new Rect(
			0.0f, 
			0.0f, 
			0.2f * Screen.width, 
			Screen.height);
	}

	void Update()
	{
		if (!Application.isEditor) return;

		if (SceneView.lastActiveSceneView != null)
		{
			Vector3 pivot = SceneView.lastActiveSceneView.pivot;
			Camera sceneCamera = SceneView.GetAllSceneCameras()[0];
			Vector3 offset = sceneCamera.transform.position - pivot;
			float d = Core.Util.Normalize(ref offset);

			if (Input.GetKeyDown(KeyCode.F))
			{
				m_Following = Selection.activeTransform;
			}
			if (m_Following != null)
			{
				if (Selection.activeTransform == null || m_Following.GetInstanceID() != Selection.activeTransform.GetInstanceID())
				{
					m_Following = null;
				}
				else
				{
					pivot = m_Following.position;
				}
			}

			if (m_Animate)
			{
				m_Rotation *= Quaternion.AngleAxis(m_RotateSpeed * Time.deltaTime, Vector3.up);
				offset = m_Rotation * offset;
				m_Distance -= Time.deltaTime * m_ZoomSpeed;
				d += m_Distance;
			}
			transform.rotation = Core.Util.SafeLookRotation(-offset, Vector3.up);
			transform.position = pivot + d * offset;
			GetComponent<Camera>().fieldOfView = sceneCamera.fieldOfView;
		}
	}

	public void SetAnimate(bool animate)
	{
		m_Animate = animate;
		if (!m_Animate)
		{
			m_Rotation = Quaternion.identity;
			m_Distance = 0.0f;
		}
	}

	void OnEnable()
	{
#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui += OnScene;
#else
		SceneView.onSceneGUIDelegate += OnScene;
#endif

	}

	void OnDisable()
	{
#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui -= OnScene;
#else
		SceneView.onSceneGUIDelegate -= OnScene;
#endif
	}

	void OnScene(SceneView sceneview)
	{
		GUILayout.BeginArea(GetArea(), GUI.skin.box);
		GUILayout.Label("Capture Camera");

		m_Animate = GUILayout.Toggle(m_Animate, "Animate");
		if (!m_Animate)
		{
			m_Rotation = Quaternion.identity;
			m_Distance = 0.0f;
		}

		GUILayout.Label("Rotate Speed " + m_RotateSpeed.ToString("0"), GUILayout.ExpandWidth(false));
		m_RotateSpeed = GUILayout.HorizontalSlider(m_RotateSpeed, -90, 90);
		GUILayout.Label("Zoom Speed " + m_ZoomSpeed.ToString("0.0"), GUILayout.ExpandWidth(false));
		m_ZoomSpeed = GUILayout.HorizontalSlider(m_ZoomSpeed, -10, 10);

		GUILayout.EndArea();
	}
#endif
	}