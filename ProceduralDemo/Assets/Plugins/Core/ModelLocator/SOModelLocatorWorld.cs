using UnityEngine;

namespace ModelLocator
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Model Locator/World")]
	public class SOModelLocatorWorld : SOModelLocator
	{
		protected override LocatorInstance InstantiateInternal(ModelLocatorBehaviour model)
		{
			Transform locatorParent = GetParent(model);
			return new LocatorInstance(this, locatorParent);
		}

		internal override Quaternion ModifyRotation(Transform root)
		{
			return m_Quaternion;
		}

		internal override Vector3 ModifyPosition(Transform root)
		{
			return root.transform.position + m_Position;
		}
	}
}
