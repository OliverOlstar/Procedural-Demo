using System.Collections.Generic;
using ODev.Util;
using UnityEditor;
using UnityEngine;

namespace ODev.PoseAnimator
{
	[CreateAssetMenu(fileName = "New Pose Clip", menuName = "PoseAnimator/Clip", order = 0)]
	public class SOPoseClip : ScriptableObject
	{
		[SerializeField]
		private PoseKey[] m_Keys;

		public IReadOnlyList<PoseKey> Keys => m_Keys;

		public void SetKeys(PoseKey[] pKeys)
		{
			m_Keys = pKeys;
			EditorUtility.SetDirty(this);
			EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
		}
	}
}
