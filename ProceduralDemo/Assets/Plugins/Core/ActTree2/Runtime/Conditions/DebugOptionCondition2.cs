using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Act2
{
	[MovedFrom(true, sourceAssembly: "Core")]
	[ConditionGroup.Debug]
	public class DebugOptionCondition : ConditionPoling<ITreeContext>
	{
		[SerializeField]
		private string m_OptionName = null;

		private DebugOption m_Option;

		protected override void OnInitialize(ref bool? failedToInitializeReturnValue)
		{
			m_Option = DebugOption.GetAllOptions().FirstOrDefault(x => Core.Str.Equals(x.Name, m_OptionName));
			if (m_Option == null)
			{
				failedToInitializeReturnValue = false;
			}
		}

		protected override bool EvaluatePoling()
		{
			return m_Option.IsSet();
		}

		public override string ToString()
		{
			return Core.Str.Build("DebugOption(", m_OptionName, ")");
		}
	}
}
