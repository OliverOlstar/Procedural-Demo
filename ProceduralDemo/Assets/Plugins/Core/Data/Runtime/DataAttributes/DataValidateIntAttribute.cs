namespace Data.Validate.Int
{
	public class RangeAttribute : IntAttribute
	{
		private int m_Min = 0;
		private int m_Max = 1;

		public RangeAttribute(int min, int max, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Min = min;
			m_Max = max;
		}

		protected override string ValidateIntValue(int value)
		{
			return Core.Util.GreaterThanEquals(value, m_Min) && Core.Util.LessThanEquals(value, m_Max) ? null :
				Core.Str.Build("Min: ", m_Min.ToString(), " Max: ", m_Max.ToString());
		}
	}

	public class MinAttribute : IntAttribute
	{
		private int m_Min = 0;

		public MinAttribute(int min, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Min = min;
		}

		protected override string ValidateIntValue(int value)
		{
			return Core.Util.GreaterThanEquals(value, m_Min) ? null :
				Core.Str.Build("Min: ", m_Min.ToString());
		}
	}

	public class MaxAttribute : IntAttribute
	{
		private int m_Max = 0;

		public MaxAttribute(int max, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Max = max;
		}

		protected override string ValidateIntValue(int value)
		{
			return Core.Util.LessThanEquals(value, m_Max) ? null :
				Core.Str.Build("Max: ", m_Max.ToString());
		}
	}
}