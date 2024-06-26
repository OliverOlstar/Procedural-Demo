
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;

//[InitializeOnLoad]


public static class FullscreenPlayMode //: MonoBehaviour
{
	//The size of the toolbar above the game view, excluding the OS border.
	static int tabHeight = 22;

	static EditorWindow GetCaptureWindow()
	{
		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		FieldInfo gameViews = T.GetField("s_GameViews", BindingFlags.NonPublic | BindingFlags.Static);
		IEnumerable enumerable = (IEnumerable)gameViews.GetValue(null);
		foreach (object obj in enumerable)
		{
			EditorWindow window = (EditorWindow)obj;
			if (window.name == "CaptureWindow")
			{
				return window;
			}
		}

		EditorWindow w = (EditorWindow)ScriptableObject.CreateInstance(T);
		w.name = "CaptureWindow";
		w.titleContent = new GUIContent("Capture");

		FieldInfo targetDisplay = T.GetField("m_TargetDisplay", BindingFlags.NonPublic | BindingFlags.Instance);
		targetDisplay.SetValue(w, 1);

		MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
		EditorWindow mainView = (EditorWindow)GetMainGameView.Invoke(null,null);
		w.minSize = mainView.minSize;
		w.maxSize = mainView.maxSize;
		Rect pos = mainView.position;
		pos.x += 20.0f;
		pos.y += 20.0f;
		w.position = pos;

		w.Show();
		return w;
	}

	static bool fullScreen = false;
	static Rect oldPos;
	static Vector2 oldMin;
	static Vector2 oldMax;

	[MenuItem("Window/Open Capture Window", false, 9999)]
	public static void OpenCaptureWindow()
	{
		GetCaptureWindow();
	}

	[MenuItem("Window/Toggle Fullscreen Capture Window %#r", false, 9999)]
	public static void FullScreenGameWindow()
	{
		if (fullScreen)
		{
			fullScreen = false;
			EditorWindow gameView = GetCaptureWindow();

			// Sanity check cached size
			oldMin.x = Mathf.Min(oldPos.width, 0.75f * Screen.currentResolution.width);
			oldMin.y = Mathf.Min(oldPos.width, 0.75f * Screen.currentResolution.height);
			oldPos.x = Mathf.Max(0.0f, oldPos.x);
			oldPos.y = Mathf.Max(0.0f, oldPos.y);
			oldPos.width = Mathf.Min(oldPos.width, 0.75f * Screen.currentResolution.width);
			oldPos.height = Mathf.Min(oldPos.height, 0.75f * Screen.currentResolution.height);

			gameView.minSize = oldMin;
			gameView.maxSize = oldMax;
			gameView.position = oldPos;
		}
		else
		{
			fullScreen = true;
			EditorWindow gameView = GetCaptureWindow();
			oldPos = gameView.position;
			oldMin = gameView.minSize;
			oldMax = gameView.maxSize;
			SetResolution();
		}
	}

	static void SetResolution()
	{
		EditorWindow gameView = GetCaptureWindow();

		float width = Screen.currentResolution.width;
		float height = Screen.currentResolution.height;
//		if (fourk)
//		{
//			width = 3840;
//			height = 2160;
//		}

		Rect newPos = new(0, 0 - tabHeight, width, height + tabHeight);

		gameView.position = newPos;
		gameView.minSize = new Vector2(width, height + tabHeight);
		gameView.maxSize = gameView.minSize;
		gameView.position = newPos;
	}

	[MenuItem("Window/Capture Camera", false, 9999)]
	static void CaptureCamera()
	{
		Camera prefab = AssetDatabase.LoadAssetAtPath<Camera>("Assets/Prefabs/Capture/CaptureCamera.prefab");
		Camera captureCamera = Object.Instantiate(prefab);
		Object.DontDestroyOnLoad(captureCamera);
		Camera mainCamera = Camera.main;
		captureCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
	}

//	[MenuItem("Window/Capture Resolution/Full Screen", false, 9999)]
//	static void SetResFull()
//	{
//		fourk = false;
//		SetResolution();
//	}
//
//	[MenuItem("Window/Capture Resolution/4K", false, 9999)]
//	static void SetRes4k()
//	{
//		fourk = true;
//		SetResolution();
//	}
}
