using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Util
{
    public static class Mesh
    {
		private static Dictionary<PrimitiveType, UnityEngine.Mesh> primitiveMeshes = new Dictionary<PrimitiveType, UnityEngine.Mesh>();

		public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider)
		{
			if (withCollider) { return GameObject.CreatePrimitive(type); }

			GameObject gameObject = new GameObject(type.ToString());
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = GetPrimitiveMesh(type);
			gameObject.AddComponent<MeshRenderer>();

			return gameObject;
		}

		public static UnityEngine.Mesh GetPrimitiveMesh(PrimitiveType type)
		{
			if (!primitiveMeshes.ContainsKey(type))
			{
				CreatePrimitiveMesh(type);
			}

			return primitiveMeshes[type];
		}

		private static UnityEngine.Mesh CreatePrimitiveMesh(PrimitiveType type)
		{
			GameObject gameObject = GameObject.CreatePrimitive(type);
			UnityEngine.Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			GameObject.DestroyImmediate(gameObject);

			primitiveMeshes[type] = mesh;
			return mesh;
		}
	}
}
