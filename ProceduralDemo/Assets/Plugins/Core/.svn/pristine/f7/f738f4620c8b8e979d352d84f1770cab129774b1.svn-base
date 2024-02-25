using UnityEngine;
using UnityEditor;
using System.Collections;
 
public class ReplaceSelection : ScriptableWizard
{
	static GameObject replacement = null;
	static bool       keep        = false;
	static bool       names       = false;
 
	public GameObject ReplacementObject = null;
	public bool       KeepOriginals     = false;
	public bool       KeepNames         = false;
 
	[MenuItem("Core/Selection/Replace")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard( "Replace Selection", typeof( ReplaceSelection ), "Replace" );
	}
 
	public ReplaceSelection()
	{
		ReplacementObject = replacement;
		KeepOriginals     = keep;
		KeepNames         = names;
	}
 
	void OnWizardUpdate()
	{
		replacement = ReplacementObject;
		keep        = KeepOriginals;
		names       = KeepNames;
	}
 
	void OnWizardCreate()
	{
		if( replacement == null )
		{
			return;
		}
 
		Transform[] transforms = null;
		#if UNITY_2020_1_OR_NEWER
		transforms = Selection.GetTransforms( SelectionMode.TopLevel | SelectionMode.Editable );
		#else
transforms = Selection.GetTransforms( SelectionMode.TopLevel | SelectionMode.OnlyUserModifiable );
#endif	 
		foreach( Transform t in transforms )
		{
			GameObject g;
			PrefabAssetType pref = PrefabUtility.GetPrefabAssetType( replacement );
	 
			if( pref == PrefabAssetType.Regular || pref == PrefabAssetType.Model )
			{
				g = ( GameObject )PrefabUtility.InstantiatePrefab( replacement );
			}
			else
			{
				g = ( GameObject )Editor.Instantiate( replacement );
			}
	 
			Transform gTransform = g.transform;
			gTransform.parent = t.parent;

			if( names )
			{
				g.name = t.name;
			}
			gTransform.localPosition = t.localPosition;
			gTransform.localScale = t.localScale;
			gTransform.localRotation = t.localRotation;

			g.SetActive(t.gameObject.activeSelf);
		}
	 
		if( !keep )
		{
			foreach( GameObject g in Selection.gameObjects )
			{
				GameObject.DestroyImmediate( g );
			}
		}
    }
}