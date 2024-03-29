﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher.Debug2
{
	public class GizmoForward : GizmoBase
	{
		[SerializeField]
		private float magnitude = 1.0f;
		[SerializeField]
		private float upOffset = 0.0f;

		protected override void DrawGizmos()
		{
			base.DrawGizmos();

			Vector3 root = transform.position + (transform.up * upOffset);
			Vector3 end = root + (transform.forward * magnitude);
			Gizmos.DrawLine(root, end);
		}
	}
}