using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class ActNodeCreateItemDrawer<TNodeItem>
	{
		private SortedDictionary<string, System.Type> m_AllTypes = new SortedDictionary<string, System.Type>();

		private List<System.Type> m_Types = new List<System.Type>();

		private string[] m_ClassNames = { };

		private System.Type m_TreeType = null;
		private System.Type m_EventType = null;

		public void Initialize(IActObject tree, IActNode node)
		{
			if (m_AllTypes.Count == 0)
			{
				foreach (System.Type type in Core.TypeUtility.GetAllTypes())
				{
					if (type.IsSubclassOf(typeof(TNodeItem)) &&
						!type.IsGenericTypeDefinition &&
						!type.IsAbstract &&
						!type.IsDefined(typeof(System.ObsoleteAttribute), false))
					{
						m_AllTypes.Add(type.Name, type);
					}
				}
			}
			node.IsEventRequired(out System.Type eventType);
			if (m_Types.Count == 0 || 
				m_TreeType != tree.GetType() ||
				m_EventType != eventType)
			{
				m_TreeType = tree.GetType();
				m_EventType = eventType;
				GetTypeNames(tree, node, m_AllTypes, $"Add {typeof(TNodeItem).Name}...", ref m_Types, out m_ClassNames);
			}
		}

		private static void GetTypeNames(
			IActObject tree, 
			IActNode node,
			IReadOnlyDictionary<string, System.Type> allTypes,
			string firstElementName,
			ref List<System.Type> typesToFill,
			out string[] classNames)
		{
			System.Type treeType = tree.GetType();
			System.Type treeContextType = tree.GetContextType();
			node.IsEventRequired(out System.Type nodeEventType);

			SortedActItemNamesAndTypes sortedTypes = new SortedActItemNamesAndTypes();
			foreach (KeyValuePair<string, System.Type> pair in allTypes)
			{
				System.Type type = pair.Value;

				// Filter by context
				System.Reflection.MethodInfo m = type.GetMethod(
					"_EditorGetContext",
					System.Reflection.BindingFlags.Static |
					System.Reflection.BindingFlags.FlattenHierarchy |
					System.Reflection.BindingFlags.Public);
				System.Type itemContextType = (System.Type)m.Invoke(null, new object[] { });
				if (!itemContextType.IsAssignableFrom(treeContextType))
				{
					continue;
				}

				//// Filter by event
				//System.Reflection.MethodInfo m2 = type.GetMethod(
				//	"_EditorGetEvent",
				//	System.Reflection.BindingFlags.Static |
				//	System.Reflection.BindingFlags.FlattenHierarchy |
				//	System.Reflection.BindingFlags.Public);
				//System.Type itemEventType = (System.Type)m2.Invoke(null, new object[] { });
				//if (itemEventType != null)
				//{
				//	if (nodeEventType == null)
				//	{
				//		continue;
				//	}
				//	if (!itemEventType.IsAssignableFrom(nodeEventType))
				//	{
				//		continue;
				//	}
				//}

				string name = type.Name;
				if (System.Attribute.GetCustomAttribute(type, typeof(NodeItemGroupAttribute), true) is NodeItemGroupAttribute groupAtt)
				{
					// Filter by group's allowed tree types
					if (groupAtt.TreeTypes != null && groupAtt.TreeTypes.Length > 0)
					{
						bool allowed = false;
						foreach (System.Type groupTreeType in groupAtt.TreeTypes)
						{
							if (groupTreeType.IsAssignableFrom(treeType))
							{
								allowed = true;
								break;
							}
						}
						if (!allowed)
						{
							continue;
						}
					}
					if (!string.IsNullOrEmpty(groupAtt.GroupName))
					{
						name = Core.Str.Build(groupAtt.GroupName, "/", name);
					}
				}
				sortedTypes.Add(name, type);
			}
			sortedTypes.Get(firstElementName, ref typesToFill, out classNames);
		}

		public bool GUI(Rect r1, out object instance)
		{
			int typeIndex = EditorGUI.Popup(r1, 0, m_ClassNames);
			if (typeIndex > 0)
			{
				instance = System.Activator.CreateInstance(m_Types[typeIndex - 1]);
				return true;
			}
			else
			{
				instance = null;
				return false;
			}
		}
	}

	public class SortedActItemNamesAndTypes : SortedDictionary<string, System.Type>
	{
		public SortedActItemNamesAndTypes() : base(new SortNames())
		{

		}

		public void Get(string firstElementName, ref List<System.Type> typesToFill, out string[] classNames)
		{
			typesToFill.Clear();
			typesToFill.AddRange(Values);
			List<string> names = new List<string>(Keys);
			if (names.Count > 0 && names[0].Contains("/")) // Note: Group tracks sort to the beginning on the list
			{
				for (int i = 1; i < names.Count; i++)
				{
					if (!names[i].Contains("/"))
					{
						names.Insert(i, string.Empty); // Insert divider after grouped tracks
						typesToFill.Insert(i, null);
						break;
					}
				}
			}
			names.Insert(0, string.Empty); // Insert divider at begining of track list
			typesToFill.Insert(0, null);

			names.Insert(0, firstElementName);
			classNames = names.ToArray();
		}

		private class SortNames : IComparer<string>
		{
			int IComparer<string>.Compare(string x, string y)
			{
				int xGroupCount = Count(x, '/');
				int yGroupCount = Count(y, '/');
				if (xGroupCount != yGroupCount)
				{
					return yGroupCount.CompareTo(xGroupCount);
				}
				return x.CompareTo(y);
			}

			private static int Count(string s, char c)
			{
				int count = 0;
				int length = s.Length;
				for (int i = 0; i < length; i++)
				{
					if (s[i] == c)
					{
						count++;
					}
				}
				return count;
			}
		}
	}
}
