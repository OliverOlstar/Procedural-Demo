using Sirenix.OdinInspector;

namespace OCore.Input
{
	[System.Serializable]
    public class InputModule_Vector2Update : InputModule_Vector2
	{
		[BoxGroup]
		public UnityEventsUtil.Vector2Event OnUpdate;

		public override void Update(in float pDeltaTime)
		{
			OnUpdate?.Invoke(Input);
		}
	}
}
