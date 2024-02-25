
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class ConditionSet
	{
		private List<Condition> m_Conditions = new List<Condition>();
		public List<Condition> GetConditions() { return m_Conditions; }

		private float m_RandomWeight = -1.0f;
		public float GetRandomWeight() { return m_RandomWeight; }

		private int m_PolingFrequency = int.MaxValue;

		private int m_TimeSlicing = 0;

		public ConditionSet(IActNode node)
		{
			foreach (Condition condition in node.Conditions)
			{
				if (condition == null)
				{
					Debug.LogWarning($"ConditionGroup() Null condition in node {node}");
					continue;
				}
				m_Conditions.Add(condition);
				RandomCondition random = condition as RandomCondition;
				if (random != null)
				{
					m_RandomWeight = random.GetWeight();
				}
				if (condition.GetPolingFrequency() < m_PolingFrequency)
				{
					m_PolingFrequency = condition.GetPolingFrequency();
				}
			}
		}

		public void Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context)
		{
			foreach (Condition condition in m_Conditions)
			{
				condition.Initialize(tree, node, context);
			}
		}

		public void StateEnter(ITreeEvent treeEvent)
		{
			m_TimeSlicing = m_PolingFrequency;
			foreach (Condition condition in m_Conditions)
			{
				condition.StateEnter(treeEvent);
			}
		}

		public void StateExit()
		{
			foreach (Condition condition in m_Conditions)
			{
				condition.StateExit();
			}
		}

		public bool EvaluateConditions(ITreeEvent treeEvent, float weightedChance, bool timeSliced)
		{
			if (timeSliced)
			{
				m_TimeSlicing++;
				if (m_TimeSlicing < m_PolingFrequency)
				{
					return false;
				}
				m_TimeSlicing = 0;
			}
			return EvaluateConditionsInternal(treeEvent, weightedChance);
		}

		private  bool EvaluateConditionsInternal(ITreeEvent treeEvent, float weightedChance)
		{
			bool not = false;
			bool expression = true;
			foreach (Condition condition in m_Conditions)
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
					if (expression && EvaluateCondition(condition, treeEvent, weightedChance) == not)
					{
						expression = false;
					}

					not = false;
				}
			}

			return expression;
		}

		private bool EvaluateCondition(Condition condition, ITreeEvent treeEvent, float weightedChance)
		{
			RandomCondition random = condition as RandomCondition;
			if (random != null)
			{
				return weightedChance > -Core.Util.EPSILON && weightedChance < random.GetWeight();
			}
			return condition.Evaluate(treeEvent);
		}
	}
}
