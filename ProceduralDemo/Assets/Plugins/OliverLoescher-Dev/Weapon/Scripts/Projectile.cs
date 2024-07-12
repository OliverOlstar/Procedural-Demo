using UnityEngine;
using ODev.Util;
using Sirenix.OdinInspector;

namespace ODev.Weapon
{
	public class Projectile : PoolElement
	{
		[Required]
		public SOProjectile Data = null;

		public Rigidbody MyRigidbody = null;
		public Collider HitboxCollider = null;
		public Collider PhysicsCollider = null;
		public GameObject Sender = null;
		private SOTeam Team = null;

		protected bool m_CanDamage = true;
		protected bool m_ActiveSelf = true;
		private int m_CurrentFrame = 0;
		private int m_LastHitFrame = 0;
		private Collider m_LastHitCollider = null;

		[Header("Floating Numbers")]
		[ColorPalette("UI"), SerializeField]
		private Color m_HitColor = new(1, 0, 0, 1);
		[ColorPalette("UI"), SerializeField]
		private Color m_CritColor = new(1, 1, 0, 1);

		private Vector3 m_StartPos = new();
		private Vector3 m_PreviousPosition = new();

		[SerializeField]
		private float m_SpawnOffsetZ = 0;

		public override void ReturnToPool()
		{
			m_ActiveSelf = false;
			CancelInvoke();
			base.ReturnToPool();
		}

		public override void OnExitPool()
		{
			m_CurrentFrame = 0;
			MyRigidbody.isKinematic = false;
			MyRigidbody.useGravity = false;
			m_CanDamage = true;
			m_LastHitCollider = null;
			HitboxCollider.enabled = true;
			if (PhysicsCollider != null)
			{
				PhysicsCollider.enabled = false;
			}
			m_ActiveSelf = true;

			base.OnExitPool();
		}

		public void Init(Vector3 pPosition, Vector3 pDirection, GameObject pSender, SOTeam pTeam = null)
		{
			transform.SetPositionAndRotation(pPosition, Quaternion.LookRotation(pDirection));
			MyRigidbody.velocity = pDirection.normalized * Random2.Range(Data.ShootForce);
			transform.position += transform.forward * m_SpawnOffsetZ;

			m_StartPos = transform.position;
			m_PreviousPosition = transform.position;

			Sender = pSender;
			Team = pTeam;
			Invoke(nameof(DoLifeEnd), Random2.Range(Data.LifeTime));
		}

		private void FixedUpdate()
		{
			if (!m_ActiveSelf)
			{
				return;
			}

			bool updateRot = false;
			if (Data.BulletGravity > 0)
			{
				MyRigidbody.AddForce(Data.BulletGravity * Time.fixedDeltaTime * Vector3.down, ForceMode.VelocityChange);
				updateRot = true;
			}

			if (updateRot)
			{
				transform.rotation = Quaternion.LookRotation(MyRigidbody.velocity);
			}
		}

		protected virtual void Update()
		{
			if (!m_ActiveSelf || !m_CanDamage)
			{
				return;
			}

			m_CurrentFrame++; // Used to ignore collision on first two frames
			if (m_CurrentFrame >= -1 && Data.UseRaycast && // Raycast Projectile
				Physics.Linecast(m_PreviousPosition, transform.position, out RaycastHit hit, Data.LayerMask, QueryTriggerInteraction.Ignore))
			{
				DoHitOther(hit.collider, hit.point);
			}
			m_PreviousPosition = transform.position;
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!m_ActiveSelf)
			{
				return;
			}
			DoHitOther(other, transform.position);
		}

		#region Hit/Damage
		private void DoHitOther(Collider pOther, Vector3 pPoint)
		{
			if (!m_CanDamage || m_CurrentFrame < 1 || pOther.isTrigger || pOther == m_LastHitCollider || IsSender(pOther.transform))
			{
				return;
			}

			bool didDamage = false;
			if (pOther.TryGetComponent<IDamageable>(out IDamageable damageable))
			{
				bool isSameTeam = SOTeam.Compare(damageable.GetTeam(), Team);
				if (isSameTeam && Team.IgnoreTeamCollisions)
				{
					return;
				}
				if (!isSameTeam || Team.TeamDamage)
				{
					UnityEngine.Debug.Log($"[{nameof(ODev.Weapon.Projectile)}] {nameof(DamageOther)}({pOther.name}, {damageable.GetGameObject().name}, {(damageable.GetTeam() == null ? "No Team" : damageable.GetTeam().name)})", pOther);
					DamageOther(damageable, pPoint);
					didDamage = true;
				}
			}

			m_LastHitFrame = m_CurrentFrame;
			m_LastHitCollider = pOther;
			DoHitOtherInternal(pOther, didDamage);
		}

		protected virtual bool DoHitOtherInternal(Collider pOther, bool pDidDamage)
		{
			if ((pDidDamage ? Data.ProjectileDamagableCollision : Data.ProjectileEnviromentCollision).DoCollision(this, pOther, ref m_CanDamage, ref m_ActiveSelf))
			{
				ReturnToPool();
				return true;
			}
			return false;
		}

		private void DamageOther(IDamageable damageable, Vector3 point)
		{
			// Rigidbody otherRb = other.GetComponentInParent<Rigidbody>();
			// if (otherRb != null)
			//	 otherRb.AddForceAtPosition(rigidbody.velocity.normalized * data.hitForce, point);

			if (Random.value > Data.CritChance01)
			{
				damageable.Damage(Data.Damage, Sender, transform.position, MyRigidbody.velocity);
			}
			else
			{
				damageable.Damage(Mathf.RoundToInt(Data.CritDamageMultiplier * Data.Damage), Sender, transform.position, MyRigidbody.velocity, m_CritColor);
			}
		}

		protected virtual void DoLifeEnd()
		{
			Data.ProjectileLifeEnd.DoCollision(this, null, ref m_CanDamage, ref m_ActiveSelf);
			ReturnToPool();
		}

		private bool IsSender(Transform other)
		{
			if (Sender == null)
			{
				return false;
			}
			if (other == Sender.transform)
			{
				return true;
			}
			if (other.parent == null)
			{
				return false;
			}
			return IsSender(other.parent);
		}
		#endregion

		private void PlayParticle(ParticleSystem pParticle, Vector3 pPosition)
		{
			if (pParticle == null)
			{
				return;
			}
			pParticle.gameObject.SetActive(true);
			pParticle.Play();
			pParticle.transform.position = pPosition;
			pParticle.transform.SetParent(null);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Vector3 endPos = transform.position + (transform.forward * -m_SpawnOffsetZ);
			Gizmos.DrawLine(transform.position, endPos);
			Gizmos.DrawWireSphere(endPos, 0.01f);

			if (Data == null)
			{
				return;
			}
			if (Data.ProjectileEnviromentCollision != null)
			{
				Data.ProjectileEnviromentCollision.DrawGizmos(this);
			}
			if (Data.ProjectileDamagableCollision != null)
			{
				Data.ProjectileDamagableCollision.DrawGizmos(this);
			}
			if (Data.ProjectileLifeEnd != null)
			{
				Data.ProjectileLifeEnd.DrawGizmos(this);
			}
		}
	}
}