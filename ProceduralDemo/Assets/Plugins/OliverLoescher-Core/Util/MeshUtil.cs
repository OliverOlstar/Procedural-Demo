using System.Collections.Generic;
using UnityEngine;

namespace OCore.Util
{
	public static class Mesh
	{
		private static readonly Dictionary<PrimitiveType, UnityEngine.Mesh> m_PrimitiveMeshes = new();

		public static GameObject CreatePrimitive(PrimitiveType pType, bool pWithCollider)
		{
			if (pWithCollider)
			{
				return GameObject.CreatePrimitive(pType);
			}

			GameObject gameObject = new(pType.ToString());
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = GetPrimitiveMesh(pType);
			gameObject.AddComponent<MeshRenderer>();
			return gameObject;
		}

		public static UnityEngine.Mesh GetPrimitiveMesh(PrimitiveType pType)
		{
			if (!m_PrimitiveMeshes.ContainsKey(pType))
			{
				CreatePrimitiveMesh(pType);
			}
			return m_PrimitiveMeshes[pType];
		}

		private static UnityEngine.Mesh CreatePrimitiveMesh(PrimitiveType pType)
		{
			GameObject gameObject = GameObject.CreatePrimitive(pType);
			UnityEngine.Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			Object.DestroyImmediate(gameObject);

			m_PrimitiveMeshes[pType] = mesh;
			return mesh;
		}
	}
}
