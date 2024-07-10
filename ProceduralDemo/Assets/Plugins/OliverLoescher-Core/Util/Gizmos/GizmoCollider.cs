using OCore.Debug2;
using UnityEngine;

namespace OCore
{
	[RequireComponent(typeof(Collider))]
	public class GizmoCollider : GizmoBase
	{
		[SerializeField]
		private bool m_Wireframe = false;

		private Collider m_Target = null;

		protected override void DrawGizmos()
		{
			base.DrawGizmos();
			if (m_Target == null && !TryGetComponent(out m_Target))
			{
				return;
			}
			Gizmos.matrix = transform.localToWorldMatrix;
			if (m_Wireframe)
			{
				switch (m_Target)
				{
					case BoxCollider box:
						Gizmos.DrawWireCube(box.center, box.size);
						break;
					case CapsuleCollider capsule:
						Util.Debug.GizmoCapsule(capsule.center, capsule.radius, capsule.height);
						break;
					case SphereCollider sphere:
						Gizmos.DrawWireSphere(sphere.center, sphere.radius);
						break;
					case MeshCollider mesh:
						Gizmos.DrawWireMesh(mesh.sharedMesh);
						break;
				}
			}
			else
			{
				switch (m_Target)
				{
					case BoxCollider box:
						Gizmos.DrawCube(box.center, box.size);
						break;
					case CapsuleCollider capsule:
						Gizmos.DrawMesh(Util.Mesh.GetPrimitiveMesh(PrimitiveType.Capsule), capsule.center, Quaternion.identity, new Vector3(capsule.radius * 2.0f, capsule.height * 0.5f, capsule.radius * 2.0f));
						break;
					case SphereCollider sphere:
						Gizmos.DrawSphere(sphere.center, sphere.radius);
						break;
					case MeshCollider mesh:
						Gizmos.DrawMesh(mesh.sharedMesh);
						break;
				}
			}
			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
