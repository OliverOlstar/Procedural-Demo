using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace OCore.Weapon
{
	[RequireComponent(typeof(Weapon))]
	public class WeaponAmmo : MonoBehaviour
	{
		[FoldoutGroup("Unity Events")]
		public UnityEvent OnReload;
		[FoldoutGroup("Unity Events")]
		public UnityEvent OnStartOverHeat;
		[FoldoutGroup("Unity Events")]
		public UnityEvent OnEndOverHeat;
		[FoldoutGroup("Unity Events")]
		public UnityEvent OnOutOfAmmo;

		private Weapon m_Weapon = null;
		private int m_TotalAmmo;
		private int m_ClipAmmo;
		private Coroutine m_ChargeRoutine = null;

		private void Start()
		{
			m_Weapon = GetComponent<Weapon>();
			m_Weapon.OnShoot.AddListener(OnShoot);

			m_ClipAmmo = m_Weapon.Data.ClipAmmo;
			m_TotalAmmo = m_Weapon.Data.TotalAmmo - m_ClipAmmo;
		}

		public void OnShoot()
		{
			if (m_Weapon.Data.MyAmmoType == SOWeapon.AmmoType.Null)
			{
				return;
			}

			m_ClipAmmo = Mathf.Max(0, m_ClipAmmo - 1); // Ammo
			if (m_ClipAmmo == 0)
			{
				m_Weapon.CanShoot = false;
				m_Weapon.Data.OutOfAmmoSound.Play(transform.position); // Audio
				OnStartOverHeat.Invoke();
			}

			if (m_Weapon.Data.MyAmmoType == SOWeapon.AmmoType.Limited && m_TotalAmmo <= 0)
			{
				if (m_ClipAmmo <= 0) // If totally out of ammo
				{
					OnOutOfAmmo.Invoke(); // If out of all ammo
				}
			}
			else
			{
				if (m_ChargeRoutine != null)
				{
					StopCoroutine(m_ChargeRoutine);
				}
				m_ChargeRoutine = StartCoroutine(AmmoRoutine()); // Recharge
			}
		}

		private IEnumerator AmmoRoutine()
		{
			yield return new WaitForSeconds(Mathf.Max(0, m_Weapon.Data.ReloadDelaySeconds - m_Weapon.Data.ReloadIntervalSeconds));

			while (m_ClipAmmo < m_Weapon.Data.ClipAmmo && (m_TotalAmmo > 0 || m_Weapon.Data.MyAmmoType == SOWeapon.AmmoType.Unlimited))
			{
				yield return new WaitForSeconds(m_Weapon.Data.ReloadIntervalSeconds);

				m_ClipAmmo++; // Clip Ammo
				if (m_Weapon.Data.MyAmmoType == SOWeapon.AmmoType.Limited)
				{
					m_TotalAmmo--; // Total Ammo
				}
				m_Weapon.Data.ReloadSound.Play(transform.position); // Audio
				OnReload.Invoke();
			}

			if (!m_Weapon.CanShoot)
			{
				m_Weapon.CanShoot = true;
				m_Weapon.Data.OnReloadedSound.Play(transform.position); // Audio
				OnEndOverHeat.Invoke(); // Events
			}
		}

		public void ModifyAmmo(int pValue)
		{
			m_TotalAmmo += pValue; // Modify Ammo
			if (m_TotalAmmo > 0 && m_ClipAmmo < m_Weapon.Data.ClipAmmo) // Check for recharge
			{
				if (m_ChargeRoutine != null)
				{
					StopCoroutine(m_ChargeRoutine);
				}
				m_ChargeRoutine = StartCoroutine(AmmoRoutine());
			}
		}
	}
}