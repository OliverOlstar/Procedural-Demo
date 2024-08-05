using System.Collections;
using System.Collections.Generic;
using ODev.Picker;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public class PoseClipCreator : MonoBehaviour
	{
		[SerializeField, Asset]
		private SOPoseClip m_Clip = null;
		[SerializeField]
		private Transform m_Root = null;

		[Button]
		private void CopyRootPoseToClip()
		{
			Queue<Transform> transforms = new();
			List<SOPoseClip.PoseKey> keys = new();

			transforms.Enqueue(m_Root);
			while (transforms.Count > 0)
			{
				// Current
				Transform t = transforms.Dequeue();
				keys.Add(new SOPoseClip.PoseKey(t.localPosition, t.localRotation, t.localScale));
				this.Log(keys[^1].ToString());

				// Next
				for (int i = 0; i < t.childCount; i++)
				{
					transforms.Enqueue(t.GetChild(i));
				}
			}

			m_Clip.SetKeys(keys.ToArray());
		}

		[Button]
		private void ApplyClipToRoot()
		{
			Queue<Transform> transforms = new();
			int index = 0;

			transforms.Enqueue(m_Root);
			while (transforms.Count > 0)
			{
				// Current
				Transform t = transforms.Dequeue();
				m_Clip.Apply(t, index);
				t.localScale = Vector3.one;

				// Next
				for (int i = 0; i < t.childCount; i++)
				{
					transforms.Enqueue(t.GetChild(i));
				}

				index++;
			}
		}
	}
}
