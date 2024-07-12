using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev
{
	public class Health : CharacterValue, IDamageable
	{
		public delegate void DamageEvent(float pValue, GameObject pSender, Vector3 pPoint, Vector3 pDirection);

		public DamageEvent OnDamageEvent;

		[SerializeField]
		private SOTeam m_Team = null;

		[Header("Death")]
		[SerializeField]
		private bool m_DisableCollidersOnDeath = true;
		private Collider[] m_Colliders = new Collider[0];

		[Space]
		[ColorPalette("UI")]
		public Color m_DeathColor = Color.grey;
		[ColorPalette("UI")]
		public Color m_DamageColor = Color.red;
		[ColorPalette("UI")]
		public Color m_HealColor = Color.green;

		protected override void Start()
		{
			base.Start();

			if (m_DisableCollidersOnDeath)
			{
				m_Colliders = GetComponentsInChildren<Collider>();
			}

			OnValueOutEvent.AddListener(Death);
			Initalize();
		}

		protected virtual void Initalize() { }

		public virtual void Damage(float pValue, GameObject pSender, Vector3 pPoint, Vector3 pDirection, Color pColor)
			=> Damage(pValue, pSender, pPoint, pDirection); // IDamageable

		[Button()]
		public virtual void Damage(float pValue, GameObject pSender, Vector3 pPoint, Vector3 pDirection) // IDamageable
		{
			Modify(-pValue);
			OnDamageEvent?.Invoke(pValue, pSender, pPoint, pDirection);
		}

		public virtual void Death()
		{
			if (!m_DisableCollidersOnDeath)
			{
				return;
			}
			foreach (Collider c in m_Colliders)
			{
				c.enabled = false;
			}
		}

		GameObject IDamageable.GetGameObject() => gameObject;
		IDamageable IDamageable.GetParentDamageable() => this;
		SOTeam IDamageable.GetTeam() => m_Team;

		public void Respawn()
		{
			m_Value = m_MaxValue;
			foreach (BarValue bar in m_UIBars)
			{
				bar.InitValue(1);
			}
			OnValueIn();
		}
	}
}