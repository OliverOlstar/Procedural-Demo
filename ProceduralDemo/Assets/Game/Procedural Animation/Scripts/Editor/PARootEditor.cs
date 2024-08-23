using UnityEditor;
using PA;

[CustomEditor(typeof(PARoot))]
public class PARootEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		if (target is PARoot root)
		{
			PARootEditor_DuplicateLimbs.DrawGUI(root);
		}
	}
}
