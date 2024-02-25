using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

public class DebugOptionCondition : ActCondition
{
	[SerializeField]
	private string m_OptionName = null;

	private DebugOption m_Option;

	protected override void OnInitialize(ActParams actParams)
	{
		m_Option = DebugOption.GetAllOptions().FirstOrDefault(x => Core.Str.Equals(x.Name, m_OptionName));
		base.OnInitialize(actParams);
	}

	public override bool Evaluate(ActParams param)
	{
		return m_Option != null && m_Option.IsSet();
	}

	public override string ToString()
	{
		return Core.Str.Build("DebugOptionCondition(", m_OptionName, ")");
	}
}
