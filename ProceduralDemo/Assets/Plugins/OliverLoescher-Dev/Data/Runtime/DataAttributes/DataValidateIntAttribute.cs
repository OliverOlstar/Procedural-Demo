using ODev.Util;

namespace ODev.Data.Validate.Int
{
	public class RangeAttribute : IntAttribute
	{
		private readonly int m_Min = 0;
		private readonly int m_Max = 1;

		public RangeAttribute(int min, int max, string columnNameToDisplayInErrors = null) : base(columnNameToDisplayInErrors)
		{
			m_Min = min;
			m_Max = max;
		}

		protected override string ValidateIntValue(int value)
		{
			return value >= m_Min && value <= m_Max ? null :
				Str.Build("Min: ", m_Min.ToString(), " Max: ", m_Max.ToString());
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
			return value >= m_Min ? null :
				Str.Build("Min: ", m_Min.ToString());
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
			return value <= m_Max ? null :
				Str.Build("Max: ", m_Max.ToString());
		}
	}
}