using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;

namespace ODev.Weapon
{
	public class Weapon : MonoBehaviour
	{
		public enum MultiMuzzleType
		{
			FirstOnly,
			Loop,
			PingPong,
			Random,
			RandomNotOneAfterItself,
			RandomAllOnce
		}

		[SerializeField, Required]
		private SOWeapon m_Data = null;
		public SOTeam Team = null;
		[ShowIf("@muzzlePoints.Length > 1"), SerializeField]
		protected MultiMuzzleType m_MultiMuzzleType = MultiMuzzleType.RandomNotOneAfterItself;
		public bool CanShoot = true;

		[Header("References")]
		[SerializeField]
		protected Transform[] m_MuzzlePoints = new Transform[1];
		[SerializeField]
		private ParticleSystem m_MuzzleFlash = null;
		[ShowIf("@muzzleFlash != null"), SerializeField]
		private Vector3 m_MuzzleFlashRelOffset = new();

		[Space]
		public GameObject Sender = null;
		[SerializeField]
		private Rigidbody m_RecoilBody = null;

		[FoldoutGroup("Unity Events")] public UnityEvent OnShoot;
		[FoldoutGroup("Unity Events")] public UnityEvent OnFailedShoot;

		private SOWeaponShootStartBase m_ShootStart = null;
		private SOWeaponSpreadBase m_Spread = null;
		public bool IsShooting { get; private set; } = false;

		public SOWeapon Data => m_Data;

		private void Start()
		{
			if (m_Data == null)
			{
				return;
			}
			m_ShootStart = Instantiate(m_Data.ShootStart).Init(Shoot);
			m_Spread = Instantiate(m_Data.Spread).Init();
			Init();
		}

		private void Reset()
		{
			Sender = gameObject;
		}

		protected virtual void Init() { }

		public void SetData(SOWeapon pData)
		{
			m_Data = pData;
			Start();
		}

		public void ShootStart()
		{
			m_ShootStart.ShootStart();
		}

		public void ShootEnd()
		{
			m_ShootStart.ShootEnd();
		}

		private void Update()
		{
			if (m_ShootStart == null || m_Spread == null)
			{
				return;
			}
			m_ShootStart.OnUpdate(Time.deltaTime);
			m_Spread.OnUpdate(Time.deltaTime);
		}

		public void Shoot()
		{
			if (!CanShoot)
			{
				OnShootFailed();
				return;
			}

			m_ShootStart.OnShoot();

			// Bullet
			Transform muzzle = GetMuzzle();
			SpawnShot(muzzle);

			// Recoil
			if (m_RecoilBody != null && m_Data.RecoilForce != Vector3.zero)
			{
				m_RecoilBody.AddForceAtPosition(muzzle.TransformDirection(m_Data.RecoilForce), muzzle.position, ForceMode.VelocityChange);
			}

			// Spread
			m_Spread.OnShoot();

			// Particles
			if (m_MuzzleFlash != null)
			{
				if (m_MuzzleFlash.transform.parent != muzzle)
				{
					m_MuzzleFlash.transform.SetParent(muzzle);
					m_MuzzleFlash.transform.SetLocalPositionAndRotation(m_MuzzleFlashRelOffset, Quaternion.identity);
				}
				m_MuzzleFlash.Play();
			}

			// Audio
			m_Data.ShotSound.Play(transform.position);

			// Event
			OnShoot?.Invoke();
		}

		protected virtual void SpawnShot(Transform pMuzzle)
		{
			for (int i = 0; i < m_Data.ProjectilesPerShot; i++)
			{
				if (m_Data.MyBulletType == SOWeapon.BulletType.Raycast)
				{
					SpawnRaycast(pMuzzle.position, pMuzzle.forward);
					return;
				}
				Vector3 dir = m_Spread.ApplySpread(pMuzzle.forward);
				SpawnProjectile(pMuzzle.position, dir);
			}
		}

		protected virtual void OnShootFailed()
		{
			m_Data.FailedShotSound.Play(transform.position);
			OnFailedShoot?.Invoke();
		}

		protected virtual Projectile SpawnProjectile(Vector3 pPoint, Vector3 pDirection)
		{
			// Spawn projectile
			GameObject projectile;
			projectile = ObjectPoolDictionary.Get(m_Data.ProjectilePrefab);
			projectile.SetActive(true);

			Projectile projectileScript = projectile.GetComponentInChildren<Projectile>();
			projectileScript.Init(pPoint, pDirection, Sender);

			// Audio
			m_Data.ShotSound.Play(transform.position); // TODO Move this incase bulletsPerShot > 1
			OnShoot?.Invoke();

			return projectileScript;
		}

		protected virtual void SpawnRaycast(Vector3 pPoint, Vector3 pForward)
		{
			Vector3 dir = m_Spread.ApplySpread(pForward);
			if (Physics.Raycast(pPoint, dir, out RaycastHit hit, m_Data.Range, m_Data.LayerMask, QueryTriggerInteraction.Ignore))
			{
				ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);

				// push object if rigidbody
				Rigidbody hitRb = hit.collider.attachedRigidbody;
				// if (hitRb != null)
				//	 hitRb.AddForceAtPosition(data.hitForce * dir, hit.point);

				// Damage my script if possible
				IDamageable a = hit.collider.GetComponent<IDamageable>();
				// if (a != null)
				//	 a.Damage(data.damage, sender, hit.point, hit.normal);
			}
		}

		public virtual void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo)
		{
			if (!m_Data.HitFXPrefab)
			{
				return;
			}
			Instantiate(m_Data.HitFXPrefab, position, rotation);
		}

		private int m_LastMuzzleIndex = 0;
		private bool m_MuzzlePingPongDirection = true;
		private readonly List<int> m_MuzzleIndexList = new();
		protected Transform GetMuzzle()
		{
			switch (m_MultiMuzzleType)
			{
				case MultiMuzzleType.Loop: // Loop ////////////////////////////////////////
					m_LastMuzzleIndex++;
					if (m_LastMuzzleIndex == m_MuzzlePoints.Length)
					{
						m_LastMuzzleIndex = 0;
					}

					return m_MuzzlePoints[m_LastMuzzleIndex];

				case MultiMuzzleType.PingPong: // PingPong ////////////////////////////////
					if (m_MuzzlePingPongDirection)
					{
						m_LastMuzzleIndex++; // Forward
						if (m_LastMuzzleIndex == m_MuzzlePoints.Length - 1)
						{
							m_MuzzlePingPongDirection = false;
						}
					}
					else
					{
						m_LastMuzzleIndex--; // Back
						if (m_LastMuzzleIndex == 0)
						{
							m_MuzzlePingPongDirection = true;
						}
					}
					return m_MuzzlePoints[m_LastMuzzleIndex];

				case MultiMuzzleType.Random: // Random ////////////////////////////////////
					return m_MuzzlePoints[Random.Range(0, m_MuzzlePoints.Length)];

				case MultiMuzzleType.RandomNotOneAfterItself: // RandomNotOneAfterItself //
					int i = Random.Range(0, m_MuzzlePoints.Length);
					if (i == m_LastMuzzleIndex)
					{
						// If is previous offset to new index
						i += Random.Range(1, m_MuzzlePoints.Length);
						// If past max, loop back around
						if (i >= m_MuzzlePoints.Length)
						{
							i -= m_MuzzlePoints.Length;
						}
					}
					m_LastMuzzleIndex = i;
					return m_MuzzlePoints[i];

				case MultiMuzzleType.RandomAllOnce: // RandomAllOnce //////////////////////
					if (m_MuzzleIndexList.Count == 0)
					{
						// If out of indexes, refill
						for (int z = 0; z < m_MuzzlePoints.Length; z++)
						{
							m_MuzzleIndexList.Add(z);
						}
					}

					// Get random index from list of unused indexes
					int a = Random.Range(0, m_MuzzleIndexList.Count);
					int b = m_MuzzleIndexList[a];
					m_MuzzleIndexList.RemoveAt(a);
					return m_MuzzlePoints[b];

				default: // First Only ////////////////////////////////////////////////////
					return m_MuzzlePoints[0];
			}
		}

		private void OnDrawGizmos()
		{
#if UNITY_EDITOR
			if (m_Data == null)
			{
				return;
			}

			foreach (Transform m in m_MuzzlePoints)
			{
				if (m == null)
				{
					continue;
				}
				(m_Spread == null ? m_Data.Spread : m_Spread).DrawGizmos(transform, m);
			}
#endif
		}

		#region Helpers
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected void Log(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.Log(pMessage, this, pMethodName);
		[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
		protected void LogError(string pMessage, [CallerMemberName] string pMethodName = "") => Util.Debug.LogError(pMessage, this, pMethodName);
		#endregion
	}
}