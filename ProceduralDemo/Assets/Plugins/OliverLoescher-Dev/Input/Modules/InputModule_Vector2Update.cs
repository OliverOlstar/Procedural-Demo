using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ODev.Input
{
	[System.Serializable]
    public class InputModule_Vector2Update : InputModule_Vector2, IInputVector2Update
	{
		[SerializeField, BoxGroup]
		public UnityEventsUtil.Vector2Event m_OnUpdate;

		public override void Update(in float pDeltaTime)
		{
			m_OnUpdate?.Invoke(Input);
		}

		public void DeregisterOnUpdate(UnityAction<Vector2> pAction) => m_OnUpdate.AddListener(pAction);
		public void RegisterOnUpdate(UnityAction<Vector2> pAction) => m_OnUpdate.RemoveListener(pAction);
	}
}
