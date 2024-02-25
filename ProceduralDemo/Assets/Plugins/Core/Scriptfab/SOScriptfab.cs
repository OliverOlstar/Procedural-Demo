using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public abstract class SOScriptfabGeneric<TValue> : SOCacheName
		where TValue : class
	{
		public abstract TValue Value { get; }
	}

	public abstract class SOScriptfab<TValue> : SOScriptfabGeneric<TValue>
		where TValue : class
	{
#if ODIN_INSPECTOR
		[Sirenix.OdinInspector.DrawWithUnity]
#endif
		[SerializeReference, SerializedReferenceDrawer(SerializedRefGUIStyle.Flat)]
		private TValue m_Type;
		public override TValue Value => m_Type;
	}

	public abstract class SOScriptfabWithNotes<TValue> : SOScriptfabGeneric<TValue>
		where TValue : class
	{
		[SerializeField]
		private InspectorNotes m_Notes = new InspectorNotes();

#if ODIN_INSPECTOR
		[Sirenix.OdinInspector.DrawWithUnity]
#endif
		[SerializeReference, SerializedReferenceDrawer(SerializedRefGUIStyle.Flat)]
		private TValue m_Type;
		public override TValue Value => m_Type;
	}
}
