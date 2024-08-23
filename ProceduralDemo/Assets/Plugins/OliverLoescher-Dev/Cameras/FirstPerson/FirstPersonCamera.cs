using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev.Camera
{
	public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField]
private Transform cameraTransform = null;
        [SerializeField, MinMaxSlider(-90, 90, true)] private Vector2 cameraYClamp = new(-40, 50);

        public void OnCameraMove(Vector2 pInput)
        {
            RotateCamera(pInput);
        }

        private void RotateCamera(Vector2 pInput)
        {
            Vector3 euler = cameraTransform.eulerAngles;
            euler.x = Mathf.Clamp(Util.Func.SafeAngle(euler.x - pInput.y), cameraYClamp.x, cameraYClamp.y);
            euler.y += pInput.x;
            euler.z = 0.0f;
            cameraTransform.rotation = Quaternion.Euler(euler);
        }
    }
}