

using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public interface IGOContext : ITreeContext
	{
		GameObject GameObject { get; }
		Transform Transform { get; }
	}

	public class GOContext : IGOContext
	{
		GameObject m_GameObject = null;
		Transform m_Transform = null;

		public GameObject GameObject => m_GameObject;
		public Transform Transform => m_Transform;

		public GOContext(GameObject gameObject)
		{
			m_GameObject = gameObject ?? throw new System.ArgumentNullException("GOContext() GameObjact cannot be null");
			m_Transform = m_GameObject.transform;
		}

		public override string ToString() => $"{GetType().Name}({(m_GameObject == null ? Core.Str.EMPTY : m_GameObject.name)})";
	}

	//public class GOParams : Params
	//{
	//	GameObject m_GameObject = null;
	//	Transform m_Transform = null;

	//	public GameObject GameObject => m_GameObject;
	//	public Transform Transform => m_Transform;

	//	public GOParams(GameObject gameObject)
	//	{
	//		m_GameObject = gameObject ?? throw new System.ArgumentNullException("GOParams() GameObjact cannot be null");
	//		m_Transform = m_GameObject.transform;
	//	}

	//	public override string ToString() { return "GOParams(" + (m_GameObject == null ? Core.Str.EMPTY : m_GameObject.name) + ")"; }
	//}
}
