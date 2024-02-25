using UnityEngine;
using System.Collections.Generic;

namespace Act2
{
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

		public class ObjectSnapShots : Dictionary<string, SnapShotGroup> { };

		public class SnapShots : Dictionary<string, ObjectSnapShots> { };

		public SnapShots m_Objects = new();

		private SnapShot[] GetOrCreateSnapShots(string ownerName, string treeName)
		{
			if (!m_Objects.TryGetValue(ownerName, out ObjectSnapShots objShots))
			{
				objShots = new();
				m_Objects.Add(ownerName, objShots);
			}
			if (!objShots.TryGetValue(treeName, out SnapShotGroup shots))
			{
				shots = new(treeName, SNAP_SHOT_COUNT);
				objShots.Add(treeName, shots);
			}
			return shots.m_SnapShots;
		}

		public bool TryGetSnapShot(string ownerName, string treeName, int index, out SnapShot snapShot)
		{
			if (!m_Objects.TryGetValue(ownerName, out ObjectSnapShots objShots))
			{
				snapShot = null;
				return false;
			}
			if (!objShots.TryGetValue(treeName, out SnapShotGroup shots))
			{
				snapShot = null;
				return false;
			}
			if (index < 0 || index >= shots.m_SnapShots.Length)
			{
				snapShot = null;
				return false;
			}
			snapShot = shots.m_SnapShots[index];
			return snapShot != null;
		}
		public bool TryGetCurrentSnapShot(string ownerName, string treeName, out SnapShot snapShot) => 
			TryGetSnapShot(ownerName, treeName, 0, out snapShot);
		public bool TryGetPreviousSnapShot(string ownerName, string treeName, out SnapShot snapShot) =>
			TryGetSnapShot(ownerName, treeName, 1, out snapShot);

		public void OnPlay(Sequencer actSequencer, Sequencer.ActionType action, ITreeEvent treeEvent = null, TransitionRT transition = null)
		{
#if UNITY_EDITOR
			if (!actSequencer.IsLeaf())
			{
				return;
			}
			SnapShot[] snapShots = GetOrCreateSnapShots(actSequencer.Tree.Owner.name, actSequencer.Tree.Name);
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
			snapShots[0] = new SnapShot(action, actSequencer, treeEvent, transition);
#endif
		}

		public void OnStop(Sequencer actSequencer, Sequencer.ActionType action)
		{
#if UNITY_EDITOR
			if (TryGetCurrentSnapShot(actSequencer.Tree.Owner.name, actSequencer.Tree.Name, out SnapShot snapShot))
			{
				snapShot.OnStop(action, actSequencer);
			}
#endif
		}

		public void MissedParams(Sequencer actSequencer, Params animParams)
		{
#if UNITY_EDITOR
			if (TryGetCurrentSnapShot(actSequencer.Tree.Owner.name, actSequencer.Tree.Name, out SnapShot snapShot))
			{
				snapShot.m_FailParamsStrings.Add(new MissedParamsSnapShot(animParams, actSequencer.GetLeafSequencer().Timer));
			}
#endif
		}

		public enum TrackState
		{
			Undecided,
			NotStarted,
			Started,
			Ended,
			FailedToStart,
			Interrupted
		}

		public class TrackSnapShot : ITimedItem
		{
			public string Name;
			public TrackState State;

			private Track.EndEventType m_EndEventType;
			private float m_StartTime;
			private float m_EndTime;
			private bool m_Master;

			public TrackSnapShot(Track track, bool isPlaying)
			{
				State =
					isPlaying ? TrackState.Undecided : // Only when the sequencer has stopped can we really know the state of it's tracks
					track.FailedToStart ? TrackState.FailedToStart :
					track.Interrupted ? TrackState.Interrupted :
					track.Ended ? TrackState.Ended :
					track.Started ? TrackState.Started :
					TrackState.NotStarted;
				Name = track.ToString();
				if (!isPlaying)
				{
					Name += $" [{State}]";
				}
				m_EndEventType = track.GetEndEventType();
				m_StartTime = track.GetStartTime();
				m_EndTime = track._EditorDisplayEndTime();
				m_Master = track.IsMajor();
			}

			public float GetStartTime() => m_StartTime;
			public Track.EndEventType GetEndEventType() => m_EndEventType;
			public bool HasEndEvent() => m_EndEventType != Track.EndEventType.NoEndEvent;
			public bool HasNegativeEndTime() => m_EndTime < 0.0f;
			public bool IsActive() => true;
			public bool IsMajor() => m_Master;
			public void _EditorAddSubTimes(List<float> times) { }
			public float _EditorDisplayEndTime() => m_EndTime;
			public Color _EditorGetColor()
			{
				Color c = NodeTransition.EDITOR_COLOR;
				switch (State)
				{
					case TrackState.Undecided:
						c = Color.Lerp(c, Color.blue, 0.5f);
						break;
					case TrackState.NotStarted:
						c = Color.Lerp(c, Color.black, 0.5f);
						break;
					case TrackState.FailedToStart:
						c = Color.Lerp(c, Color.red, 0.5f);
						break;
					case TrackState.Started:
					case TrackState.Interrupted:
						c = Color.Lerp(c, Color.yellow, 0.5f);
						break;
					case TrackState.Ended:
						c = Color.Lerp(c, Color.green, 0.5f);
						break;
				}
				return c;
			}
		}

		public class TransitionSnapShot : ITimedItem
		{
			public Node.Properties ToNodeProperties;
			public bool Taken;
			private float m_StartTime;
			private float m_EndTime;

			public TransitionSnapShot(ActTree2 tree, TransitionRT transition)
			{
				ToNodeProperties = Node.GetProperties(tree, transition.ToNode.SourceNode);
				Taken = false;

				m_StartTime = transition.Transition.GetStartTime();
				m_EndTime = transition.Transition.GetEndTime();
			}

			public void UpdateTakenState(SnapShot nextShot)
			{
				if (nextShot == null || nextShot.m_Transition == null)
				{
					Taken = false;
					return;
				}
				if (ToNodeProperties.ID != nextShot.m_Transition.ToNode.ID)
				{
					Taken = false;
					return;
				}
				if (!Core.Util.Approximately(m_StartTime, nextShot.m_Transition.Transition.GetStartTime()) ||
					!Core.Util.Approximately(m_EndTime, nextShot.m_Transition.Transition.GetEndTime()))
				{
					Taken = false;
					return;
				}
				Taken = true;
			}

			public float GetStartTime() => m_StartTime;
			public Track.EndEventType GetEndEventType() => m_StartTime < 0.0f ? Track.EndEventType.NoEndEvent : Track.EndEventType.EndTime;
			public bool HasEndEvent() => GetEndEventType() != Track.EndEventType.NoEndEvent;
			public bool HasNegativeEndTime() => m_EndTime < 0.0f;
			public bool IsActive() => true;
			public bool IsMajor() => false;
			public void _EditorAddSubTimes(List<float> times) { }
			public float _EditorDisplayEndTime() => m_EndTime;
			public Color _EditorGetColor() => Taken ? Color.Lerp(NodeTransition.EDITOR_COLOR, Color.green, 0.5f) : NodeTransition.EDITOR_COLOR;
		}

		public class SequencerSnapShot
		{
			public string Path;
			public Node.Properties NodeProperties;
			public string Params = null;

			public float StartTime = 0.0f;
			public float StopTime = -1.0f;
			public float Duration => StopTime > 0.0f ? StopTime - StartTime : 0.0f;

			public List<TrackSnapShot> TrackStates = null;
			public List<TransitionSnapShot> TransitionSnapShots = null;
			public Sequencer.ActionType? StopReason = null;
			public bool IsStopped => StopReason.HasValue;

			public SequencerSnapShot(Sequencer actSequencer)
			{
				Path = actSequencer._EditorTreePath;
				Params = actSequencer.Params?.ToString();
				NodeRT node = actSequencer.Node;
				NodeProperties = Node.GetProperties(actSequencer.Tree.SourceTree, node.SourceNode);
				StartTime = actSequencer.Timer;

				if (node.TryGetTransitions(out IReadOnlyList<TransitionRT> transitions))
				{
					TransitionSnapShots = new List<TransitionSnapShot>(transitions.Count);
					foreach (TransitionRT transition in transitions)
					{
						TransitionSnapShots.Add(new TransitionSnapShot(actSequencer.Tree.SourceTree, transition));
					}
				}
				else
				{
					TransitionSnapShots = new List<TransitionSnapShot>();
				}

				TrackStates = new List<TrackSnapShot>(node.Tracks.List.Count);
				UpdateStateFromSequencer(actSequencer, true);
			}

			public void Stop(Sequencer.ActionType reasonForStopping, Sequencer sequencer)
			{
				StopReason = reasonForStopping;
				StopTime = sequencer.Timer;
				UpdateStateFromSequencer(sequencer, false);
			}

			private void UpdateStateFromSequencer(Sequencer sequencer, bool isPlaying)
			{
				TrackStates.Clear();
				foreach (Track track in sequencer.Node.Tracks.List)
				{
					TrackStates.Add(new TrackSnapShot(track, isPlaying));
				}
				TrackStates.Sort(Track.CompareTracks);
			}

			public IEnumerable<ITimedItem> GetTracksAndTransitions()
			{
				foreach (TransitionSnapShot trans in TransitionSnapShots)
				{
					yield return trans;
				}
				foreach (TrackSnapShot track in TrackStates)
				{
					yield return track;
				}
			}
		}

		public class SnapShot
		{
			public string m_SnapShotName = null;

			public string m_TreeName = string.Empty;
			public string m_TreePath = string.Empty;
			public System.Type m_TreeType = null;

			public List<SequencerSnapShot> m_SequencerSnapShots = new List<SequencerSnapShot>();

			public string m_EventString = null;

			public Sequencer.ActionType m_Action;
			public TransitionRT m_Transition = null;

			public List<MissedParamsSnapShot> m_FailParamsStrings = new List<MissedParamsSnapShot>();
			public List<SnapShotStopped> m_Stopped = new List<SnapShotStopped>(); // Note: There should really only be one
			public float m_TimeStamp = -1.0f;

			public SnapShot(Sequencer.ActionType action, Sequencer actSequencer, ITreeEvent treeEvent = null, TransitionRT transition = null)
			{
				m_SnapShotName = actSequencer.GetLeafSequencer()._EditorTreePath ?? throw new System.ArgumentNullException("SnapShot() Sequencer cannot be null");
				m_EventString = treeEvent?.ToString() ?? "No Event";

				m_TreeName = actSequencer.Tree.Name;
				m_TreeType = actSequencer.Tree.SourceTree.GetType();
#if UNITY_EDITOR
				m_TreePath = UnityEditor.AssetDatabase.GetAssetPath(actSequencer.Tree.SourceTree);
#endif

				m_Action = action;
				m_Transition = transition;

				m_TimeStamp = Time.time;

				UpdateFromSequencer(actSequencer);
			}

			public void UpdateFromSequencer(Sequencer actSequencer)
			{
				actSequencer = actSequencer.GetRootSequencer();
				m_SequencerSnapShots.Clear();
				do
				{
					m_SequencerSnapShots.Add(new SequencerSnapShot(actSequencer));
					actSequencer = actSequencer.ChildSequencer;
				}
				while (actSequencer != null);
			}

			public void OnStop(Sequencer.ActionType action, Sequencer actSequencer)
			{
				foreach (SequencerSnapShot shot in m_SequencerSnapShots)
				{
					if (shot.Path == actSequencer._EditorTreePath)
					{
						shot.Stop(action, actSequencer);
						break;
					}
				}
				if (actSequencer.IsLeaf())
				{
					// Special case when we're resequencing there might be another node with major tracks higher up in the hierarchy
					// that will keep the sequencer alive and prevent it stopping
					if (action == Sequencer.ActionType.Resequence)
					{
						Sequencer parent = actSequencer;
						while (parent.ParentSequencer != null)
						{
							parent = parent.ParentSequencer;
							if (parent.IsPlaying() && parent.Node.HasMajorTrack())
							{
								return;
							}
						}
					}
					m_Stopped.Add(new SnapShotStopped(action));
				}
			}

			public float GetDuration()
			{
				float duration = 0.0f;
				foreach (SequencerSnapShot shot in m_SequencerSnapShots)
				{
					if (shot.Duration > duration)
					{
						duration = shot.Duration;
					}
				}
				return duration;
			}
		}

		public class SnapShotStopped
		{
			public Sequencer.ActionType Action;
			public float TimeStamp = 0.0f;

			public SnapShotStopped(Sequencer.ActionType action)
			{
				Action = action;
				TimeStamp = Time.time;
			}
		}

		public class MissedParamsSnapShot
		{
			public string m_Params = null;
			public float m_Timer = 0.0f;
			public float m_TimeStamp = 0.0f;

			public MissedParamsSnapShot(Params animParams, float timer)
			{
				m_Params = animParams.ToString();
				m_Timer = timer;
				m_TimeStamp = Time.time;
			}
		}

		public class SnapShotGroup
		{
			public string m_Name = string.Empty;
			public SnapShot[] m_SnapShots = null;

			public SnapShotGroup(string id, int maxSnapShots)
			{
				m_Name = id;
				m_SnapShots = new SnapShot[maxSnapShots];
			}
		}
	}
}
