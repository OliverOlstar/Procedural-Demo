
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Act2
{
	public class AddTrackDrawer
	{
		private ActNodeCreateItemDrawer<Track> m_AddTrackDrawer = new ActNodeCreateItemDrawer<Track>();

		public void OnGUI(IActObject tree, IActNode node, ref SerializedProperty sNode, out Rect position)
		{
			m_AddTrackDrawer.Initialize(tree, node);

			EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
			Rect rect = EditorGUILayout.GetControlRect();
			position = rect;
			position.width = TrackDrawer.GetActItemWidth(rect.width);

			if (m_AddTrackDrawer.GUI(position, out object instance))
			{
				SerializedProperty sTracks = sNode.FindPropertyRelative("m_Tracks");
				sTracks.arraySize++;
				SerializedProperty ele = sTracks.GetArrayElementAtIndex(sTracks.arraySize - 1);
				ele.managedReferenceValue = instance;
			}
		}
	}

	public class AddTracksAndTransitionsDrawer
	{
		private AddTrackDrawer m_AddTrackDrawer = new AddTrackDrawer();

		public void OnGUI(ActTree2 tree, Node node, ref SerializedProperty sNode)
		{
			m_AddTrackDrawer.OnGUI(tree, node, ref sNode, out Rect position);
			Rect r2 = position;
			r2.x += position.width;
			List<Node> sibs = tree.GetSiblings(node);
			if (sibs.Count == 0)
			{
				return;
			}
			string[] sibNames = new string[2 * sibs.Count + 1];
			sibNames[0] = "Add Transition...";
			int index = 1;
			for (int i = 0; i < sibs.Count; i++)
			{
				sibNames[index] = sibs[i].Name + "/Anytime";
				index++;
				sibNames[index] = sibs[i].Name + "/After";
				index++;
			}
			int addIndex = EditorGUI.Popup(r2, 0, sibNames);
			if (addIndex != 0)
			{
				SerializedProperty sTransitions = sNode.FindPropertyRelative("m_Transitions");
				sTransitions.arraySize++;
				int sibIndex = (addIndex - 1) / 2;
				SerializedProperty newTransition = sTransitions.GetArrayElementAtIndex(sTransitions.arraySize - 1);
				newTransition.FindPropertyRelative("m_ToID").intValue = sibs[sibIndex].GetID();

				int mod = (addIndex - 1) % 2;
				float startTime = mod == 0 ? 0.0f : -Core.Util.SPF30;
				newTransition.FindPropertyRelative("m_StartTime").floatValue = startTime;
				newTransition.FindPropertyRelative("m_EndTime").floatValue = -Core.Util.SPF30;
			}
		}
	}
}
