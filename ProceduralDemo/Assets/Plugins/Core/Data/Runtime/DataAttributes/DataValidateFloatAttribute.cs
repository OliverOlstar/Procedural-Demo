namespace Data.Validate.Float
{
	public class RangeAttribute : FloatAttribute
	{
		private float m_Min = 0.0f;
		private float m_Max = 1.0f;

		public RangeAttribute(float min, float max, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Min = min;
			m_Max = max;
		}

		protected override string ValidateFloatValue(float value)
		{
			return Core.Util.GreaterThanEquals(value, m_Min) && Core.Util.LessThanEquals(value, m_Max) ? null :
				Core.Str.Build("Min: ", m_Min.ToString(), " Max: ", m_Max.ToString());
		}
	}

	public class MinAttribute : FloatAttribute
	{
		private float m_Min = 0.0f;

		public MinAttribute(float min, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Min = min;
		}

		protected override string ValidateFloatValue(float value)
		{
			return Core.Util.GreaterThanEquals(value, m_Min) ? null :
				Core.Str.Build("Min: ", m_Min.ToString());
		}
	}

	public class MaxAttribute : FloatAttribute
	{
		private float m_Max = 0.0f;

		public MaxAttribute(float min, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Max = min;
		}

		protected override string ValidateFloatValue(float value)
		{
			return Core.Util.LessThanEquals(value, m_Max) ? null :
				Core.Str.Build("Max: ", m_Max.ToString());
		}
	}
}