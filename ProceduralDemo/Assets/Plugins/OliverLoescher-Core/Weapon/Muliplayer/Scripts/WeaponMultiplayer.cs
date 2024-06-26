using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Sirenix.OdinInspector;

namespace OCore.Weapon.Multiplayer
{
	[RequireComponent(typeof(PhotonView))]
	public class WeaponMultiplayer : Weapon
	{
		private PhotonView m_PhotonView = null;
		private Dictionary<int, ProjectileMultiplayer> m_ActiveProjectiles = new Dictionary<int, ProjectileMultiplayer>();

		public bool IsValid => PhotonNetwork.IsConnected && m_PhotonView.IsMine;

		protected override void Init()
		{
			m_PhotonView = GetComponent<PhotonView>();
		}

		#region Self
		protected override Projectile SpawnProjectile(Vector3 pPoint, Vector3 pForce)
		{
			Projectile projectile = base.SpawnProjectile(pPoint, pForce);
			int id = projectile.gameObject.GetInstanceID();
			if (projectile is ProjectileMultiplayer projectileMultiplayer)
			{
				projectileMultiplayer.InitMultiplayer(id, this, true);
			}

			if (IsValid)
			{
				m_PhotonView.RPC(nameof(RPC_ShootProjectile), RpcTarget.Others, id, pPoint, pForce);
			}
			return projectile;
		}

		protected override void SpawnRaycast(Vector3 pPoint, Vector3 pForward)
		{
			base.SpawnRaycast(pPoint, pForward);
			if (IsValid)
			{
				m_PhotonView.RPC(nameof(RPC_ShootRaycast), RpcTarget.Others, pPoint, pForward);
			}
		}

		protected override void OnShootFailed()
		{
			base.OnShootFailed();
			if (IsValid)
			{
				m_PhotonView.RPC(nameof(RPC_ShootFailed), RpcTarget.Others);
			}
		}

		public void Projectile_DoLifeEnd(int pID, Vector3 pPosition)
		{
			if (IsValid)
			{
				m_PhotonView.RPC(nameof(RPC_Projectile_DoLifeEnd), RpcTarget.Others, pID, pPosition);
			}
		}

		public void Projectile_DoCollision(int pID, Vector3 pPosition, bool pDidDamage)
		{
			if (IsValid)
			{
				m_PhotonView.RPC(nameof(RPC_Projectile_DoCollision), RpcTarget.Others, pID, pPosition, pDidDamage);
			}
		}
		#endregion Self

		#region Other
		[PunRPC]
		public void RPC_ShootFailed()
		{
			base.OnShootFailed();
		}

		[PunRPC]
		public void RPC_ShootProjectile(int pID, Vector3 pPoint, Vector3 pForce)
		{
			Projectile projectile = base.SpawnProjectile(pPoint, pForce);
			if (!(projectile is ProjectileMultiplayer projectileMultiplayer))
			{
				LogError("ProjectilePrefab has Projectile.cs when it should have a ProjectileMultiplayer.cs, this will not work correctly.");
				return;
			}
			projectileMultiplayer.InitMultiplayer(pID, this, false);
			m_ActiveProjectiles.Add(pID, projectileMultiplayer);
		}

		[PunRPC]
		public void RPC_ShootRaycast(Vector3 pPoint, Vector3 pForward)
		{
			base.SpawnRaycast(pPoint, pForward);
		}

		[PunRPC]
		public void RPC_Projectile_DoLifeEnd(int pID, Vector3 pPosition)
		{
			if (Util.TryGetAndRemove(ref m_ActiveProjectiles, pID, out ProjectileMultiplayer projectile))
			{
				projectile.transform.position = pPosition;
				projectile.ForceLifeEnd();
			}
		}

		[PunRPC]
		public void RPC_Projectile_DoCollision(int pID, Vector3 pPosition, bool pDidDamage)
		{
			if (!m_ActiveProjectiles.TryGetValue(pID, out ProjectileMultiplayer projectile))
			{
				return;
			}
			projectile.transform.position = pPosition;
			if (projectile.ForceCollision(pDidDamage))
			{
				m_ActiveProjectiles.Remove(pID);
			}
		}
		#endregion
	}
}