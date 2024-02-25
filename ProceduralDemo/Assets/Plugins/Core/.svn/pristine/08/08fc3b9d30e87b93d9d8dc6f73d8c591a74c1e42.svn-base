using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class ActTreeBehaviourBase2 : MonoBehaviour, ISequenceControllerOwner
	{
		[SerializeField]
		private string m_StartNode = Core.Str.EMPTY;
		public string StartNode => m_StartNode;
		[SerializeField]
		private SequenceController.UpdateMode m_UpdateMode = SequenceController.UpdateMode.GameTime;

		protected SequenceController m_Controller = null;
		public SequenceController Controller => m_Controller;

		private bool m_SafeDestroy = false;
		public bool GetDestroy() { return m_SafeDestroy; }

		public abstract ActTree2 MainTree { get; }
		public abstract IEnumerable<ActTree2> SecondaryTrees { get; }
		public abstract ITreeContext GetContext();

		public void SetStartNode(int id, Params scriptedParams = null)
		{
			m_Controller.SetStartNode(id, scriptedParams);
		}
		public void SetStartNodeName(string nodeName, Params scriptedParams = null)
		{
			m_Controller.SetStartNodeName(nodeName, scriptedParams);
		}
		public void PlayNodeName(string nodeName, Params scriptedParams = null, bool resequenceIfAlreadyPlaying = true, bool ignoreConditions = false, int priority = 0)
		{
			m_Controller.PlayNodeName(nodeName, scriptedParams, resequenceIfAlreadyPlaying, ignoreConditions, priority);
		}
		public void PlayNodeID(int id, Params scriptedParams = null, bool resequenceIfAlreadyPlaying = true, bool ignoreConditions = false, int priority = 0)
		{
			m_Controller.PlayNodeID(id, scriptedParams, resequenceIfAlreadyPlaying, ignoreConditions, priority);
		}

		public void Resequence() // Reset() is a special function name for Monobehaviours that gets called at edit time, can't use it here
		{
			m_Controller.Reset();
		}

		public void SendParams(Params stateParams)
		{
			m_Controller.SendParams(stateParams);
		}

		protected virtual void Awake()
		{
			m_Controller = new SequenceController(MainTree, SecondaryTrees, this, m_UpdateMode);
			m_Controller.SetStartNodeName(m_StartNode);
		}

		// We want to initialize the trees after Awake() has been called
		protected virtual void Start()
		{
			m_Controller.Start();
		}

		protected virtual void OnEnable()
		{
			m_Controller.Reset();
		}

		protected virtual void OnDisable()
		{
			m_Controller.Stop();
		}

		void OnApplicationQuit()
		{
			m_SafeDestroy = true;
		}

		protected virtual void OnDestroy()
		{
			if (!m_SafeDestroy)
			{
				Debug.LogWarning(this.GetType().Name + ".OnDestroy() Should use SafeDestroy to destory an object with an AnimStateController on it");
			}
		}

		public bool IsPlaying(string nodeName)
		{
			return m_Controller.IsPlaying(nodeName);
		}

		public bool IsPlaying(int nodeID)
		{
			return m_Controller.IsPlaying(nodeID);
		}

		public void SetUpdateMode(SequenceController.UpdateMode updateMode)
		{
			m_UpdateMode = updateMode;
			m_Controller.SetUpdateMode(m_UpdateMode);
		}

		// Update is called once per frame
		protected virtual void Update()
		{
			m_Controller.Update();
		}

		// Destory in late update so all updates have finished 
		// If safe destroy is called from the tree then it happens the same frame
		void LateUpdate()
		{
			if (m_SafeDestroy)
			{
				// Make sure we get our end events before destroying
				m_Controller.Stop();
			}
		}

		public void SafeDestroy()
		{
			m_SafeDestroy = true;
		}

		public static void SafeDestroy(GameObject obj)
		{
			ActTreeBehaviourBase2[] controllers = obj.GetComponents<ActTreeBehaviourBase2>();
			if (controllers.Length > 0)
			{
				foreach (ActTreeBehaviourBase2 controller in controllers)
				{
					controller.SafeDestroy();
				}
			}
			else
			{
				Destroy(obj);
			}
		}
	}
}
