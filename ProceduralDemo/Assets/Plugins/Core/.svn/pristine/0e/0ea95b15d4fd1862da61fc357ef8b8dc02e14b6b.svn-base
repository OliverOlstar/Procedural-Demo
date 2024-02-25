using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	/// <summary>Implemented by the owner of an Act System, intended to be some kind of UnityEngine.Object</summary>
	public interface IActObject
	{
		string name { get; }
		int GetInstanceID();
		System.Type GetContextType();
		bool TryGetNode(int nodeID, out IActNode node);
	}

	/// <summary>Information required for both runtime and serialized Act Nodes</summary>
	public interface IActNodeRuntime
	{
		string Name { get; }
		int ID { get; }
		bool IsEventRequired(out System.Type requiredEvent);
	}

	/// <summary>Interface for a serialized Act Node mostly to be used at edit time</summary>
	public interface IActNode : IActNodeRuntime
	{
		List<Condition> Conditions { get; }
		List<NodeTransition> Transitions { get; }
		List<Track> Tracks { get; }
		IEnumerable<ITimedItem> GetAllTimedItems();
	}

	/// <summary>Common interface shared between Tracks and Conditions</summary>
	public interface INodeItem
	{
		bool IsEventRequired(out System.Type trackEventType);
		bool _EditorIsValid(IActObject tree, IActNodeRuntime node, out string error);
	}

	public struct BaseNodeProperties
	{
		public string Name;
		public int ID;
		public bool HasTrack;
		public bool HasMajorTrack;
		public bool IsEvent;
		public System.Type RequiredEventType;
		public bool HasConditions;
		public bool HasFalseCondtion;

		public static BaseNodeProperties GetProperties(IActNode node)
		{
			bool isMaster = false;
			foreach (Track track in node.Tracks)
			{
				if (track != null && // Apparently this can happen?
					track.IsMajor())
				{
					isMaster = true;
					break;
				}
			}
			bool hasFalseCondition = false;
			foreach (Condition condition in node.Conditions)
			{
				if (condition is FalseCondition)
				{
					hasFalseCondition = true;
					break;
				}
			}
			bool isEvent = node.IsEventRequired(out System.Type requiredEventType);
			bool hasConditions = node.Conditions.Count > 0;
			BaseNodeProperties properties = new BaseNodeProperties
			{
				Name = node.Name,
				ID = node.ID,
				HasTrack = node.Tracks.Count > 0,
				HasMajorTrack = isMaster,
				IsEvent = isEvent,
				RequiredEventType = requiredEventType,
				HasConditions = hasConditions,
				HasFalseCondtion = hasFalseCondition,
			};
			return properties;
		}
	}

	public static class NodeItemUtil
	{
		public static bool _EditorIsValid<TContext>(INodeItem nodeItem, IActObject tree, IActNodeRuntime node, out string error)
		{
			if (!typeof(TContext).IsAssignableFrom(tree.GetContextType()))
			{
				error = ActItemContextError<TContext>(nodeItem);
				return false;
			}
			return ValidateRequiredEventType(nodeItem, node, out error);
		}

		public static bool TryInitialize<TContext>(INodeItem nodeItem, IActNodeRuntime node, ITreeContext genericContext, out TContext context, out string error) where TContext : ITreeContext
		{
			if (!(genericContext is TContext))
			{
				error = ActItemContextError<TContext>(nodeItem);
				context = default;
				return false;
			}
			if (!ValidateRequiredEventType(nodeItem, node, out error))
			{
				context = default;
				return false;
			}
			context = (TContext)genericContext;
			return true;
		}

		private static string ActItemContextError<TContext>(INodeItem nodeItem) =>
			$"Tree doesn't have required context {typeof(TContext).Name}, {nodeItem.GetType().Name} will be ignored at runtime";

		private static bool ValidateRequiredEventType(INodeItem nodeItem, IActNodeRuntime node, out string error)
		{
			if (!nodeItem.IsEventRequired(out System.Type trackEventType))
			{
				error = null;
				return true;
			}
			if (!node.IsEventRequired(out System.Type nodeEventType))
			{
				error = $"Requires event {Util.EventTypeToString(trackEventType)}, cannot be used on a 'Poling' node";
				return false;
			}
			if (!trackEventType.IsAssignableFrom(nodeEventType))
			{
				error = $"Node required event {Util.EventTypeToString(nodeEventType)} does not match track required event {Util.EventTypeToString(trackEventType)}";
				return false;
			}
			error = null;
			return true;
		}
	}
}
