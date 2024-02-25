using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	[CustomEditor(typeof(SOActConditionFabBase), true)]
	public class ActConditionFabEditor : Editor
	{
		private ConditionDrawer m_ConditionDrawer = new ConditionDrawer();

		public override void OnInspectorGUI()
		{
			ActTree2 tree = target as ActTree2;
			SerializedObject sTree = serializedObject;

			SerializedProperty sNotes = sTree.FindProperty("m_Notes");
			EditorGUILayout.PropertyField(sNotes);

			SerializedProperty sRootNode = sTree.FindProperty("m_RootNode");
			SerializedProperty sEventType = sRootNode.FindPropertyRelative("m_EventType");
			EditorGUILayout.PropertyField(sEventType);

			m_ConditionDrawer.OnGUI(tree, ref sTree, tree.RootNode, ref sRootNode);
			if (sTree.hasModifiedProperties)
			{
				sTree.ApplyModifiedProperties();
			}
		}
	}
}
