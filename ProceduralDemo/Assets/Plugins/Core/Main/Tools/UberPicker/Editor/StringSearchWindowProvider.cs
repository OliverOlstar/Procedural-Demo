using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.IO;
using System;

public struct StringSearchWindowContext : IAssetPickerAttribute, IAssetPickerPathSource
{
	private string m_CacheKey;
	private bool m_DoCachePaths;
	private string m_Title;
	private Func<List<string>> m_GetPaths;
	private char[] m_PathSeperators;
	private bool m_AllowNull;
	private bool m_ForceFlatten;

	public StringSearchWindowContext(string cacheKey, string title, Func<List<string>> getPaths, char[] pathSeperators, bool allowNull = false, bool forceFlatten = false, bool doCachePaths = true)
	{
		m_CacheKey = cacheKey;
		m_DoCachePaths = doCachePaths;
		m_Title = title;
		m_GetPaths = getPaths;
		m_PathSeperators = pathSeperators;
		m_AllowNull = allowNull;
		m_ForceFlatten = forceFlatten;
	}

	public string CacheKey => m_CacheKey;
	public string PathCacheKey => m_DoCachePaths ? m_CacheKey : null;

	bool IAssetPickerAttribute.AllowNull => m_AllowNull;
	bool IAssetPickerAttribute.ForceFlatten => m_ForceFlatten;
	string IAssetPickerAttribute.OverrideFirstName => null;

	List<string> IAssetPickerPathSource.GetPaths() => m_GetPaths.Invoke();
	char[] IAssetPickerPathSource.GetPathSperators() => m_PathSeperators;
	string IAssetPickerPathSource.GetSearchWindowTitle() => m_Title;
	bool IAssetPickerPathSource.TryGetUnityObjectType(out Type type)
	{
		type = null;
		return false;
	}
}

public class StringSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
	private const string ICONPATH = "Assets/Editor/Textures/SearchWindowStringIcon.png";
	private const string NULLICONPATH = "Assets/Editor/Textures/SearchWindowNoneIcon.png";
	private const string NULLITEMNAME = "None";

	private static readonly Core.AssetDatabaseDependentValues<string, StringSearchWindowProvider> s_CachedSearchWindows =
		new();

	public static void Show(
		in StringSearchWindowContext context,
		Action<string> onSelected)
	{
		StringSearchWindowProvider window = GetOrCreate(context, onSelected);
		SearchWindowContext context2 = new(
			GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
			window.Width,
			window.Height);
		SearchWindow.Open(context2, window);
	}

	public static StringSearchWindowProvider GetOrCreate(
		in StringSearchWindowContext context,
		Action<string> onSelected)
	{
		if (!s_CachedSearchWindows.TryGet(context.CacheKey, out StringSearchWindowProvider provider))
		{
			provider = CreateInstance<StringSearchWindowProvider>();
			UberPickerPathCache cachePaths = UberPickerPathCache.GetPaths(context.PathCacheKey, context, context);
			provider.Init(cachePaths);
			s_CachedSearchWindows.Set(context.CacheKey, provider);
		}
		provider.RegisterOnSelected(onSelected);
		return provider;
	}

	private List<SearchTreeEntry> m_Entries = null;
	private Action<string> m_OnSelected;

	private void RegisterOnSelected(Action<string> onSelected)
	{
		m_OnSelected = onSelected;
	}
	private UberPickerPathCache m_Paths;
	private GUIContent m_NoneEntryContent = null;

	private float m_Width = 0.0f;
	public float Width => m_Width;

	private float m_Height = 0.0f;
	public float Height => m_Height;

	public void Init(UberPickerPathCache paths)
	{
		m_Paths = paths;

		Texture tex = AssetDatabase.GetCachedIcon(NULLICONPATH);
		m_NoneEntryContent = new GUIContent(NULLITEMNAME, tex);

		m_Entries = new List<SearchTreeEntry>(m_Paths.Names.Length);
		for (int i = 0; i < m_Paths.Names.Length; i++)
		{
			string name = m_Paths.Names[i];
			string path = m_Paths.Paths[i];
			int level = m_Paths.ItemLevels[i];
			if (path == string.Empty)
			{
				m_Entries.Add(new SearchTreeGroupEntry(new GUIContent(name), level));
			}
			else if (name == UberPickerPathCache.NULL_ITEM_NAME)
			{
				SearchTreeEntry entry = new(m_NoneEntryContent);
				entry.userData = null;
				entry.level = level;
				m_Entries.Add(entry);
			}
			else
			{
				GUIContent content = Path.HasExtension(path) ? // If this is an asset path, then get it's icon
					new GUIContent(name, AssetDatabase.GetCachedIcon(path)) :
					new GUIContent(name, AssetDatabase.GetCachedIcon(ICONPATH));
				SearchTreeEntry entry = new(content);
				entry.userData = path;
				entry.level = level;
				m_Entries.Add(entry);
			}

			// Calculate content width
			// Note: Groups are very special case-y as they don't have icons, but the drawer draws a blank space there as if there was one, 
			// also need to account for the '>' symbol after the group name
			float width =
				string.IsNullOrEmpty(path) ? EditorStyles.label.CalcSize(new GUIContent(name, tex)).x + 13.0f : // Group
				Path.HasExtension(path) ? EditorStyles.label.CalcSize(new GUIContent(name, tex)).x : // Item is an Asset
				EditorStyles.label.CalcSize(new GUIContent(name)).x; // Item is not an asset
			width += 3.0f; // Small magic number to make things fit more comfortably
			m_Width = Mathf.Max(m_Width, width);
		}

		int largestGroupCount = 0;
		int largestGroupLevel = 0;
		for (int groupLevel = 1; groupLevel < 100; groupLevel++)
		{
			bool found = false;
			int groupCount = 0;
			for (int i = 0; i < m_Paths.Names.Length; i++)
			{
				int level = m_Paths.ItemLevels[i];
				if (level < groupLevel)
				{
					largestGroupCount = Mathf.Max(largestGroupCount, groupCount);
					largestGroupLevel = level;
					groupCount = 0;
				}
				else if (level == groupLevel)
				{
					found = true;
					groupCount++;
				}
			}
			largestGroupCount = Mathf.Max(largestGroupCount, groupCount);
			largestGroupLevel = m_Paths.ItemLevels[^1];
			if (!found)
			{
				break;
			}
		}
		float x = 1.11f;
		float y = 1.6f;
		m_Height = (x * largestGroupCount * EditorGUIUtility.singleLineHeight) + (y * 2 * EditorGUIUtility.singleLineHeight);
		// Adjust height based on screen size so we can make the window as large as possible without it opening in a weird position
		float maxHeight = Screen.currentResolution.height * 0.45f;
		if (m_Height > maxHeight)
		{
			m_Height = maxHeight;
			m_Width += GUI.skin.verticalScrollbar.fixedWidth; // We know we're going to have to scroll so make space to accodate bar
		}
	}

	public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) => m_Entries;

	public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
	{
		m_OnSelected.Invoke(searchTreeEntry.userData as string);
		return true;
	}
}
