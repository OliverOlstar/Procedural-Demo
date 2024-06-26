using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OCore.Cue
{
    public struct CueContext
    {
		public Vector3 Point { get; private set; }

		public CueContext(Vector3 pPoint)
		{
			Point = pPoint;
		}
    }
}
