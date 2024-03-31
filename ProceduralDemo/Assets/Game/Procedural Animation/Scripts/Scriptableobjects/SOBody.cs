using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PA
{
	[CreateAssetMenu(fileName = "New PA Body", menuName = "Procedural Animation/Body")]
	public class SOBody : ScriptableObject
	{
		[Header("Position")]
		public float SpringXZ = 1.0f;
		public float DamperXZ = 0.05f;

		public float SpringY = 2.0f;
		public float DamperY = 0.06f;

		[Header("Rotation")]
		public float RotationDampening = 8.0f;
		[Range(0.0f, 5.0f)]
		public float RotationBlendXZ = 1.0f;
		[Range(0.0f, 5.0f)]
		public float RotationBlendY = 1.0f;
	}
}
