using UnityEditor;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
	[CustomEditor(typeof(GameObjectGameplayTagContainer))]
	public class GameObjectGameplayTagContainerEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (!Application.isPlaying)
			{
				return;
			}

			var container = ((GameObjectGameplayTagContainer)target).GameplayTagContainer;
			foreach (var tag in container.GetTags())
			{
				GUILayout.Label(tag.Name);
			}
		}
	}
}