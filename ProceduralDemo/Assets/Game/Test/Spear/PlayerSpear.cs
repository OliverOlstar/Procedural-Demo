using ODev.Util;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class PlayerSpear : MonoBehaviour
{
	public enum State
	{
		Stored,
		Thrown,
		Landed,
		Pulling
	}

	public UnityEvent<Collider> m_OnTriggerEnter = new();

	[SerializeField]
	private PlayerSpearStore m_Store = new();
	[SerializeField]
	private PlayerSpearThrow m_Throw = new();
	[SerializeField]
	private PlayerSpearLand m_Land = new();
	[SerializeField]
	private PlayerSpearPull m_Pull = new();
	[SerializeField]
	private bool m_Log = false;

	private PlayerSpearController m_ActiveController = null;
	private Mono.Updateable m_Updateable = new(Mono.Type.Fixed, Mono.Priorities.ModelController);

	public State ActiveState => m_ActiveController != null ? m_ActiveController.State : State.Stored;

	private void Start()
	{
		m_Store.Setup(this);
		m_Throw.Setup(this);
		m_Land.Setup(this);
		m_Pull.Setup(this);
		Store();
	}
	private void OnDestroy()
	{
		m_ActiveController?.Stop();
	}

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}
	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	[Button]
	private void Throw() => Throw(transform.position, transform.forward);
	public void Throw(Vector3 pPoint, Vector3 pDirection/*, float pCharge01*/)
	{
		this.Log($"pPoint {pPoint}, pDirection {pDirection}");
		SwitchController(m_Throw);
		m_Throw.Start(pPoint, pDirection);
	}

	[Button]
	public void Attach(Transform pAttachTo, Vector3 pHitPoint)
	{
		this.Log($"pAttachTo {pAttachTo}");
		SwitchController(m_Land);
		m_Land.Start(pAttachTo, pHitPoint);
	}

	[Button]
	public void Pull(Transform pToTarget)
	{
		this.Log($"pToTarget {pToTarget}");
		SwitchController(m_Pull);
		m_Pull.Start(pToTarget);
	}

	[Button]
	public void Store()
	{
		this.Log();
		SwitchController(m_Store);
		m_Store.Start();
	}

	private void Tick(float pDeltaTime)
	{
		m_ActiveController?.Tick(pDeltaTime);
	}

	private void SwitchController(PlayerSpearController pController)
	{
		if (pController == m_ActiveController)
		{
			this.LogWarning($"Tried switching to the already active controller {pController.State}");
			// return;
		}
		m_ActiveController?.Stop();
		m_ActiveController = pController;
	}

	private void OnTriggerEnter(Collider pOther)
	{
		if (ActiveState == State.Landed)
		{
			m_OnTriggerEnter.Invoke(pOther);
		}
	}

	private void OnDrawGizmos()
	{
		m_ActiveController?.DrawGizmos();
	}

	internal void LogInternal(string pMessage, string pMethodName)
	{
		if (m_Log)
		{
			this.Log(pMessage, pMethodName);
		}
	}
}
