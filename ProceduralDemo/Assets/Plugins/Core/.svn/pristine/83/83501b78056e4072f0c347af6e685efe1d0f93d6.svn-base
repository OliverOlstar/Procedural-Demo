using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public static class Util
	{
		public static string EventTypeToString(System.Type eventType)
		{
			string eventName = eventType.IsInterface ? eventType.Name.Substring(1) : eventType.Name;
			return eventName;
		}
	}
	public static class ConditionGroup
	{
		public class Core : ConditionGroupAttribute
		{
			public Core() : base("Core") { }
		}

		public class Logic : ConditionGroupAttribute
		{
			public Logic() : base("Logic") { }
		}

		public class Debug : ConditionGroupAttribute
		{
			public Debug() : base("Debug") { }
		}
	}
}
