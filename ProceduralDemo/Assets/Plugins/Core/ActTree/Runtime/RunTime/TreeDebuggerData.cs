using UnityEngine;
using System.Collections.Generic;

public class TreeDebuggerData
{
	static readonly int SNAP_SHOT_COUNT = 50;

	static TreeDebuggerData s_Singleton = null;

	public static TreeDebuggerData Get()
	{
		if (s_Singleton == null)
		{
			s_Singleton = new TreeDebuggerData();
		}
		return s_Singleton;
	}

	public Dictionary<string, SnapShotGroup> m_Objects = new Dictionary<string, SnapShotGroup>();

	SnapShot[] GetSnapShotList(string id)
	{
		if (!m_Objects.ContainsKey(id))
		{
			m_Objects.Add(id, new SnapShotGroup(id, SNAP_SHOT_COUNT));
		}

		return m_Objects[id].GetSnapShots();
	}

	public void NewSnapShot(string id, ActSequencer actSequencer, ActParams animParams)
	{
#if UNITY_EDITOR
		InsertSnapShot(id, new SnapShot(actSequencer.GetRootSequencer(), animParams, actSequencer.ToString()));
#endif
	}

	public void NewEmptySnapShot(string id, ActSequencer actSequencer)
	{
#if UNITY_EDITOR
		if (actSequencer.GetNode().GetID() != Act.Node.ROOT_ID)
		{
			// Only save empty snapshots from a root sequencer.
			return;
		}
		InsertSnapShot(id, new SnapShot(null, null, actSequencer.GetTree().GetName() + " - Stopped"));
#endif
	}

	public void MissedParams(string id, ActParams animParams)
	{
#if UNITY_EDITOR
		SnapShot[] snapShots = GetSnapShotList(id);
		if (snapShots == null || snapShots.Length < 1 || snapShots[0] == null)
		{
			return;
		}
		SnapShot snapShot = snapShots[0];
		List<string> missedParams = new List<string>(snapShot.m_FailParamsStrings);
		missedParams.Add(animParams.ToString());
		snapShot.m_FailParamsStrings = missedParams.ToArray();
#endif
	}

	private void InsertSnapShot(string id, SnapShot snapShot)
	{
		SnapShot[] snapShots = GetSnapShotList(id);
		if (snapShots == null || snapShots.Length < 1)
		{
			return;
		}

		if (snapShots[0] != null)
		{
			for (int i = SNAP_SHOT_COUNT - 1; i > 0; i--)
			{
				if (snapShots[i - 1] == null)
				{
					continue;
				}

				snapShots[i] = snapShots[i - 1];
			}
		}
		snapShots[0] = snapShot;
	}

	public class SnapShot
	{
		public string m_SnapShotName = null;
		public ActSequencer m_Sequencer = null;
		public string m_AnimParamsString = null;
		public string[] m_FailParamsStrings = {};
		public float m_TimeStamp = -1.0f;

		public SnapShot(ActSequencer actSequencer, ActParams animParams, string name)
		{
			m_TimeStamp = Time.time;
			m_Sequencer = actSequencer?.Copy();
			m_AnimParamsString = animParams?.ToString() ?? "NoParams";
			m_SnapShotName = name;
		}
	}

	public class SnapShotGroup
	{
		public string m_Name = string.Empty;
		SnapShot[] m_SnapShots = null;

		public SnapShotGroup(string id, int maxSnapShots)
		{
			m_Name = id;
			m_SnapShots = new SnapShot[maxSnapShots];
		}

		public SnapShot[] GetSnapShots()
		{
			return m_SnapShots;
		}
	}
}
