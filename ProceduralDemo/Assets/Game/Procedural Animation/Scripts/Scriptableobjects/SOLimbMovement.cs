using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using OliverLoescher.Cue;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Limb Movement", menuName = "Procedural Animation/Limb/Movement")]
	public class SOLimbMovement : ScriptableObject
	{
		public float StepDistance = 1.0f;

		[Header("Animation")]
		public Easing.EaseParams EaseStep;
		public Easing.EaseParams EaseHeight;
		[Min(Math.NEARZERO)]
		public Vector2 StepSeconds = Vector2.one;
		public float UpHeight = 1.0f;

		[Header("Linecast")]
		public Vector2 LinecastUpDown = new(1, -1);
		public LayerMask StepLayer = new();

		[Header("Cues")]
		public SOCue OnStepCue;
	}
}
