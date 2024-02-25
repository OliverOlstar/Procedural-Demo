
using UnityEngine;
using UnityEditor;
using System.IO;

public class CaptureTools : EditorWindow
{
    FreeCamera m_Camera = null;

    [MenuItem("Window/Capture Tools", false, 9999)]
	static void CreateWizard()
	{
        BundleTools window = EditorWindow.GetWindow<BundleTools>("Capture Tools");
		window.Show();
	}

	void OnGUI()
	{
        if (GUILayout.Button("Spawn Camera"))
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Capture/CaptureCamera.prefab");
            GameObject captureCamera = Object.Instantiate<GameObject>(prefab);
            Object.DontDestroyOnLoad(captureCamera);
            m_Camera = captureCamera.AddComponent<FreeCamera>();
        }

        if (GUILayout.Button("Open Capture Window"))
        {
            FullscreenPlayMode.OpenCaptureWindow();
        }

        if (GUILayout.Button("Toggle Fullscreen Capture Window (ctrl + shift + r)"))
        {
            FullscreenPlayMode.FullScreenGameWindow();
        }

        bool animate = GUILayout.Toggle(m_Camera.IsAnimating(), "Animate");
        m_Camera.SetAnimate(animate);

        GUILayout.Label("Rotate Speed " + m_Camera.m_RotateSpeed.ToString("0"), GUILayout.ExpandWidth(false));
        m_Camera.m_RotateSpeed = GUILayout.HorizontalSlider(m_Camera.m_RotateSpeed, -90, 90);
        GUILayout.Label("Zoom Speed " + m_Camera.m_ZoomSpeed.ToString("0.0"), GUILayout.ExpandWidth(false));
        m_Camera.m_ZoomSpeed = GUILayout.HorizontalSlider(m_Camera.m_ZoomSpeed, -10, 10);
	}
}

