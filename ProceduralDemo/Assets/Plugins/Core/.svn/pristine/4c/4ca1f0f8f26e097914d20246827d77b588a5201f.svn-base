
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ActAddContentDrawer
{
	private SortedDictionary<string, System.Type> m_AllTypes = new SortedDictionary<string, System.Type>();
	private List<System.Type> m_Types = new List<System.Type>();
	private string[] m_ClassNames = { };
	private int m_Context = 0;

	public void OnGUI(ActTree tree, ref SerializedObject sTree, Act.Node node)
	{
		Initialize(tree);
		DrawSequences(tree, ref sTree, node, tree.GetAllOpportunities());
	}

	private void Initialize(ActTree tree)
	{
		if (m_AllTypes.Count == 0)
		{
			foreach (System.Reflection.Assembly domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (System.Type type in domainAssembly.GetTypes())
				{
					if (type.IsSubclassOf(typeof(ActTrack)) &&
						!type.IsGenericTypeDefinition &&
						!type.IsAbstract &&
						!type.IsDefined(typeof(System.ObsoleteAttribute), false))
					{
						m_AllTypes.Add(type.Name, type);
					}
				}
			}
		}
		if (m_Types.Count == 0 || m_Context != tree.GetContextMask())
		{
			m_Context = tree.GetContextMask();
			SortedDictionary<string, System.Type> types = new SortedDictionary<string, System.Type>(new SortTrackNames());
			foreach (KeyValuePair<string, System.Type> pair in m_AllTypes)
			{
				System.Type type = pair.Value;
				System.Reflection.MethodInfo m = type.GetMethod(
					"GetContextMask",
					System.Reflection.BindingFlags.Static |
					System.Reflection.BindingFlags.FlattenHierarchy |
					System.Reflection.BindingFlags.Public);
				int trackMask = (int)m.Invoke(null, new object[] { });
				if ((trackMask & m_Context) == 0)
				{
					continue;
				}
				object[] atts = type.GetCustomAttributes(typeof(Act.TrackGroupAttribute), true);
				if (atts.Length > 0 && atts[0] is Act.TrackGroupAttribute trackGroup)
				{
					types.Add(trackGroup.GroupName + "/" + type.Name, type);
				}
				else
				{
					types.Add(type.Name, type);
				}
			}
			m_Types.Clear();
			m_Types.AddRange(types.Values);
			List<string> names = new List<string>(types.Keys);
			for (int i = 0; i < names.Count; i++)
			{
				if (!names[i].Contains("/"))
				{
					names.Insert(i, string.Empty); // Insert divider after grouped tracks
					m_Types.Insert(i, null);
					break;
				}
			}
			names.Insert(0, string.Empty); // Insert divider at begining of track list
			m_Types.Insert(0, null);
			names.Insert(0, "Add Track...");
			m_ClassNames = names.ToArray();
		}
	}

	private class SortTrackNames : IComparer<string>
	{
		int IComparer<string>.Compare(string x, string y)
		{
			bool xHasGroup = x.Contains("/");
			bool yHasGroup = y.Contains("/");
			if (xHasGroup != yHasGroup)
			{
				return xHasGroup ? -1 : 1;
			}
			return x.CompareTo(y);
		}
	}

	private void DrawSequences(ActTree tree, ref SerializedObject sTree, Act.Node node, List<Act.NodeSequence> sequences)
	{
		Rect rect = EditorGUILayout.GetControlRect();
		Rect r1 = rect;
		r1.width = ActTrackDrawer.GetActItemWidth(rect.width);
		Rect r2 = r1;
		r2.x += r1.width;
		int typeIndex = EditorGUI.Popup(r1, 0, m_ClassNames);
		if (typeIndex > 0)
		{
			ActTrack newTrack = (ActTrack)ScriptableObject.CreateInstance(m_Types[typeIndex - 1]);
			newTrack.EditorSetNodeID(node.GetID());
			Undo.RegisterCreatedObjectUndo(newTrack, "AddTrack");
			Undo.RecordObject(tree, "AddTrack");
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
			AssetDatabase.AddObjectToAsset(newTrack, tree);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newTrack)); // Update AssetDatabase
			ActTrackDrawer.UpdateTrackList(tree, ref sTree);
		}
		List<Act.Node> sibs = GetSibilings(tree, node);
		string[] sibNames = new string[2 * sibs.Count + 1];
		sibNames[0] = "Add Transition...";
		int index = 1;
		for (int i = 0; i < sibs.Count; i++)
		{
			sibNames[index] = sibs[i].GetName() + "/Anytime";
			index++;
			sibNames[index] = sibs[i].GetName() + "/After";
			index++;
		}
		int addIndex = EditorGUI.Popup(r2, 0, sibNames);
		if (addIndex != 0)
		{
			SerializedProperty serSequences = sTree.FindProperty("m_Opportunities");
			serSequences.arraySize++;
			int sibIndex = (addIndex - 1) / 2;
			SerializedProperty newSequence = serSequences.GetArrayElementAtIndex(serSequences.arraySize - 1);
			newSequence.FindPropertyRelative("m_FromID").intValue = node.GetID();
			newSequence.FindPropertyRelative("m_ToID").intValue = sibs[sibIndex].GetID();

			int mod = (addIndex - 1) % 2;
			float startTime = mod == 0 ? 0.0f : -Core.Util.SPF30;
			newSequence.FindPropertyRelative("m_StartTime").floatValue = startTime;
			newSequence.FindPropertyRelative("m_EndTime").floatValue = -Core.Util.SPF30;
		}
	}

	private int GetParentID(ActTree tree, Act.Node node)
	{
		foreach (Act.NodeLink link in tree.GetAllNodeLinks())
		{
			if (link.GetChildID() == node.GetID())
			{
				return link.GetParentID();
			}
		}
		return Act.Node.INVALID_ID;
	}

	private List<Act.Node> GetSibilings(ActTree tree, Act.Node node)
	{
		int parentID = GetParentID(tree, node);
		List<Act.Node> siblings = new List<Act.Node>();
		foreach (Act.NodeLink link in tree.GetAllNodeLinks())
		{
			if (link.GetParentID() == parentID)
			{
				Act.Node sib = tree.GetNode(link.GetChildID());
				if (sib != null)
				{
					siblings.Add(sib);
				}
			}
		}
		return siblings;
	}
}
