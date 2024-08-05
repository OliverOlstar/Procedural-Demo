using System.Collections.Generic;
using UnityEngine;

namespace ODev.PoseAnimator
{
	public class PoseAnimator : UpdateableMonoBehaviour
	{
		[SerializeField]
		private Transform m_Root = null;

		private SOPoseAnimation m_Animation = null;

		public void Add(SOPoseAnimation pAnimation)
		{
			m_Animation = pAnimation;
		}

		protected override void Tick(float pDeltaTime)
		{
			if (m_Animation == null)
			{
				return;
			}

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
