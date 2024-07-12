using System.Collections.Generic;

namespace ODev.Picker
{
	public static class UberPickerPathOptimizer
	{
		public static void Calculate(
			List<string> input,
			char[] seperators,
			string title,
			out List<string> names,
			out List<string> paths,
			out List<int> levels)
		{
			// Create tree
			TreeNode root = new(title, string.Empty, null);
			foreach (string item in input)
			{
				root.TryAddElement(item, seperators);
			}

			// Compact tree
			root.CompactGroups();

			// Convert tree back to string arrays
			names = new List<string>();
			paths = new List<string>();
			levels = new List<int>();
			root.FillList(ref names, ref paths, ref levels, 0);
		}

		private class TreeNode
		{
			public string Name = string.Empty;
			public string Path = string.Empty; // If string.Empty, this is group & not an item

			public TreeNode Parent = null;
			public List<TreeNode> Children = new();

			public TreeNode(string name, string path, TreeNode parent)
			{
				Name = name;
				Path = path;
				Parent = parent;
			}

			public bool TryAddElement(string path, char[] separators)
			{
				string[] separatedPath = path.Split(separators);

				// Ignore paths which contain a file or folder that starts with '_' character, this convention lets us deliberately hide things from UberPicker
				for (int i = 1; i < separatedPath.Length; i++)
				{
#if UNITY_2020
				if (separatedPath[i].StartsWith("_"))
#else
					if (separatedPath[i].StartsWith('_'))
#endif
					{
						return false;
					}
				}
				string itemName = System.IO.Path.GetFileNameWithoutExtension(path);
				AddElementRecursive(separatedPath, 0, itemName, path);
				return true;
			}

			private void AddElementRecursive(string[] groups, int groupIndex, string name, string path)
			{
				// Last group element is the actual asset, should be == to 'name'
				if (groupIndex == groups.Length - 1)
				{
					Children.Add(new TreeNode(name, path, this));
					return;
				}

				string groupName = groups[groupIndex];
				TreeNode node = null;
				foreach (TreeNode child in Children)
				{
					if (child.Name == groupName)
					{
						node = child;
						break;
					}
				}
				if (node == null) // If group doesn't already exist
				{
					node = new TreeNode(groupName, string.Empty, this);
					Children.Add(node);
				}
				groupIndex++;
				node.AddElementRecursive(groups, groupIndex, name, path); // Recurse
			}

			public void RemoveSelf()
			{
				if (Parent == null)
				{
					return; // Cannot remove Root
				}
				foreach (TreeNode child in Children)
				{
					if (string.IsNullOrEmpty(child.Path) && Name != "Assets")
					{
						child.Name = Name + "/" + child.Name;
					}
					child.Parent = Parent;
				}
				Parent.Children.Remove(this);
				Parent.Children.AddRange(Children);
			}

			public void CompactGroups() // Should be called on root Node
			{
				// If only one child group, remove it
				while (Children.Count == 1 && Children[0].Path == string.Empty)
				{
					// Normally we remove groups the other way removing itself if they have few children 
					// but we can not remove the root so we have to remove the other way for it by removing the children.
					Children[0].RemoveSelf();
				}

				// Compact Children
				for (int i = 0; i < Children.Count; i++)
				{
					if (Children[i].CompactGroupsInternal())
					{
						i--;
					}
				}
			}

			private bool CompactGroupsInternal()
			{
				// Is item, not a group
				if (!string.IsNullOrEmpty(Path))
				{
					return false;
				}

				bool keepAlive = false;
				foreach (TreeNode child in Children)
				{
					if (!string.IsNullOrEmpty(child.Path))
					{
						keepAlive = true;
						break;
					}
				}
				if (!keepAlive)
				{
					keepAlive = Children.Count > 1;
				}

				// Compact Children
				for (int i = 0; i < Children.Count; i++)
				{
					if (Children[i].CompactGroupsInternal())
					{
						i--;
					}
				}

				// Remove self
				if (!keepAlive)
				{
					RemoveSelf();
					return true;
				}

				return false;
			}

			public void FillList(ref List<string> names, ref List<string> paths, ref List<int> levels, int level)
			{
				names.Add(Name);
				paths.Add(Path);
				levels.Add(level);

				// Add Groups
				foreach (TreeNode child in Children)
				{
					if (child.Children.Count > 0)
					{
						child.FillList(ref names, ref paths, ref levels, level + 1);
					}
				}

				// Add Items
				foreach (TreeNode child in Children)
				{
					if (child.Children.Count == 0)
					{
						child.FillList(ref names, ref paths, ref levels, level + 1);
					}
				}
			}
		}
	}
}