
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ContentTree
{
	public class ContentTreeBehaviour : MonoBehaviour, Act2.ITreeOwner
	{
		[System.Serializable]
		public class ContentSource
		{
			[UberPicker.AssetNonNull, FormerlySerializedAs("Source")]
			public SOContentSource SourceName;

#if ODIN_INSPECTOR
			[Sirenix.OdinInspector.DrawWithUnity]
#endif
			[Core.ListableStyle.Single.Array, FormerlySerializedAs("Trees")]
			public Trees ContentTrees;
		}

		[System.Serializable]
		public class Trees : Core.ListableSO<Act2.ActTree2> { }

		[SerializeField, FormerlySerializedAs("m_InitialTrees")]
		private List<ContentSource> m_ContentSources = new List<ContentSource>();

		private class ContentTrees : List<Act2.TreeRT> { }

		private Dictionary<SOContentSource, ContentTrees> m_ContentTrees = new Dictionary<SOContentSource, ContentTrees>();

		protected struct ContentSequencer
		{
			public SOContentSource Source;
			public Act2.Sequencer Sequencer;

			public ContentSequencer(SOContentSource source, Act2.Sequencer sequencer)
			{
				Source = source;
				Sequencer = sequencer;
			}
		}
		private List<ContentSequencer> m_Sequencer = new List<ContentSequencer>();
		protected IEnumerable<ContentSequencer> Sequencers => m_Sequencer;

		public static void Add(GameObject gameObject, SOContentSource source, Act2.ActTree2 tree)
		{
			if (!gameObject.TryGetComponent(out ContentTreeBehaviour content))
			{
				content = gameObject.AddComponent<ContentTreeBehaviour>();
			}
			Act2.TreeRT newTree = content.AddTree(source, tree);
			newTree.Initialize();
		}

		public static void Remove(GameObject gameObject, SOContentSource source)
		{
			if (gameObject.TryGetComponent(out ContentTreeBehaviour content))
			{
				content.RemoveContent(source);
			}
		}

		protected virtual void Awake()
		{
			foreach (ContentSource tree in m_ContentSources)
			{
				foreach (Act2.ActTree2 t in tree.ContentTrees)
				{
					AddTree(tree.SourceName, t);
				}
			}
		}

		// Defer initialization to Start() so behaviours have time to wake up
		private void Start()
		{
			foreach (ContentTrees trees in m_ContentTrees.Values)
			{
				foreach (Act2.TreeRT tree in trees)
				{
					tree.Initialize();
				}
			}
		}

		protected virtual void OnEnable()
		{
			CleanUpSequencers(); // TODO: We need to guarantee a sequencer can't survive enable/disable otherwise it could get recycled by the pool while we're disabled
		}

		protected virtual void OnDisable()
		{
			CleanUpSequencers(); // In case we are re-enabled don't want to be holding on to invalid sequencers
		}

		public bool ContainsContentNode(SOContentSource source, SOContentNode node) => ContainsContentName(source, node.Name);
		public bool ContainsContentName(SOContentSource source, string nodeName = null)
		{
			if (!m_ContentTrees.TryGetValue(source, out ContentTrees trees))
			{
				return false;
			}
			if (string.IsNullOrEmpty(nodeName))
			{
				return true;
			}
			foreach (Act2.TreeRT tree in trees)
			{
				if (tree.HasNode(nodeName))
				{
					return true;
				}
			}
			return false;
		}

		public Act2.TreeRT AddTree(SOContentSource source, Act2.ActTree2 actTree)
		{
			if (!m_ContentTrees.TryGetValue(source, out ContentTrees trees))
			{
				trees = new ContentTrees();
				m_ContentTrees.Add(source, trees);
			}
			Act2.TreeRT rt = Act2.TreeRT.CreateAndDeferInitialize(actTree, this);
			trees.Add(rt);
			return rt;
		}

		public void RemoveContent(SOContentSource source)
		{
			if (!m_ContentTrees.ContainsKey(source))
			{
				return;
			}
			m_ContentTrees.Remove(source);
			for (int i = m_Sequencer.Count - 1; i >= 0; i--)
			{
				ContentSequencer seq = m_Sequencer[i];
				if (seq.Source.GetInstanceID() == seq.Source.GetInstanceID())
				{
					seq.Sequencer.StopAndDispose(Act2.Sequencer.ActionType.ForcedStop);
					m_Sequencer.RemoveAt(i);
				}
			}
		}

		public Act2.Sequencer TrySequenceNode(SOContentSource source, SOContentNode node, Act2.ITreeEvent treeEvent = null, float timeScale = 1.0f) =>
			TrySequenceName(source, node.Name, treeEvent, timeScale);
		public Act2.Sequencer TrySequenceName(SOContentSource source, string nodeName, Act2.ITreeEvent treeEvent = null, float timeScale = 1.0f)
		{
			if (source == null || string.IsNullOrEmpty(nodeName))
			{
				return null;
			}
			if (!m_ContentTrees.TryGetValue(source, out ContentTrees trees))
			{
				Debug.LogError($"{name} ContentTreeBehaviour.Play() Content source {source.name} doesn't exist");
				return null;
			}
			foreach (Act2.TreeRT tree in trees)
			{
				if (!tree.HasNode(nodeName))
				{
					continue; // Small optimization, can skip trying to sequence if we know the node we're looking for isn't in this tree
				}
				Act2.Sequencer sequencer = Act2.Sequencer.TryCreateAndSequenceNode(
					Act2.Sequencer.ActionType.ForcedPlayNode,
					tree,
					Act2.Sequencer.PlayNode.FromName(tree, nodeName),
					treeEvent != null ? new Act2.Params(treeEvent) : null,
					timeScale);
				if (sequencer != null)
				{
					m_Sequencer.Add(new ContentSequencer(source, sequencer));
					return sequencer;
				}
			}
			Debug.LogError($"{name} ContentTreeBehaviour.Play() Content node {nodeName} couldn't play");
			return null;
		}

		public virtual Act2.ITreeContext GetContext() => new Act2.GOContext(gameObject);

		private void Update()
		{
			CleanUpSequencers();
		}

		private void CleanUpSequencers()
		{
			for (int i = m_Sequencer.Count - 1; i >= 0; i--)
			{
				Act2.Sequencer seq = m_Sequencer[i].Sequencer;
				if (!seq.IsPlaying())
				{
					m_Sequencer.RemoveAt(i);
				}
			}
		}
	}
}
