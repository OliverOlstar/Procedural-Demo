using System;
using System.Collections.Generic;
using UnityEngine;
using ODev;

public abstract class Messaging
{
	public const int DefaultSortOrder = 0;
}

public abstract class Messaging<TKey> : Messaging where TKey : Enum
{
	public delegate void Message<TData>(TData data) where TData : IMessageData;

	private static readonly Dictionary<TKey, Router> s_Routers;

	static Messaging()
	{
		s_Routers = new Dictionary<TKey, Router>();
	}

	public static void RegisterAction<TData>(TKey key, Message<TData> action)
		where TData : IMessageData
	{
		RegisterAction(key, action, DefaultSortOrder);
	}

	public static void RegisterAction<TData>(TKey key, Message<TData> action, int sortOrder)
		where TData : IMessageData
	{
		if (!s_Routers.TryGetValue(key, out Router router))
		{
			router = new Router<TData>();
			s_Routers.Add(key, router);
		}
		router.RegisterAction(action, sortOrder);
	}

	public static void DeregisterAction<TData>(TKey key, Message<TData> action)
		where TData : IMessageData
	{
		if (!s_Routers.TryGetValue(key, out Router router))
		{
			return;
		}
		router.DeregisterAction(action);
	}

	public static bool Route<TData>(TKey key, TData data) 
		where TData : IMessageData
	{
		if (!s_Routers.TryGetValue(key, out Router router))
		{
			return false;
		}
		return router.Route(data);
	}

	public static Type GetDataType(TKey key)
	{
		TryGetDataType(key, out Type dataType);
		return dataType;
	}

	public static bool TryGetDataType(TKey key, out Type dataType)
	{
		if (!s_Routers.TryGetValue(key, out Router router))
		{
			dataType = null;
			return false;
		}
		dataType = router.GetDataType();
		return true;
	}

	private abstract class Router
	{
		internal void RegisterAction(Delegate action, int sortOrder)
		{
			RegisterActionInternal(action, sortOrder);
		}

		internal void DeregisterAction(Delegate action)
		{
			DeregisterActionInternal(action);
		}

		internal bool Route(IMessageData data)
		{
			return RouteInternal(data);
		}

		internal abstract Type GetDataType();

		protected abstract void RegisterActionInternal(Delegate action, int sortOrder);
		protected abstract void DeregisterActionInternal(Delegate action);
		protected abstract bool RouteInternal(IMessageData data);
	}

	private class Router<TData> : Router where TData : IMessageData
	{
		private Dictionary<int, HashSet<Message<TData>>> m_Actions;
		private List<int> m_Order;

		internal Router()
		{
			m_Actions = new Dictionary<int, HashSet<Message<TData>>>();
			m_Order = new List<int>();
		}

		internal override Type GetDataType()
		{
			return typeof(TData);
		}

		protected sealed override void RegisterActionInternal(Delegate action, int sortOrder)
		{
			if (!m_Actions.TryGetValue(sortOrder, out HashSet<Message<TData>> actions))
			{
				actions = new HashSet<Message<TData>>();
				m_Actions[sortOrder] = actions;
				if (!m_Order.Contains(sortOrder))
				{
					m_Order.Add(sortOrder);
					m_Order.Sort();
				}
			}
			if (action is Message<TData> add)
			{
				actions.Add(add);
			}
		}

		protected sealed override void DeregisterActionInternal(Delegate action)
		{
			Message<TData> remove = action as Message<TData>;
			foreach (KeyValuePair<int, HashSet<Message<TData>>> kvp in m_Actions)
			{
				if (kvp.Value.Remove(remove))
				{
					break;
				}
			}
		}

		protected sealed override bool RouteInternal(IMessageData data)
		{
			if (!data.GetType().Is(typeof(TData)))
			{
				Debug.LogErrorFormat(
					"Messaging.RouteInternal() Received unexpected type while routing message. Expected {0} is not assignable from received {1}", 
					typeof(TData).Name, data.GetType().Name);
				return false;
			}
			TData tdata = (TData)data;

			// It's possible that new messages are registered/deregistered during routing, or event that this message is routed again during routing
			// Copy all message handlers into routing list in case any messages are registered/deregistered during routing
			List<Message<TData>> routingList = ListPool<Message<TData>>.Request();
			foreach (int order in m_Order)
			{
				if (m_Actions.TryGetValue(order, out HashSet<Message<TData>> set))
				{
					routingList.AddRange(set);
				}
			}
			foreach (Message<TData> msg in routingList)
			{
				try
				{
					msg?.Invoke(tdata);
				}
				catch (Exception exception)
				{
					// Actions which cause exceptions create the worst type of bugs because 
					// means someone could end up waiting forever for the next callback in the list
					// Its worth catching exceptions here in release, but we still want to make them obvious in dev
					DebugUtil.DevExceptionFormat("Caught exception while routing message: {0}", exception);
				}
			}
			ListPool<Message<TData>>.Return(routingList);
			return true;
		}
	}
}
