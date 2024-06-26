using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OCore
{
	public class ObjectPoolDictionary : MonoBehaviour
	{
		[System.Serializable]
		public class PoolValues
		{
			[AssetsOnly]
			public GameObject Prefab = null;
			public string Key => Prefab.name;

			[Tooltip("What returns when all items are already checked out")]
			public PoolReturnType ReturnType = PoolReturnType.Expand;
			[HideInPlayMode]
			public int StartingCopies = 5;
		}

		private class Pool
		{
			private readonly PoolValues m_Values = null;
			private readonly List<PoolItem> m_ItemsIn = new();
			private readonly List<PoolItem> m_ItemsOut = new();

			private readonly Transform m_Transform = null;

			public Pool(PoolValues pValues, Transform pTransform)
			{
				m_Values = pValues;
				m_Transform = pTransform;

				while (m_ItemsIn.Count < m_Values.StartingCopies)
				{
					m_ItemsIn.Add(InstiateNewObject());
				}
			}

			private PoolItem InstiateNewObject()
			{
				PoolItem item = new()
				{
					gameObject = Instantiate(m_Values.Prefab, m_Transform),
					parent = m_Transform
				};

				item.element = item.gameObject.GetComponentInChildren<PoolElement>();
				if (item.element != null)
				{
					item.gameObject = item.element.gameObject;
					item.element.Init(m_Values.Prefab.name, item.parent);
					item.gameObject.name = m_Values.Prefab.name;
				}

				item.gameObject.SetActive(false);
				return item;
			}

			public GameObject CheckOutObject(bool pEnable = false)
			{
				PoolItem item;
				// Grab Next
				if (m_ItemsIn.Count > 0)
				{
					item = m_ItemsIn[0];
					m_ItemsOut.Add(item);
					m_ItemsIn.RemoveAt(0);
				}
				// Expand
				else if (m_Values.ReturnType == PoolReturnType.Expand)
				{
					item = InstiateNewObject();
				}
				// Grab first out
				else if (m_Values.ReturnType == PoolReturnType.Loop)
				{
					if (m_ItemsOut.Count > 0)
					{
						// Return first to pool, then take it back out
						item = m_ItemsOut[0];
						item.element?.ReturnToPool();
						m_ItemsIn.Remove(item);
					}
					else
					{
						// If none out create new
						item = InstiateNewObject();
					}
					m_ItemsOut.Add(item);
				}
				else // (returnType == PoolReturnType.Null)
				{
					return null;
				}

				item.gameObject.SetActive(pEnable);
				if (item.element != null)
				{
					item.element.OnExitPool();
				}
				return item.gameObject;
			}

			public void CheckInObject(PoolElement pElement, bool pDisable = true)
			{
				pElement.gameObject.transform.SetParent(pElement.Parent, true);
				pElement.gameObject.transform.localScale = Util.Math.Inverse(pElement.Parent.lossyScale);
				if (pElement.gameObject.activeSelf)
				{
					pElement.gameObject.SetActive(!pDisable);
				}

				PoolItem item = new()
				{
					gameObject = pElement.gameObject,
					element = pElement,
					parent = pElement.Parent
				};

				m_ItemsOut.Remove(item);
				m_ItemsIn.Add(item);
			}

			public void ObjectDestroyed(GameObject pObject, PoolElement pElement)
			{
				PoolItem item = new()
				{
					gameObject = pObject,
					element = pElement
				};

				if (!m_ItemsIn.Remove(item))
				{
					m_ItemsOut.Remove(item);
				}
			}

			public void OnDestroy()
			{

			}
		}

		public struct PoolItem
		{
			public Transform parent;
			public GameObject gameObject;
			public PoolElement element;
		}

		public enum PoolReturnType
		{
			Expand,
			Loop,
			Null
		}

		#region Singleton
		public static ObjectPoolDictionary s_Instance = null;

		public static ObjectPoolDictionary Instance
		{
			get
			{
				if (s_Instance != null)
				{
					return s_Instance;
				}
				GameObject gameObject = new("ObjectPoolDictionary Container");
				s_Instance = gameObject.AddComponent<ObjectPoolDictionary>();
				return s_Instance;
			}
		}
		#endregion

		private static readonly Dictionary<string, Pool> s_Dictionary = new();

		public static GameObject Get(GameObject pObject)
		{
			if (pObject == null)
			{
				return null;
			}
			if (!s_Dictionary.ContainsKey(pObject.name))
			{
				PoolValues values = new()
				{
					Prefab = pObject
				};
				return Instance.CreatePool(values).CheckOutObject();
			}
			return s_Dictionary[pObject.name].CheckOutObject();
		}
		public static GameObject Get(PoolElement pElement)
		{
			if (pElement == null)
			{
				return null;
			}
			if (!s_Dictionary.ContainsKey(pElement.PoolKey))
			{
				PoolValues values = new()
				{
					Prefab = pElement.gameObject
				};
				return Instance.CreatePool(values).CheckOutObject();
			}
			return s_Dictionary[pElement.PoolKey].CheckOutObject();
		}
		public static GameObject Get(PoolValues pValues)
		{
			if (pValues.Prefab == null)
			{
				return null;
			}
			if (!s_Dictionary.ContainsKey(pValues.Key))
			{
				return Instance.CreatePool(pValues).CheckOutObject();
			}
			return s_Dictionary[pValues.Key].CheckOutObject();
		}

		public static GameObject Play(GameObject pObject, Vector3 pPosition, Quaternion pRotation, Transform pParent = null) => InternalPlay(Get(pObject), pPosition, pRotation, pParent);
		public static GameObject Play(PoolElement pElement, Vector3 pPosition, Quaternion pRotation, Transform pParent = null) => InternalPlay(Get(pElement), pPosition, pRotation, pParent);
		public static GameObject Play(PoolValues pValues, Vector3 pPosition, Quaternion pRotation, Transform pParent = null) => InternalPlay(Get(pValues), pPosition, pRotation, pParent);
		private static GameObject InternalPlay(GameObject pPoolObject, Vector3 pPosition, Quaternion pRotation, Transform pParent = null)
		{
			pPoolObject.transform.SetParent(pParent);
			pPoolObject.transform.SetPositionAndRotation(pPosition, pRotation);
			pPoolObject.SetActive(true);
			return pPoolObject;
		}

		public static void Return(PoolElement pElement, bool pDisable = true)
		{
			s_Dictionary[pElement.PoolKey].CheckInObject(pElement, pDisable);
		}

		public static void ObjectDestroyed(GameObject pObject, PoolElement pElement)
		{
			if (s_Dictionary.ContainsKey(pObject.name))
			{
				s_Dictionary[pObject.name].ObjectDestroyed(pObject, pElement);
			}
		}

		private Pool CreatePool(PoolValues pValues)
		{
			Transform poolT = new GameObject("Pool " + pValues.Key).transform;
			poolT.SetParent(transform);
			Pool pool = new(pValues, poolT);

			s_Dictionary.Add(pValues.Key, pool);
			return pool;
		}

		private void OnDestroy()
		{
			foreach (Pool p in s_Dictionary.Values)
			{
				p.OnDestroy();
			}
			s_Dictionary.Clear();
		}
	}
}