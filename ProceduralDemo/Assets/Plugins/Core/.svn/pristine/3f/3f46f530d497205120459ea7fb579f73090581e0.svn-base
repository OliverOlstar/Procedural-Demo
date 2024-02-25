
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public enum PoolOrder
	{
		First = 0,
		Second,
		Third,
		Last
	}

	public interface IPoolBehaviour
	{
		PoolOrder ExecutionOrder { get; }

		void OnInitialize();

		void OnReset();

		bool KeepAlive();

		void OnPlay(UnityEngine.Object owner);

		void OnStop(bool kill);
	}
}
