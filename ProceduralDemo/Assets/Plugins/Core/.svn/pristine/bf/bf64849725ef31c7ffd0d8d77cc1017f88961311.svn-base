using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModelLocator
{
	public class ModelLocatorBehaviour : MonoBehaviour
	{
		private Dictionary<string, Transform> m_Bones = new Dictionary<string, Transform>();
		public IEnumerable<Transform> Bones => m_Bones.Values;

		public virtual Transform RootTransform => transform;

		protected virtual void Awake()
		{
			Transform root = transform;
			if (gameObject.TryGetComponentInChildren(out Animator animator)) // Animator should be on the top level of every model
			{
				root = animator.transform;
			}
			CollectBones(root);
		}

		private void CollectBones(Transform t)
		{
			foreach (Transform bone in t)
			{
				AddBone(bone.name, bone);
				CollectBones(bone);
			}
		}

		public Transform GetBone(string name)
		{
			Transform bone = null;
			m_Bones.TryGetValue(name, out bone);
			return bone;
		}

		public bool TryGetBone(string name, out Transform bone) => m_Bones.TryGetValue(name, out bone);

		public void AddBone(string name, Transform bone)
		{
			if (m_Bones.ContainsKey(bone.name))
			{
				Debug.LogWarning(Core.Str.Build(
					name, ".UnitModelBehaviour.AddBone() Overwriting a bone with the same name \"", bone.name, "\".",
					"\n Original: ", Core.DebugUtil.GetScenePath(m_Bones[bone.name].gameObject), ".", bone.name,
					"\nDuplicate: ", Core.DebugUtil.GetScenePath(bone.gameObject), ".", bone.name));
			}
			m_Bones[bone.name] = bone;
		}
	}
}
