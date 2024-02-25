using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class NodeItemGroupAttribute : System.Attribute
	{
		public string GroupName;
		public Color Color;
		public System.Type[] TreeTypes;

		public NodeItemGroupAttribute(string name, Color color, params System.Type[] allowOnlyInTreeTypes)
		{
			GroupName = name;
			Color = color;
			TreeTypes = allowOnlyInTreeTypes;
		}
	}

	public class TrackGroupAttribute : NodeItemGroupAttribute
	{
		public TrackGroupAttribute(string name, Color color, params System.Type[] allowOnlyInTreeTypes) : base(name, color, allowOnlyInTreeTypes) { }
	}

	public class ConditionGroupAttribute : NodeItemGroupAttribute
	{
		public ConditionGroupAttribute(string name, params System.Type[] allowOnlyInTreeTypes) : base(name, CoreTrackGroup.DEFAULT_COLOR, allowOnlyInTreeTypes) { }
	}

	public static class CoreTrackGroup
	{
		public static readonly Color DEFAULT_COLOR = new Color(0.0f, 0.2f, 0.8f);

		public class DefaultAttribute : TrackGroupAttribute
		{
			public DefaultAttribute() : base(string.Empty, DEFAULT_COLOR) { }
		}

		public class DebugAttribute : TrackGroupAttribute
		{
			public DebugAttribute() : base("Debug", Core.ColorConst.Orange) { }
		}

		public class ActVarAttribute : TrackGroupAttribute
		{
			public ActVarAttribute() : base("Act Var", new Color(0.05f, 0.05f, 0.4f)) { }
		}
	}
}
