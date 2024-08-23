using UnityEngine;
using UnityEditor;
using System.Linq;

public class CameraAnimationEditorWindow : EditorWindow
{
	[MenuItem("Window/Camera Animation Editor", false, 1500)]
	public static CameraAnimationEditorWindow Get()
	{
		CameraAnimationEditorWindow window = GetWindow<CameraAnimationEditorWindow>("Camera Animation Editor");
		window.Show();
		return window;
	}
	const string AnimationSavePath = "/Data/CameraAnimations/";


	SerializedObject m_SSequence = null;
	CameraAnimationSequence m_Sequence;
	CameraAnimationSequence.CameraAnimationKeyframe m_Keyframe;
	Camera m_KeyframeCamera = null;
	GameObject m_LocatorParent = null;
	GameObject m_MainParent = null;

	string m_SequencePath = Core.Str.EMPTY;
	int m_SequenceIndex = -1;
	int m_KeyframeIndex = 0;
	bool m_ChangedSequence = false;
	bool m_ShowDebugScrubber = false;
	float m_DebugScrubberVal = 0;
	SceneView m_SceneView;
	void OnEnable()
	{
		Undo.undoRedoPerformed += OnUndo;
	}

	private void OnUndo()
	{
		if(m_SSequence == null || m_Sequence == null)
		{
			return;
		}
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(m_Sequence));
		Repaint();
	}

	void OnDisable()
	{
		Undo.undoRedoPerformed -= OnUndo;
	}

	private void OnGUI()
	{
		if(!EditorApplication.isPlaying)
		{
			EditorGUILayout.LabelField("Please enter playmode to edit animations");
			m_SequenceIndex = -1;
			m_Sequence = null;
			m_SSequence = null;
			m_KeyframeIndex = -1;
			m_SequencePath = Core.Str.EMPTY;
			m_MainParent = null;
			return;
		}

		if(GUILayout.Button("Select Character for animation"))
		{
			m_MainParent = Selection.activeGameObject;
		}

		if (m_MainParent == null) 
		{
			return;
		}

		m_SceneView = (SceneView)SceneView.sceneViews.ToArray()[0];
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Selected Character:");
		EditorGUILayout.SelectableLabel(m_MainParent.name);
		EditorGUILayout.EndHorizontal();

		#region -- select Sequence --
		string[] sequenceGuids = AssetDatabase.FindAssets("t:CameraAnimationSequence");
		if(sequenceGuids.Length <= 0)
		{
			return;
		}
		string[] sequencePaths = new string[sequenceGuids.Length];
		string[] sequenceNames = new string[sequenceGuids.Length];
		for (int i = 0; i < sequenceGuids.Length; i++)
		{
			string path = AssetDatabase.GUIDToAssetPath(sequenceGuids[i]);
			sequencePaths[i] = path;
			string[] pathBuilder = path.Substring(7, path.Length - 13).Split('/');
			if(pathBuilder.Length > 1)
			{
				sequenceNames[i] = pathBuilder[^2] + "/";
			}
			sequenceNames[i] += pathBuilder[^1];
		}
		m_SequenceIndex = EditorGUILayout.Popup(m_SequenceIndex, sequenceNames);
		if(m_SequenceIndex >= 0)
		{
			m_SequencePath = sequencePaths[m_SequenceIndex];
		}
		else
		{
			m_SequencePath = Core.Str.EMPTY;
		}
		if (!Core.Str.IsEmpty(m_SequencePath))
		{
			UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(m_SequencePath);
			if(obj is CameraAnimationSequence)
			{
				if(obj != m_Sequence)
				{
					m_Sequence = (CameraAnimationSequence)obj;
					m_SSequence = new SerializedObject(obj);

					m_KeyframeIndex = 0;
					m_ChangedSequence = true;
					if(Core.Str.IsEmpty(m_Sequence.GetBoneName()))
					{
						Debug.LogError("[CameraAnimationEditorWindow] the sequence bone name is empty");
						return;
					}

					if (m_LocatorParent == null || m_LocatorParent.name != m_Sequence.GetBoneName())
					{
						Transform trans = Core.Util.FindInTransformChildren(m_MainParent.transform, m_Sequence.GetBoneName());
						if (trans != null)
						{
							m_LocatorParent = trans.gameObject;
						}
						else
						{
							Debug.LogError("[CameraAnimationEditorWindow] something is wrong with your selected object, it might not contain the bone called " + m_Sequence.GetBoneName());
							return;
						}
					}
				}
				else
				{
					m_ChangedSequence = false;
				}
			}
		}
		#endregion
		#region -- select Keyframe --
		if (m_Sequence== null || m_SSequence == null)
		{
			return;
		}

		EditorGUILayout.Separator();
		#region -- keyframe displayBar --
		
		string[] keyFrameNames = new string[m_Sequence.m_Keyframes.Count];

		for(int i = 0; i < keyFrameNames.Length; ++i)
		{
			keyFrameNames[i] = Core.Util.SecondsToFrames(m_Sequence.m_Keyframes[i].GetFrameValue()).ToString();
		}
		int newKeyframeIndex = GUILayout.SelectionGrid(m_KeyframeIndex, keyFrameNames, 25);

		#endregion;
		if(newKeyframeIndex != m_KeyframeIndex || m_ChangedSequence) // change frame
		{
			m_KeyframeIndex = newKeyframeIndex;
			m_Keyframe = m_Sequence.m_Keyframes[m_KeyframeIndex];
			ResetCamera();
		}
		#endregion
		EditorGUILayout.Separator();
		#region -- render Keyframe Gui --
		int newFrameVal = EditorGUILayout.DelayedIntField("Frame", Core.Util.SecondsToFrames(m_Keyframe.GetFrameValue()));
		if(newFrameVal != Core.Util.SecondsToFrames(m_Keyframe.GetFrameValue()))
		{
			AdjustFrameOrder(newFrameVal);
		}

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Add Keyframe"))
		{
			AddKeyframe();
		}
		if(GUILayout.Button("Delete Keyframe"))
		{
			DeleteKeyframe();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Move Camera To Keyframe"))
		{
			ResetCamera();
		}
		if (GUILayout.Button("Move Camera To Editor View"))
		{
			if(m_SceneView != null)
			{
				m_KeyframeCamera.transform.SetPositionAndRotation(m_SceneView.camera.transform.position, m_SceneView.camera.transform.rotation);
			}
		}
		if (GUILayout.Button("Move Editor View To Camera"))
		{
			if (m_SceneView != null)
			{
				m_SceneView.AlignViewToObject(m_KeyframeCamera.transform);
			}
		}
		EditorGUILayout.EndHorizontal();
		if (GUILayout.Button("Apply Keyframe"))
		{
			SaveCameraToKeyframe();
		}
		#endregion

		if (GUI.changed)
		{
			m_SSequence.ApplyModifiedProperties();
		}
		m_ShowDebugScrubber = EditorGUILayout.Toggle("Debug Scrubber", m_ShowDebugScrubber);
		if (m_ShowDebugScrubber)
		{
			m_DebugScrubberVal = EditorGUILayout.Slider(m_DebugScrubberVal, 0.0f, 1.0f);

			CameraAnimationSequence.CameraAnimationKeyframe frame = m_Sequence.Interpolate(m_DebugScrubberVal);
			m_KeyframeCamera.transform.localPosition = frame.GetPosition();
			m_KeyframeCamera.transform.localRotation = frame.GetRotation();
			m_KeyframeCamera.fieldOfView = frame.GetFOV();
		}
	}

	void ResetCamera()
	{
		if(m_KeyframeCamera == null)
		{
			m_KeyframeCamera = new GameObject("CAMERA_ANIMATION_EDITOR_KEYFRAME_CAMERA", typeof(Camera)).GetComponent<Camera>();
		}
		m_KeyframeCamera.transform.SetParent(m_LocatorParent.transform);
		m_KeyframeCamera.transform.localPosition = m_Keyframe.GetPosition();
		m_KeyframeCamera.transform.localRotation = m_Keyframe.GetRotation();
		m_KeyframeCamera.fieldOfView = m_Keyframe.GetFOV();
		Selection.activeGameObject = m_KeyframeCamera.gameObject;
	}
	void DestroyCamera()
	{
		if(m_KeyframeCamera != null)
		{
			Destroy(m_KeyframeCamera.gameObject);
			m_KeyframeCamera = null;
		}
	}

	void SaveCameraToKeyframe()
	{
		if (m_KeyframeCamera == null || m_LocatorParent == null)
		{
			return;
		}
		SerializedProperty serFrames = m_SSequence.FindProperty("m_Keyframes");
		SerializedProperty newFrame = serFrames.GetArrayElementAtIndex(m_KeyframeIndex);
		newFrame.FindPropertyRelative("m_FOV").intValue = (int)m_KeyframeCamera.fieldOfView;
		newFrame.FindPropertyRelative("m_Position").vector3Value = m_KeyframeCamera.transform.localPosition;
		newFrame.FindPropertyRelative("m_Rotation").vector3Value = m_KeyframeCamera.transform.localRotation.eulerAngles;
		m_SSequence.ApplyModifiedProperties();
		Selection.activeGameObject = m_KeyframeCamera.gameObject;
	}

	void AddKeyframe()
	{
		CameraAnimationSequence.CameraAnimationKeyframe lastFrame = m_Keyframe;
		int newFrameIndex = m_KeyframeIndex+1;
		if(m_KeyframeIndex < m_Sequence.m_Keyframes.Count -1 && (m_Sequence.m_Keyframes[newFrameIndex].GetFrameValue() - m_Keyframe.GetFrameValue()) <= 1 )
		{
			return;
		}
		SerializedProperty serFrames = m_SSequence.FindProperty("m_Keyframes");
		float newFrameVal = lastFrame.GetFrameValue() +1;
		serFrames.InsertArrayElementAtIndex(newFrameIndex);
		SerializedProperty newFrame = serFrames.GetArrayElementAtIndex(newFrameIndex);
		newFrame.FindPropertyRelative("m_FrameValue").floatValue = newFrameVal;
		newFrame.FindPropertyRelative("m_FOV").intValue = lastFrame.GetFOV();
		newFrame.FindPropertyRelative("m_Position").vector3Value = lastFrame.GetPosition();
		newFrame.FindPropertyRelative("m_Rotation").vector3Value = lastFrame.GetVectorRotation();

		m_SSequence.ApplyModifiedProperties();
		m_KeyframeIndex = newFrameIndex;
		m_ChangedSequence = true;
		m_Keyframe = m_Sequence.m_Keyframes[newFrameIndex];
		Repaint();
	}

	void DeleteKeyframe()
	{
		SerializedProperty serFrames = m_SSequence.FindProperty("m_Keyframes");
		serFrames.DeleteArrayElementAtIndex(m_KeyframeIndex);
		m_SSequence.ApplyModifiedProperties();

		if(m_KeyframeIndex > serFrames.arraySize -1)
		{
			m_KeyframeIndex = serFrames.arraySize -1;
		}
	}

	void SaveKeyframe()
	{
		SerializedProperty serFrames = m_SSequence.FindProperty("m_Keyframes");
		SerializedProperty newFrame = serFrames.GetArrayElementAtIndex(m_KeyframeIndex);
		newFrame.FindPropertyRelative("m_FrameValue").floatValue = m_Keyframe.GetFrameValue() +1;
		newFrame.FindPropertyRelative("m_FOV").intValue = m_Keyframe.GetFOV();
		newFrame.FindPropertyRelative("m_Position").vector3Value = m_Keyframe.GetPosition();
		newFrame.FindPropertyRelative("m_Rotation").vector3Value = m_Keyframe.GetVectorRotation();
		m_SSequence.ApplyModifiedProperties();
	}

	void AdjustFrameOrder(int newFrameVal)
	{
		if(m_Sequence.m_Keyframes.Any(x=>x.GetFrameValue() == newFrameVal))
		{
			return;
		}
		newFrameVal = Mathf.Max(0, newFrameVal); // dont be negative
		SerializedProperty serFrames = m_SSequence.FindProperty("m_Keyframes");
		SerializedProperty frame = serFrames.GetArrayElementAtIndex(m_KeyframeIndex);
		frame.FindPropertyRelative("m_FrameValue").floatValue = Core.Util.FramesToSeconds(newFrameVal);
		int LargestIndex = -1;
		for (int i = 0; i < serFrames.arraySize; ++i)
		{
			float curFrameVal = Core.Util.SecondsToFrames(serFrames.GetArrayElementAtIndex(i).FindPropertyRelative("m_FrameValue").floatValue);
			if (curFrameVal >= newFrameVal)
			{
				LargestIndex = i;
				break;
			}
		}
		int newIndex = Mathf.Clamp(LargestIndex,0,serFrames.arraySize-1);
		serFrames.MoveArrayElement(m_KeyframeIndex, newIndex);
		m_KeyframeIndex = newIndex;
		m_SSequence.ApplyModifiedProperties();
		m_ChangedSequence = true;
		m_Keyframe = m_Sequence.m_Keyframes[newIndex];
		Repaint();
	}
}
