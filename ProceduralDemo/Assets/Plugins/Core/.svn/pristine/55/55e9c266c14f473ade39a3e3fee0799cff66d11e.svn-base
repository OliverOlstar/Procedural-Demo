
using System.Collections.Generic;
using UnityEngine;

public class ActConditionGroup
{
	List<ActCondition> m_Conditions = new List<ActCondition>();
	public List<ActCondition> GetConditions() { return m_Conditions; }

	float m_RandomWeight = -1.0f;
	public float GetRandomWeight() { return m_RandomWeight; }

	int m_PolingFrequency = -1;

	int m_TimeSlicing = 0;

	public void Add(ActCondition condition)
	{
		m_Conditions.Add(condition);
	}

	public void Initialize(ActTreeRT tree, ActNodeRT node, ActParams actParams)
	{
		m_PolingFrequency = int.MaxValue;
		foreach (ActCondition condition in m_Conditions)
		{
			condition.Initialize(tree, node, actParams);
			if (condition.RequiresEvent())
			{
				m_PolingFrequency = -1;
			}
			else
			{
				int frequency = condition.GetPolingFrequency();
				if (frequency < m_PolingFrequency)
				{
					m_PolingFrequency = frequency;
				}
			}
			RandomCondition random = condition as RandomCondition;
			if (random != null)
			{
				m_RandomWeight = random.GetWeight();
			}
		}
	}

	public void StateEnter(ActParams param)
	{
		m_TimeSlicing = m_PolingFrequency;
		foreach (ActCondition condition in m_Conditions)
		{
			condition.StateEnter(param);
		}
	}

	public void StateExit()
	{
		foreach (ActCondition condition in m_Conditions)
		{
			condition.StateExit();
		}
	}

	public bool PoleConditions(ActParams param)
	{
		if (m_PolingFrequency < 0)
		{
			return false;
		}
		m_TimeSlicing++;
		if (m_TimeSlicing < m_PolingFrequency)
		{
			return false;
		}
		m_TimeSlicing = 0;
		return EvaluateConditions(param, 0.0f); // Weighted chance of 0 means random conditions will always evaluate to true
	}

	public bool EvaluateConditions(ActParams param, float weightedChance)
	{
		bool not = false;
		bool expression = true;
		foreach (ActCondition condition in m_Conditions)
		{
			if (condition is OrCondition)
			{
				if (expression)
				{
					return true;
				}
				else
				{
					expression = true;
				}
			}
			else if (condition is NotCondition)
			{
				not = true;
			}
			else
			{
				if (expression && EvaluateCondition(condition, param, weightedChance) == not)
				{
					expression = false;
				}

				not = false;
			}
		}

		return expression;
	}

	bool EvaluateCondition(ActCondition condition, ActParams param, float weightedChance)
	{
		RandomCondition random = condition as RandomCondition;
		if (random != null)
		{
			return weightedChance > -Core.Util.EPSILON && weightedChance < random.GetWeight();
		}
		return condition.Evaluate(param);
	}
}
