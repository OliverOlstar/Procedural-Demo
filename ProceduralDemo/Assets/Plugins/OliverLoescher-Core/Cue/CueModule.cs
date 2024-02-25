using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OliverLoescher.Cue
{
	[System.Serializable]
	public class CueModule
	{
		public virtual void Play(CueContext pContext) { }
	}
}