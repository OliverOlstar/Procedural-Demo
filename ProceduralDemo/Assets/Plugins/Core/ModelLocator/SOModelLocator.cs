using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelLocator
{
	public abstract class SOModelLocator : ScriptableObject
	{
		[SerializeField]
		private Core.InspectorNotes m_Notes = new Core.InspectorNotes();

		[SerializeField]
		protected Vector3 m_Position = Vector3.zero;
		public Vector3 Position => m_Position;
		[SerializeField]
		protected Vector3 m_Rotation = Vector3.zero;
		[SerializeField, HideInInspector]
		protected Quaternion m_Quaternion = Quaternion.identity;
		public Quaternion Rotation => m_Quaternion;

		[SerializeField]
		private string m_ParentName = string.Empty;
		public string ParentName => m_ParentName;

		private void OnValidate()
		{
			m_Quaternion = Quaternion.Euler(m_Rotation);
		}

		protected Transform GetParent(ModelLocatorBehaviour model)
		{
			if (string.IsNullOrEmpty(m_ParentName))
			{
				return model.gameObject.transform; // Default to root transform
			}
			if (!model.TryGetBone(m_ParentName, out Transform bone))
			{
				Debug.LogError(Core.Str.Build(name, " ", GetType().Name, ".GetLocator() Could not find ", m_ParentName, " in object ", model.name));
				return model.gameObject.transform;
			}
			return bone;
		}

		public LocatorInstance Instantiate(GameObject gameOject)
		{
			ModelLocatorBehaviour behaviour = Core.Util.GetOrAddComponentInChildren<ModelLocatorBehaviour>(gameOject);
			return InstantiateInternal(behaviour);
		}

		public LocatorInstance Instantiate(ModelLocatorBehaviour model)
		{
			return InstantiateInternal(model);
		}

		public LocatorInstance Instantiate(IModelLocatorContext source)
		{
			ModelLocatorBehaviour behaviour = source.ModelLocatorBehaviour ?? 
				throw new System.ArgumentNullException("SOModelLocator.Instantiate() IModelLocatorBehaviourSource.ModelLocatorBehaviour cannot be null");
			return InstantiateInternal(behaviour);
		}

		protected abstract LocatorInstance InstantiateInternal(ModelLocatorBehaviour model);

		internal abstract Quaternion ModifyRotation(Transform root);
		internal abstract Vector3 ModifyPosition(Transform root);
	}

	public interface IModelLocatorContext
	{
		ModelLocatorBehaviour ModelLocatorBehaviour { get; }
	}

	public struct LocatorInstance
	{
		private SOModelLocator m_Locator;
		private Transform m_Transform;
		public Transform Transform => m_Transform;

		public Vector3 Position()
		{
			return m_Locator.ModifyPosition(m_Transform);
		}

		public Quaternion Rotation()
		{
			return m_Locator.ModifyRotation(m_Transform);
		}

		public Vector3 Forward()
		{
			return m_Locator.ModifyRotation(m_Transform) * Vector3.forward;
		}
		public Vector3 Right()
		{
			return m_Locator.ModifyRotation(m_Transform) * Vector3.right;
		}
		public Vector3 Up()
		{
			return m_Locator.ModifyRotation(m_Transform) * Vector3.up;
		}

		public LocatorInstance(SOModelLocator root, Transform parentTransform)
		{
			m_Locator = root;
			m_Transform = parentTransform;
		}

		public void AttachAsChild(Transform transform)
		{
			transform.position = Position();
			transform.rotation = Rotation();
			transform.parent = Transform;
		}
	}
}
