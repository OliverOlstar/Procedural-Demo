using System.Collections;
using UnityEngine;

namespace OCore
{
	public class ScreenMessage : Singleton<ScreenMessage>, ISingleton
	{
		public class Message
		{
			public string MyMessage { get; private set; }
			public float Seconds { get; private set; }

			public Message(string pMessage, float pSeconds)
			{
				MyMessage = pMessage;
				Seconds = pSeconds;
			}
		}

		private const string CANVAS_PREFAB_PATH = "ScreenMessages";

		private static ScreenMessageCanvas s_Canvas = null;
		private static ResourceRequest s_LoadCanvas = null;

		void ISingleton.OnAccessed()
		{
			if (s_Canvas == null && s_LoadCanvas == null)
			{
				Util.Mono.Start(Initalize());
			}
		}

		private static IEnumerator Initalize()
		{
			Log($"Loading Asset: {CANVAS_PREFAB_PATH}", "Initalize");
			s_LoadCanvas = Resources.LoadAsync(CANVAS_PREFAB_PATH);
			while (!s_LoadCanvas.isDone)
			{
				yield return null;
			}
			s_Canvas = (Object.Instantiate(s_LoadCanvas.asset) as GameObject).GetComponent<ScreenMessageCanvas>();
			s_LoadCanvas = null;
		}

		private IEnumerator LargeMessageInternal(string pMessage, float pSeconds)
		{
			while (s_Canvas == null)
			{
				yield return null;
			}
			s_Canvas.LargeMessage.QueueMessage(new Message(pMessage, pSeconds));
		}
		public IEnumerator SmallMessageInternal(string pMessage, float pSeconds)
		{
			while (s_Canvas == null)
			{
				yield return null;
			}
			s_Canvas.SmallMessage.QueueMessage(new Message(pMessage, pSeconds));
		}

		public static void LargeMessage(string pMessage, float pSeconds) => Util.Mono.Start(Instance.LargeMessageInternal(pMessage, pSeconds));
		public static void SmallMessage(string pMessage, float pSeconds) => Util.Mono.Start(Instance.SmallMessageInternal(pMessage, pSeconds));
	}
}
