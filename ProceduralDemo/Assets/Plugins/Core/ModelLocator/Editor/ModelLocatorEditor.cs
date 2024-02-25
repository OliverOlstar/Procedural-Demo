
using UnityEngine;
using UnityEditor;

namespace ModelLocator
{
	[CustomEditor(typeof(SOModelLocator), true)]
	public class ModelLocatorEditor : Editor
	{
		public static void TrySave(DummyModelLocator editor)
		{
			if (ModelLocatorTools.Editor.Source != null &&
				ModelLocatorTools.Editor.Source == editor.Source)
			{
				Save(ModelLocatorTools.Editor.Source);
			}
		}

		public static void Save(SOModelLocator locator)
		{
			SerializedObject sObj = new SerializedObject(locator);
			Transform dummyTransform = ModelLocatorTools.Editor.transform;
			sObj.FindProperty("m_ParentName").stringValue = dummyTransform.parent == null ? string.Empty : dummyTransform.parent.name;
			sObj.FindProperty("m_Position").vector3Value = dummyTransform.localPosition;
			sObj.FindProperty("m_Rotation").vector3Value = dummyTransform.localEulerAngles;
			sObj.FindProperty("m_Quaternion").quaternionValue = dummyTransform.localRotation;
			sObj.ApplyModifiedProperties();
			DestroyImmediate(ModelLocatorTools.Editor.gameObject);
			ModelLocatorTools.Editor = null;
		}

		public static void Edit(SOModelLocator locator, GameObject parentObject)
		{
			DummyModelLocator editor = new GameObject(locator.name).AddComponent<DummyModelLocator>();
			editor.Source = locator;

			if (parentObject != null)
			{
				editor.transform.SetParent(parentObject.transform);
			}

			editor.transform.localPosition = locator.Position;
			editor.transform.localRotation = locator.Rotation;

			Selection.activeGameObject = editor.gameObject;

			EditorWindow.GetWindow<SceneView>().LookAt(editor.transform.position);

			if (ModelLocatorTools.Editor != null)
			{
				DestroyImmediate(ModelLocatorTools.Editor.gameObject);
			}
			ModelLocatorTools.Editor = editor;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			SOModelLocator locator = (SOModelLocator)target;
			// If there is no editor object or we are not the owner
			if (ModelLocatorTools.Editor == null || ModelLocatorTools.Editor.Source != locator)
			{
				if (GUILayout.Button("Edit", GUILayout.Height(2.0f * EditorGUIUtility.singleLineHeight)))
				{
					Event e = Event.current;
					Vector2 p = GUIUtility.GUIToScreenPoint(e.mousePosition);
					if (!AssetPickerRenameWindow.TryOpen(locator, p))
					{
						GameObject parentObject = GameObject.Find(locator.ParentName);
						Edit(locator, parentObject);
					}
				}
			}
			else if (GUILayout.Button("Save", GUILayout.Height(2.0f * EditorGUIUtility.singleLineHeight)))
			{
				Save(locator);
			}
		}

		//enum HandleType
		//{
		//	None = 0,
		//	Move,
		//	Rotate
		//}
		//HandleType mHandleType = HandleType.None;

		//public void OnEnable()
		//{
		//	SceneView.onSceneGUIDelegate += SceneUpdate;
		//}

		//public void OnDisable()
		//{
		//	SceneView.onSceneGUIDelegate -= SceneUpdate;
		//}

		//public static void DrawLocatorHandle(Core.SimpleTransform xform, Color color)
		//{
		//	Handles.color = color;
		//	Handles.DrawWireDisc(xform.GetPosition(), xform.GetRotation() * Vector3.up, 0.1f);
		//	Handles.DrawWireDisc(xform.GetPosition(), xform.GetRotation() * Vector3.forward, 0.1f);
		//	Handles.DrawWireDisc(xform.GetPosition(), xform.GetRotation() * Vector3.right, 0.1f);
		//	Handles.color = Color.red;
		//	Handles.DrawLine(xform.GetPosition(), xform.GetRotation() * Vector3.right * 0.5f + xform.GetPosition());
		//	Handles.color = Color.green;
		//	Handles.DrawLine(xform.GetPosition(), xform.GetRotation() * Vector3.up * 0.5f + xform.GetPosition());
		//	Handles.color = Color.blue;
		//	Handles.DrawLine(xform.GetPosition(), xform.GetRotation() * Vector3.forward * 0.5f + xform.GetPosition());
		//}

		//void SceneUpdate(SceneView sceneView)
		//{
		//	ObjLocator locator = (ObjLocator)target;
		//	Core.SimpleTransform parentXform = locator.GetParentSimpleTransform(gameObject);
		//	Core.SimpleTransform xform = locator.GetTransRelObj(gameObject);

		//	if (mHandleType == HandleType.Rotate)
		//	{
		//		Quaternion newRot = Handles.RotationHandle(xform.GetRotation(), xform.GetPosition());
		//		if (Quaternion.Angle(newRot, xform.GetRotation()) > Core.Util.EPSILON)
		//		{
		//			xform.SetRotation(newRot);
		//			locator.SetTransformRelativeObject(gameObject, xform);
		//		}
		//	}
		//	else if (mHandleType == HandleType.Move)
		//	{
		//		Vector3 newPostion = Handles.PositionHandle(xform.GetPosition(), xform.GetRotation());
		//		if ((newPostion - xform.GetPosition()).sqrMagnitude > Core.Util.EPSILON)
		//		{
		//			xform.SetPosition(newPostion);
		//			locator.SetTransformRelativeObject(gameObject, xform);
		//		}
		//	}

		//	DrawLocatorHandle(xform, Color.white);
		//	DrawLocatorHandle(parentXform, Color.grey);
		//	Handles.color = Color.grey;
		//	Handles.DrawLine(xform.GetPosition(), parentXform.GetPosition());

		//	if (Event.current.Equals(Event.KeyboardEvent("f")))
		//	{
		//		sceneView.LookAt(xform.GetPosition());
		//	}
		//	if (Event.current.Equals(Event.KeyboardEvent("e")))
		//	{
		//		mHandleType = HandleType.Rotate;
		//	}
		//	if (Event.current.Equals(Event.KeyboardEvent("w")))
		//	{
		//		mHandleType = HandleType.Move;
		//	}
		//	if (Event.current.Equals(Event.KeyboardEvent("q")))
		//	{
		//		mHandleType = HandleType.None;
		//	}

		//	if (GUI.changed)
		//	{
		//		EditorUtility.SetDirty(target);
		//	}
		//}
	}
}
