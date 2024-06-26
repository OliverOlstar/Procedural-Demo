using UnityEngine;
using UnityEngine.InputSystem;

namespace OCore.Weapon.Demo
{
	public class DemoWeaponAiming : MonoBehaviour
	{
		[SerializeField]
		private Camera m_Camera = null;

		void Update()
		{
			if (Physics.Raycast(m_Camera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit))
			{
				transform.LookAt(hit.point);
			}
		}
	}
}