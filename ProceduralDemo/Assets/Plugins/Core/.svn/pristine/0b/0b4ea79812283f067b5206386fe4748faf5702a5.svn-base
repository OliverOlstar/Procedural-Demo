
using System.Collections.Generic;
using UnityEngine;

namespace ModelLocator
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Model Locator/Simple")]
	public class SOModelLocatorSimple : SOModelLocator
	{
		private string m_LocatorName = Core.Str.EMPTY;
		public string GetName()
		{
			if (Core.Str.IsEmpty(m_LocatorName))
			{
				m_LocatorName = Core.Str.Build(name, GetInstanceID().ToString());
			}
			return m_LocatorName;
		}

		protected override LocatorInstance InstantiateInternal(ModelLocatorBehaviour model)
		{
			string locatorName = GetName();
			if (model.TryGetBone(locatorName, out Transform locator))
			{
				return new LocatorInstance(this, locator);
			}
			Transform locatorParent = GetParent(model);
			locator = CreateLocator(locatorParent, locatorName, m_Position, m_Quaternion);
			if (model != null)
			{
				model.AddBone(locatorName, locator);
			}
			return new LocatorInstance(this, locator);
		}

		public static Transform CreateLocator(Transform locatorParent, string locatorName, Vector3 localPosition, Quaternion localRotation)
		{
			Transform locator = new GameObject(locatorName).transform;
			locator.SetParent(locatorParent);
			locator.localPosition = localPosition;
			locator.localRotation = localRotation;
			// A somewhat common technique is to mirror characters usually by inverting the x scale
			// It's also often that the models parent has been scaled, setting the local scale to 1
			// means the models scale will be applied to any FX parented to this locator
			// So instead of setting the local scale to 1, we just want to remove any mirroring that has been applied
			locator.localScale = Core.Util.Vector3Abs(locator.localScale);
			return locator;
		}

		internal override Quaternion ModifyRotation(Transform root) => root.transform.rotation;
		internal override Vector3 ModifyPosition(Transform root) => root.transform.position;
	}

}
