using UnityEngine;

namespace OCore.UI
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasCameraSetter : MonoBehaviour
	{
		[SerializeField, Min(0.0f)]
		private float m_PlaneDistance = 0.101f;

		private void Start()
		{
			if (!TryGetComponent(out Canvas canvas))
            {
				Debug.LogError($"There is no canvas with this {GetType().Name}. This should never happen.", this);
				return;
            }
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = Camera.main;
			canvas.planeDistance = m_PlaneDistance;
			Destroy(this);
		}
	}
}