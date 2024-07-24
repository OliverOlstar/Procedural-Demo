using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev.CheatMenu.Pages
{
    public struct CheatMenuDebugLogStruct
    {
		public string Message { get; private set; }
		public string CallStack { get; private set; }
		public LogType Type { get; private set; }
		public bool IsValid;

		public CheatMenuDebugLogStruct(string pMessage, string pCallStack, LogType pType, bool pIsValid)
		{
			Message = pMessage;
			CallStack = pCallStack;
			Type = pType;
			IsValid = pIsValid;
		}
    }
}
