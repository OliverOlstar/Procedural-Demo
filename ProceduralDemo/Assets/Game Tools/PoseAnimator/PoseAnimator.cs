using System.Collections;
using System.Collections.Generic;
using ODev.Picker;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public class PoseAnimator : UpdateableMonoBehaviour
	{
		[SerializeField]
		private Transform m_Root = null;
		[SerializeField, Asset]
		private SOPoseAnimation m_Animation = new();

		protected override void Tick(float pDeltaTime)
		{
			Queue<Transform> transforms = new();
			int index = 0;

			transforms.Enqueue(m_Root);
			while (transforms.Count > 0)
			{
				// Current
				Transform t = transforms.Dequeue();
				m_Animation.Apply(t, index);

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
