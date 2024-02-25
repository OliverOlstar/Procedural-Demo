
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ContentTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Core")]
	public class ContentNodeTrack : Act2.TrackGeneric<Act2.IGOContext>
	{
		[SerializeField, UberPicker.AssetNonNull, FormerlySerializedAs("m_Source"), FormerlySerializedAs("m_ContentTree")]
		private SOContentSource m_ContentSourceName = null;

		[SerializeField, UberPicker.AssetNonNull, FormerlySerializedAs("m_NodeName"), FormerlySerializedAs("m_ContentNode")]
		private SOContentNode m_ContentNodeName = null;

		[SerializeField, UberPicker.Asset, FormerlySerializedAs("m_FallbackContentNode")]
		private SOContentNode m_FallbackContentNodeName = null;

		private ContentTreeBehaviour m_Content = null;
		private Act2.Sequencer m_Sequencer = null;

		public override EndEventType GetEndEventType() => EndEventType.EndTime;

		public override TrackType GetDefaultTrackType() => TrackType.Major;

		protected override bool OnInitialize()
		{
			if (m_ContentSourceName == null)
			{
				Debug.LogError($"ContentTreeTrack.TryStart() {base.m_Node.Name} source is null");
				return false;
			}
			if (m_ContentNodeName == null)
			{
				Debug.LogError($"ContentTreeTrack.TryStart() {base.m_Node.Name} node is null");
				return false;
			}
			if (!m_Context.GameObject.TryGetComponent(out m_Content))
			{
				m_Content = m_Context.GameObject.AddComponent<ContentTreeBehaviour>();
			}
			return true;
		}

		protected override bool TryStart(Act2.ITreeEvent treeEvent )
		{
			SOContentNode node = m_ContentNodeName;
			if (m_FallbackContentNodeName != null && !m_Content.ContainsContentNode(m_ContentSourceName, m_ContentNodeName))
			{
				node = m_FallbackContentNodeName;
			}
			m_Sequencer = m_Content.TrySequenceNode(m_ContentSourceName, node, treeEvent, CurrentTimeScale);
			if (m_Sequencer == null)
			{
				return false;
			}
			return base.TryStart(treeEvent);
		}

		protected override void OnStart()
		{
			m_Sequencer.Play(Act2.Sequencer.ActionType.ForcedPlayNode);
		}

		protected override bool OnUpdate(float time)
		{
			if (!m_Sequencer.IsPlaying()) // ContentTreeBehaviour can stop our sequencer if the tree gets unloaded
			{
				return false;
			}
			m_Sequencer.SetTimeScale(CurrentTimeScale);
			// ActSequenceController calls update with a delta time of 0 when a new node is sequenced, this makes sure tracks get a chance to update
			// If our time is zero then this must be that extra 0 time update, so we also want to pass a delta time of 0 into our sequencer
			float deltaTime = Core.Util.Approximately(time, 0.0f) ? 0.0f : Time.deltaTime;
			return m_Sequencer.Update(deltaTime);
		}

		protected override void OnEnd()
		{
			m_Sequencer.StopAndDispose(Interrupted ? Act2.Sequencer.ActionType.ForcedStop : Act2.Sequencer.ActionType.Resequence);
			m_Sequencer = null;
		}

		public override bool TryHandleEvent(Act2.Params newParams)
		{
			if (!m_Sequencer.IsPlaying())
			{
				return false;
			}
			if (m_Sequencer.NewParams(newParams))
			{
				return true;
			}
			return false;
		}
	}
}
