using UnityEngine;
using UnityEditor;

namespace Core
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Transform), true)]
	public class TransformEditor : Editor
	{
		static Vector3 mPosition = Vector3.zero;
		static Vector3 mRotation = Vector3.zero;
		//Make a function that replaces the Transform Component in the Inpector
		public override void OnInspectorGUI()
		{
			//target is the object you have selected in the editor. We are making a variable that is equal to the target's transform
			Transform t = (Transform)target;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Copy"))
			{
				mPosition = t.transform.localPosition;
				mRotation = t.transform.localEulerAngles;
			}
			if (GUILayout.Button("Paste"))
			{
				Undo.RecordObject(t.transform, "Paste Transform");
				t.transform.localPosition = mPosition;
				t.transform.localEulerAngles = mRotation;
			}
			GUILayout.EndHorizontal();

			EditorGUI.indentLevel = 0;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("P", GUILayout.ExpandWidth(false)))
			{
				Undo.RecordObject(t, "Reset Position " + t.name);
				t.transform.localPosition = Vector3.zero;
			}
			Vector3 positionDifference = EditorGUILayout.Vector3Field("Position", t.localPosition, GUILayout.ExpandWidth(true)) - t.localPosition;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("R", GUILayout.ExpandWidth(false)))
			{
				Undo.RecordObject(t, "Reset Rotation " + t.name);
				t.transform.localRotation = Quaternion.identity;
			}
			Vector3 rotationDifference = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles, GUILayout.ExpandWidth(true)) - t.localEulerAngles;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("S", GUILayout.ExpandWidth(false)))
			{
				Undo.RecordObject(t, "Reset Scale " + t.name);
				t.transform.localScale = new Vector3(1, 1, 1);
			}
			Vector3 scaleDifference = EditorGUILayout.Vector3Field("Scale", t.localScale, GUILayout.ExpandWidth(true)) - t.localScale;
			GUILayout.EndHorizontal();
			
			if (GUI.changed)
			{
				foreach (Object o in targets)
				{
					t = ((Transform)o);
					Undo.RecordObject(t, "Transform Change");
					t.localPosition += FixIfNaN(positionDifference);
					t.localEulerAngles += FixIfNaN(rotationDifference);
					t.localScale += FixIfNaN(scaleDifference);
				}
			}
		}

		private Vector3 FixIfNaN(Vector3 v)
		{
			if (float.IsNaN(v.x))
			{
				v.x = 0;
			}
			if (float.IsNaN(v.y))
			{
				v.y = 0;
			}
			if (float.IsNaN(v.z))
			{
				v.z = 0;
			}
			return v;
		}
	}
}