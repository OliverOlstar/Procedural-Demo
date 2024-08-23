using UnityEngine;
using System.Collections.Generic;

namespace Core
{
	public static class MeshCombiner
	{
		static readonly int MAX_VERTS = 65534;

		class MeshSkeleton
		{
			public List<Vector3> m_Vertices = new(MAX_VERTS);
			public List<Vector3> m_Normals = new(MAX_VERTS);
			public List<Vector4> m_Tangents = new(MAX_VERTS);
			public List<Color> m_VertexColors = new(MAX_VERTS);
			public List<Vector2> m_UV = new(MAX_VERTS);
			public List<Vector2> m_UV2 = new(MAX_VERTS);
			public List<int> m_Triangles = new(MAX_VERTS);

			public static MeshSkeleton FromSubMeshes(Transform transform, Mesh mesh, List<int> submeshs, Vector4 lightmapScaleOffset)
			{
				if (transform.lossyScale.x < 0.0f || transform.lossyScale.y < 0.0f || transform.lossyScale.z < 0.0f)
				{
					Debug.LogError("MeshCombiner.MeshSkeleton.FromSubMeshes: Transform " + DebugUtil.GetScenePath(transform.gameObject)
						+ " has a negative scale which will interfere with mesh combining.", transform.gameObject);
				}

				List<int> tris = new(MAX_VERTS);
				foreach (int submesh in submeshs)
				{
					tris.AddRange(mesh.GetTriangles(submesh));
				}
				int[] vertIndexAdjuster = new int[mesh.vertices.Length];
				for (int i = 0; i < vertIndexAdjuster.Length; i++)
				{
					vertIndexAdjuster[i] = -1;
				}

				if (mesh.uv.Length != mesh.vertices.Length)
				{
					Debug.LogWarning("MeshCombiner.MeshSkeleton.FromSubMesh: Mesh " + mesh.name + " on " + DebugUtil.GetScenePath(transform.gameObject)
						+ " has " + mesh.vertices.Length + " verts and " + mesh.uv.Length + " uvs", transform.gameObject);
				}
				if (mesh.uv.Length != mesh.normals.Length)
				{
					Debug.LogWarning("MeshCombiner.MeshSkeleton.FromSubMesh: Mesh " + mesh.name + " on " + DebugUtil.GetScenePath(transform.gameObject)
						+ " has " + mesh.vertices.Length + " verts and " + mesh.normals.Length + " normals", transform.gameObject);
				}
				if (mesh.uv.Length != mesh.tangents.Length)
				{
					Debug.LogWarning("MeshCombiner.MeshSkeleton.FromSubMesh: Mesh " + mesh.name + " on " + DebugUtil.GetScenePath(transform.gameObject)
						+ " has " + mesh.vertices.Length + " verts and " + mesh.tangents.Length + " tangents", transform.gameObject);
				}

				List<int> verts = new();
				MeshSkeleton meshSkeleton = new();
				for (int i = 0; i < tris.Count; i++)
				{
					int vert = tris[i];
					
					if (!verts.Contains(vert))
					{
						verts.Add(vert);
					}

					if (vertIndexAdjuster[vert] >= 0)
					{
						meshSkeleton.m_Triangles.Add(vertIndexAdjuster[vert]);
						continue;
					}

					vertIndexAdjuster[vert] = meshSkeleton.m_Vertices.Count;
					meshSkeleton.m_Triangles.Add(vertIndexAdjuster[vert]);
					meshSkeleton.m_Vertices.Add(transform.localToWorldMatrix.MultiplyPoint(mesh.vertices[vert]));

					if (vert < mesh.normals.Length)
					{
						meshSkeleton.m_Normals.Add(
							transform.localToWorldMatrix.MultiplyVector(mesh.normals[vert]));
					}
					else
					{
						meshSkeleton.m_Normals.Add(Vector3.up);
					}

					if (vert < mesh.tangents.Length)
					{
						Vector4 tangent = transform.localToWorldMatrix.MultiplyVector(mesh.tangents[vert]);
						tangent.w = mesh.tangents[vert].w;
						meshSkeleton.m_Tangents.Add(tangent);
					}
					else
					{
						meshSkeleton.m_Tangents.Add(Vector4.zero);
					}

					if (vert < mesh.colors.Length)
					{
						meshSkeleton.m_VertexColors.Add(mesh.colors[vert]);
					}
					else
					{
						meshSkeleton.m_VertexColors.Add(Color.white);
					}

					if (vert < mesh.uv.Length)
					{
						meshSkeleton.m_UV.Add(mesh.uv[vert]);
					}
					else
					{
						meshSkeleton.m_UV.Add(Vector2.zero);
					}

					if (vert < mesh.uv2.Length)
					{
						meshSkeleton.m_UV2.Add(new Vector2(mesh.uv2[vert].x * lightmapScaleOffset.x, mesh.uv2[vert].y * lightmapScaleOffset.y)
							+ new Vector2(lightmapScaleOffset.z, lightmapScaleOffset.w));
					}
					else
					{
						meshSkeleton.m_UV2.Add(Vector2.zero);
					}
				}

				return meshSkeleton;
			}

			public Mesh GetMesh()
			{
				Mesh mesh = new();
				mesh.vertices = m_Vertices.ToArray();
				mesh.colors = m_VertexColors.ToArray();
				mesh.normals = m_Normals.ToArray();
				mesh.tangents = m_Tangents.ToArray();
				mesh.uv = m_UV.ToArray();
				mesh.uv2 = m_UV2.ToArray();
				mesh.triangles = m_Triangles.ToArray();
				mesh.subMeshCount = 1;
				return mesh;
			}
		}

		class MeshAccumulator
		{
			List<MeshSkeleton> m_AccumulatedMeshSkeletons = new();
			public Material m_Material = null;
			public int m_LightmapIndex = -1;

			public MeshAccumulator(Material material, int lightmapIndex)
			{
				m_AccumulatedMeshSkeletons.Add(new MeshSkeleton());
				m_Material = material;
				m_LightmapIndex = lightmapIndex;
			}

			public bool CanAddMesh(MeshSkeleton meshSkeleton, Material material, int lightmapIndex, bool compareMaterialName)
			{
				if (!compareMaterialName)
				{
					return lightmapIndex == m_LightmapIndex
						&& material.GetInstanceID() == m_Material.GetInstanceID();
				}
				return lightmapIndex == m_LightmapIndex
					&& (material.GetInstanceID() == m_Material.GetInstanceID()
					|| material.name == m_Material.name);
			}

			public void Accumulate(MeshSkeleton meshSkeleton)
			{
				// find a skeleton with space.
				MeshSkeleton accMeshSkeletion = null;
				for (int i = 0; i < m_AccumulatedMeshSkeletons.Count; i++)
				{
					if (m_AccumulatedMeshSkeletons[i].m_Vertices.Count + meshSkeleton.m_Vertices.Count < MAX_VERTS)
					{
						accMeshSkeletion = m_AccumulatedMeshSkeletons[i];
						break;
					}
				}
				// if ones does not exist make it.
				if (accMeshSkeletion == null)
				{
					accMeshSkeletion = new MeshSkeleton();
					m_AccumulatedMeshSkeletons.Add(accMeshSkeletion);
				}

				int removedVertices = 0;
				int removedTriangles = 0;

				int[] vertexMap = new int[meshSkeleton.m_Vertices.Count];
				for (int i = 0; i < meshSkeleton.m_Vertices.Count; i++)
				{
					bool duplicate = false;
					for (int j = 0; j < accMeshSkeletion.m_Vertices.Count; j++)
					{
						if (Util.VectorEquals(meshSkeleton.m_Vertices[i], accMeshSkeletion.m_Vertices[j])
							&& Util.ColorEquals(meshSkeleton.m_VertexColors[i], accMeshSkeletion.m_VertexColors[j])
							&& Util.VectorEquals(meshSkeleton.m_Normals[i], accMeshSkeletion.m_Normals[j])
							&& Util.VectorEquals(meshSkeleton.m_Tangents[i], accMeshSkeletion.m_Tangents[j])
							&& Util.VectorEquals(meshSkeleton.m_UV[i], accMeshSkeletion.m_UV[j])
							&& Util.VectorEquals(meshSkeleton.m_UV2[i], accMeshSkeletion.m_UV2[j]))
						{
							vertexMap[i] = j;
							duplicate = true;
							break;
						}
					}
					if (duplicate)
					{
						removedVertices++;
						continue;
					}
					vertexMap[i] = accMeshSkeletion.m_Vertices.Count;
					accMeshSkeletion.m_Vertices.Add(meshSkeleton.m_Vertices[i]);
					accMeshSkeletion.m_VertexColors.Add(meshSkeleton.m_VertexColors[i]);
					accMeshSkeletion.m_Normals.Add(meshSkeleton.m_Normals[i]);
					accMeshSkeletion.m_Tangents.Add(meshSkeleton.m_Tangents[i]);
					accMeshSkeletion.m_UV.Add(meshSkeleton.m_UV[i]);
					accMeshSkeletion.m_UV2.Add(meshSkeleton.m_UV2[i]);
				}

				// must offset all the vert indices as the verts are being added to the end of the mesh.
				for (int i = 0; i < meshSkeleton.m_Triangles.Count; i += 3)
				{
					int triangleVertex1 = vertexMap[meshSkeleton.m_Triangles[i]];
					int triangleVertex2 = vertexMap[meshSkeleton.m_Triangles[i + 1]];
					int triangleVertex3 = vertexMap[meshSkeleton.m_Triangles[i + 2]];
					bool duplicate = false;
					for (int j = 0; j < accMeshSkeletion.m_Triangles.Count; j += 3)
					{
						if (accMeshSkeletion.m_Triangles[j] == triangleVertex1
							&& accMeshSkeletion.m_Triangles[j + 1] == triangleVertex2
							&& accMeshSkeletion.m_Triangles[j + 2] == triangleVertex3)
						{
							duplicate = true;
							break;
						}
					}
					if (duplicate)
					{
						removedTriangles++;
						continue;
					}
					accMeshSkeletion.m_Triangles.Add(triangleVertex1);
					accMeshSkeletion.m_Triangles.Add(triangleVertex2);
					accMeshSkeletion.m_Triangles.Add(triangleVertex3);
				}
				Debug.Log("MeshAccumulator.Accumulate: Optimization removed " + removedVertices + " vertices " + removedTriangles + " triangles.");
			}

			public string GetName(Transform parent)
			{
				return string.Format("{0}_Material({1})_LMI({2})",
						parent.name,
						m_Material.name,
						m_LightmapIndex);
			}

			public Mesh[] GetMeshes(string name)
			{
				Mesh [] meshes = new Mesh[m_AccumulatedMeshSkeletons.Count];
				for (int i = 0; i < m_AccumulatedMeshSkeletons.Count; i++)
				{
					meshes[i] = m_AccumulatedMeshSkeletons[i].GetMesh();
					meshes[i].name = m_AccumulatedMeshSkeletons.Count > 1 ? name + " " + i : name;
				}
				return meshes;
			}
		}

		public static List<Mesh> CombineAllChildMeshes(Transform parentTransform, bool compareMaterialName = false)
		{
			bool hasCollider = false;
			MeshRenderer[] renderers = parentTransform.GetComponentsInChildren<MeshRenderer>();
			List<MeshAccumulator> meshAccumulators = new(renderers.Length);

			for (int i = 0; i < renderers.Length; i++)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					float percent = (float)i / (float)(renderers.Length - 1.0f);
					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
						"Combining meshes", + (i+1) + "/" + renderers.Length, percent))
					{
						UnityEditor.EditorUtility.ClearProgressBar();
						return new List<Mesh>();
					}
				}
#endif

				MeshRenderer renderer = renderers[i];
				if (renderer.isPartOfStaticBatch)
				{
					Debug.LogError("MeshAccumulator.CombineAllChildMeshes() Can't combine " +  DebugUtil.GetScenePath(renderer.gameObject) + " because it's part of static batch", renderer.gameObject);
					continue;
				}
				if (!renderer.transform.TryGetComponent<MeshFilter>(out MeshFilter filter))
				{
					continue;
				}

				Mesh mesh = filter.sharedMesh;
				if (mesh == null)
				{
					continue;
				}

				if (renderer.GetComponent<MeshCollider>() != null)
				{
					hasCollider = true;
				}

				meshAccumulators = AccumulateAllSubmeshes(renderer, mesh, meshAccumulators, compareMaterialName);

				if (Application.isPlaying)
				{
					Object.Destroy(renderer.gameObject);
				}
			}

#if UNITY_EDITOR
			UnityEditor.EditorUtility.ClearProgressBar();
#endif

			return CreateCombinedMeshes(parentTransform, meshAccumulators, hasCollider);
		}

		[System.Obsolete()]
		public static Mesh SeperateSubMesh(Mesh mesh, MeshRenderer renderer, int index)
		{
			int[] triangles = mesh.GetTriangles(index);
			Mesh newMesh = new();
			newMesh.Clear();

			List<Vector3> verts = new();
			List<Vector3> norms = new();
			List<Vector2> uvs = new();
			List<Vector2> uv2s = new();
			List<Color> colors = new();

			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				bool vertIsInSubMesh = false;
				for (int j = 0; j < triangles.Length; j++)
				{
					// if a triangle in the submesh references this vert at index i.
					if (triangles[j] == i)
					{
						// make sure this ver is added.
						vertIsInSubMesh = true;
						// adjust the triangles new reference.
						triangles[j] = verts.Count;
					}
				}

				if (vertIsInSubMesh)
				{
					if (i < mesh.vertices.Length) { verts.Add(mesh.vertices[i]); }

					if (i < mesh.normals.Length) { norms.Add(mesh.normals[i]); }

					if (i < mesh.colors.Length) { colors.Add(mesh.colors[i]); }

					if (i < mesh.uv.Length) { uvs.Add(mesh.uv[i]); }

					if (i < mesh.uv2.Length) { uv2s.Add(mesh.uv2[i]); }
				}
			}

			newMesh.vertices = verts.ToArray();
			newMesh.normals = norms.ToArray();
			newMesh.colors = colors.ToArray();
			newMesh.uv = uvs.ToArray();

			Vector2[] lightmapuvs = new Vector2[uv2s.Count];
			// write lightmap adjusted uv2s into the new mesh.
			for (int i = 0; i < lightmapuvs.Length; i++)
			{
				float x = (uv2s[i].x * renderer.lightmapScaleOffset.x) + renderer.lightmapScaleOffset.z;
				float y = (uv2s[i].y * renderer.lightmapScaleOffset.y) + renderer.lightmapScaleOffset.w;
				lightmapuvs[i] = new Vector2(x, y);
			}
			newMesh.uv2 = lightmapuvs;

			newMesh.triangles = triangles;
			newMesh.subMeshCount = 1;
			//UnityEditor.AssetDatabase.CreateAsset(newMesh, "Assets/" + mesh.name + "_submesh[" + index + "].asset");
			return newMesh;
		}

		[System.Obsolete()]
		public static Mesh CombineSubMeshes(CombineInstance[] meshes)
		{
			Mesh newMesh = new();
			newMesh.Clear();
			List<int> tris = new();
			List<Vector3> verts = new();
			List<Vector3> norms = new();
			List<Vector2> uvs = new();
			List<Vector2> uv2s = new();
			List<Color> colors = new();
			List<int>[] subMeshes = new List<int>[meshes.Length];

			for (int i = 0; i < meshes.Length; i++)
			{
				subMeshes[i] = new List<int>();
				for (int j = 0; j < meshes[i].mesh.triangles.Length; j++)
				{
					subMeshes[i].Add(meshes[i].mesh.triangles[j] + verts.Count);
				}

				for (int j = 0; j < meshes[i].mesh.vertices.Length; j++)
				{
					verts.Add(meshes[i].transform.MultiplyPoint(meshes[i].mesh.vertices[j]));
				}

				norms.AddRange(meshes[i].mesh.normals);
				uvs.AddRange(meshes[i].mesh.uv);
				uv2s.AddRange(meshes[i].mesh.uv2);
				colors.AddRange(meshes[i].mesh.colors);
				tris.AddRange(meshes[i].mesh.triangles);
			}
			newMesh.vertices = verts.ToArray();
			newMesh.normals = norms.ToArray();
			newMesh.uv = uvs.ToArray();
			newMesh.uv2 = uv2s.ToArray();
			newMesh.colors = colors.ToArray();
			newMesh.triangles = tris.ToArray();

			for (int i = 0; i < subMeshes.Length; i++)
			{
				newMesh.subMeshCount++;
				newMesh.SetTriangles(subMeshes[i].ToArray(), i);
			}

			return newMesh;
		}

		static List<MeshAccumulator> AccumulateAllSubmeshes(
			MeshRenderer renderer,
			Mesh mesh,
			List<MeshAccumulator> meshAccumulators,
			bool compareMaterialName)
		{
			Transform transform = renderer.transform;
			int lightmapIndex = renderer.lightmapIndex;
			Vector4 lightmapScaleOffset = renderer.lightmapScaleOffset;

			List<int> accumulatedSubmeshes = new(mesh.subMeshCount);

			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				if (accumulatedSubmeshes.Contains(j))
				{
					// already accumulated this submesh earlier.
					continue;
				}

				Material material = renderer.sharedMaterials[j];
				if (material == null)
				{
					Debug.LogError("MeshCombiner.AccumulateAllSubmeshes: GameObject " + DebugUtil.GetScenePath(renderer.gameObject) + " material at index " + j + " is null.", renderer.gameObject);
					continue;
				}
				List<int> matchingSubmeshes = new() { j };
				for (int k = j + 1; k < mesh.subMeshCount; k++)
				{
					if (k < renderer.sharedMaterials.Length)
					{
						if (renderer.sharedMaterials[k] == material)
						{
							matchingSubmeshes.Add(k);
							accumulatedSubmeshes.Add(k);
						}
					}
					else
					{
						Debug.LogError("MeshCombiner.AccumulateAllSubmeshes: GameObject " + DebugUtil.GetScenePath(renderer.gameObject) + " has more submeshes than is does materials.", renderer.gameObject);
						break;
					}
				}

				MeshSkeleton subMeshSkeleton = MeshSkeleton.FromSubMeshes(transform, mesh, matchingSubmeshes, lightmapScaleOffset);
				if (subMeshSkeleton == null)
				{
					continue;
				}

				// find the accumulator that matches the submesh.
				MeshAccumulator matchingAccumulator = null;
				for (int k = 0; k < meshAccumulators.Count; k++)
				{
					if (meshAccumulators[k].CanAddMesh(subMeshSkeleton, material, lightmapIndex, compareMaterialName))
					{
						matchingAccumulator = meshAccumulators[k];
						break;
					}
				}
				// if one does not exist make it.
				if (matchingAccumulator == null)
				{
					matchingAccumulator = new MeshAccumulator(material, lightmapIndex);
					meshAccumulators.Add(matchingAccumulator);
				}

				matchingAccumulator.Accumulate(subMeshSkeleton);
			}
			return meshAccumulators;
		}

		static List<Mesh> CreateCombinedMeshes(Transform target, List<MeshAccumulator> meshAccumulators, bool addCollider)
		{
			Transform combinedMeshes = new GameObject(target.name + "(Combined)").transform;
			combinedMeshes.transform.parent = target.parent;
			List<Mesh> meshes = new(meshAccumulators.Count);
			for (int i = 0; i < meshAccumulators.Count; i++)
			{
				string name = meshAccumulators[i].GetName(target);
				Mesh[] accumulatedMeshes = meshAccumulators[i].GetMeshes(name);
				for (int j = 0; j < accumulatedMeshes.Length; j++)
				{
					GameObject newObject = new(accumulatedMeshes.Length > 1 ? name + " " + j : name);
					newObject.transform.parent = combinedMeshes;
					MeshFilter filter = newObject.AddComponent<MeshFilter>();
					MeshRenderer renderer = newObject.AddComponent<MeshRenderer>();

					filter.sharedMesh = accumulatedMeshes[j];
					renderer.material = meshAccumulators[i].m_Material;
					renderer.lightmapIndex = meshAccumulators[i].m_LightmapIndex;
					renderer.lightmapScaleOffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);

					RendererInfoStorage rendererInfo = newObject.AddComponent<RendererInfoStorage>();
					rendererInfo.m_LightmapIndex = meshAccumulators[i].m_LightmapIndex;
					rendererInfo.m_LightmapScaleOffset = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);

					if (addCollider)
					{
						newObject.AddComponent<MeshCollider>();
					}
					meshes.Add(accumulatedMeshes[j]);
				}
			}
			return meshes;
		}
	}
}
