using UnityEngine;
using Sirenix.OdinInspector;

namespace OCore
{
	public class DamageReciever : MonoBehaviour, IDamageable
	{
		[SerializeField]
		private IDamageable m_Parent = null;

		[DisableInPlayMode, SerializeField]
		private GameObject m_ParentObject = null;
		[SerializeField]
		private float m_DamageMultiplier = 1.0f;

		private void Awake()
		{
			if (m_ParentObject == null)
			{
				m_Parent = transform.parent.GetComponentInParent<IDamageable>();

				if (m_Parent == null)
				{
					Debug.LogError("[DamageReciever] Couldn't find IDamagable through GetComponentInParent, destroying self", gameObject);
					Destroy(this);
				}
			}
			else
			{

				if (!m_ParentObject.TryGetComponent<IDamageable>(out m_Parent))
				{
					Debug.LogError("[DamageReciever] Couldn't find IDamagable on parentObject, destroying self", gameObject);
					Destroy(this);
				}
			}
		}

		void IDamageable.Damage(float pValue, GameObject pAttacker, Vector3 pPoint, Vector3 pDirection, Color pColor)
		{
			m_Parent.Damage(DamageMultipler(pValue), pAttacker, pPoint, pDirection, pColor);
		}
		void IDamageable.Damage(float pValue, GameObject pAttacker, Vector3 pPoint, Vector3 pDirection)
		{
			m_Parent.Damage(DamageMultipler(pValue), pAttacker, pPoint, pDirection);
		}
		private float DamageMultipler(float pValue) => pValue * m_DamageMultiplier;

		GameObject IDamageable.GetGameObject()
		{
			if (m_Parent == null)
			{
				return gameObject;
			}
			return m_Parent.GetGameObject();
		}
		IDamageable IDamageable.GetParentDamageable()
		{
			if (m_Parent == null)
			{
				return this;
			}
			return m_Parent.GetParentDamageable();
		}
		SOTeam IDamageable.GetTeam()
		{
			if (m_Parent == null)
			{
				return null;
			}
			return m_Parent.GetTeam();
		}
	}
}