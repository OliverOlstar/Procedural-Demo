
using UnityEngine;

[Act.TrackGroup(Act.CoreTrackGroup.Debug)]
public class DebugPrintTrack : ActTrack
{
	public enum LogType
	{
		Log = 0,
		Warn,
		Error,
		Exception
	}

	[SerializeField]
	private LogType m_Type = LogType.Log;
	[SerializeField, TextArea]
	private string m_DebugMessage = string.Empty;
	[SerializeField]
	private Color m_Color = Color.black;
	[SerializeField]
	private bool m_HasEnd = true;

	private enum TimeType
	{
		GameTime = 0,
		RealTime
	}

	[SerializeField]
	private TimeType m_Time = TimeType.GameTime;
	private float GetTime() => m_Time == TimeType.GameTime ? Time.time : Time.realtimeSinceStartup;

	private ActParams m_Params = null;
	private float m_TimeStamp = 0.0f;

	public override EndEventType GetEndEventType()
	{
		return m_HasEnd ? EndEventType.EndTime : EndEventType.NoEndEvent;
	}

	protected override bool OnInitialize(ActParams actParams)
	{
		// Don't want to be logging in release builds
		if (Core.Util.IsRelease())
		{
			return false;
		}
		return base.OnInitialize(actParams);
	}

	protected override bool TryStart(ActParams actParams)
	{
		m_Params = actParams;
		return true;
	}

	private void Print(string callbackName, float duration = -1.0f)
	{
		string title = Core.DebugUtil.ColorString(m_Color, $"DebugPrintTrack.{callbackName}() {m_DebugMessage} - {m_Time}:{GetTime()}");
		Core.Str.AddLine(title);
		if (Core.Util.GreaterThanEquals(duration, 0.0f))
		{
			Core.Str.Add("duration: ", Core.DebugUtil.TruncatedFloatToString(duration, 4), "s ", m_Time.ToString(), "\n",
				Core.DebugUtil.TruncatedFloatToString(Core.Util.FPS30 * duration, 1), " frames");
			if (HasNegativeEndTime())
			{
				Core.Str.Add("\n");
			}
			else
			{
				Core.Str.AddLine("/", GetEndTime().ToString());
			}
		}
		Core.Str.AddLine(m_Node.ToString());
		Core.Str.Add(m_Params.ToString());
		switch (m_Type)
		{
			case LogType.Warn:
				Debug.LogWarning(Core.Str.Finish());
				break;
			case LogType.Error:
				Debug.LogError(Core.Str.Finish());
				break;
			case LogType.Exception:
				Core.DebugUtil.DevException(Core.Str.Finish());
				break;
			default:
				Debug.Log(Core.Str.Finish());
				break;
		}
	}

	protected override void OnCancelled()
	{
		Print("OnCancelled");
	}

	protected override void OnStart()
	{
		Print("OnStart");
		m_TimeStamp = GetTime();
	}

	protected override void OnEnd()
	{
		float duration = GetTime() - m_TimeStamp;
		Print("OnEnd", duration);
	}

	protected override void OnInterrupted()
	{
		float duration = GetTime() - m_TimeStamp;
		Print("OnInterrupted", duration);
	}

	public override Color _EditorGetColor()
	{
		switch (m_Type)
		{
			case LogType.Warn:
				return Core.ColorConst.Orange;
			case LogType.Error:
				return Color.red;
			case LogType.Exception:
				return Color.red;
			default:
				return Color.blue;
		}
	}
}
