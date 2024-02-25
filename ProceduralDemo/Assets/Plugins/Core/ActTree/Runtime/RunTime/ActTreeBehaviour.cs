using UnityEngine;

public class ActTreeBehaviour : MonoBehaviour, IActSequenceControllerOwner
{
	[SerializeField]
	private ActTree m_Tree = null;
	public ActTree GetSourceTree() { return m_Tree; }
	[SerializeField]
	private ActTree[] m_SecondaryTrees = { };
	[SerializeField]
	private string m_StartNode = Core.Str.EMPTY;
	public string StartNode => m_StartNode;
	[SerializeField]
	private ActSequenceController.UpdateMode m_UpdateMode = ActSequenceController.UpdateMode.GameTime;

	protected ActSequenceController m_Controller = null;
	public ActSequenceController Controller => m_Controller;
	[System.Obsolete("Use property instead")]
	public ActSequenceController GetController() { return m_Controller; }

	private bool m_SafeDestroy = false;
	public bool GetDestroy() { return m_SafeDestroy; }

	public virtual ActParams CreateParams()
	{
		return new GOParams(gameObject);
	}

	public void SetStartNode(int id, ActParams scriptedParams = null)
	{
		m_Controller.SetStartNode(id, scriptedParams);
	}
	public void SetStartNodeName(string nodeName, ActParams scriptedParams = null)
	{
		m_Controller.SetStartNodeName(nodeName, scriptedParams);
	}
	public void PlayNode(string nodeName, ActParams scriptedParams = null)
	{
		m_Controller.PlayNode(nodeName, scriptedParams);
	}
	public void PlayNode(int id, ActParams scriptedParams = null)
	{
		m_Controller.PlayNode(id, scriptedParams);
	}

	public void Reset()
	{
		m_Controller.Reset();
	}

	public void SendParams(ActParams stateParams)
	{
		m_Controller.SendParams(stateParams);
	}

	protected virtual void Awake()
	{
		m_Controller = new ActSequenceController(m_Tree, m_SecondaryTrees, this, m_UpdateMode);
		m_Controller.SetStartNodeName(m_StartNode);
	}

	// We want to initialize the trees after Awake() has been called
	protected virtual void Start()
	{
		m_Controller.Start();
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
		m_Controller.Destroy();
		m_Controller = null;
	}

	void OnDisable()
	{
		if (!m_SafeDestroy)
		{
			Debug.LogWarning(this.GetType().Name + ".OnDisable() Avoid disabling this behaviour");
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

	public void SetUpdateMode(ActSequenceController.UpdateMode updateMode)
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
			Destroy(gameObject);
		}
	}

	public void SafeDestroy()
	{
		m_SafeDestroy = true;
	}

	public static void SafeDestroy(GameObject obj)
	{
		ActTreeBehaviour[] controllers = obj.GetComponents<ActTreeBehaviour>();
		if (controllers.Length > 0)
		{
			foreach (ActTreeBehaviour controller in controllers)
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
