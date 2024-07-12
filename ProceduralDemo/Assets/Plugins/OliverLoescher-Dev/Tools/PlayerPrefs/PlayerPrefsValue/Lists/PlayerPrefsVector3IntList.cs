using UnityEngine;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsVector3IntList : PlayerPrefsList<Vector3Int>
	{
		public PlayerPrefsVector3IntList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override Vector3Int GetIndex(int index, bool isGlobal)
		{
			Vector3Int Vector3Int = Vector3Int.zero;
			if (isGlobal)
			{
				Vector3Int.x = PlayerPrefs.GetGlobalInt($"{Key}{index}_x", 0);
				Vector3Int.y = PlayerPrefs.GetGlobalInt($"{Key}{index}_y", 0);
				Vector3Int.z = PlayerPrefs.GetGlobalInt($"{Key}{index}_z", 0);
			}
			else
			{
				Vector3Int.x = PlayerPrefs.GetInt($"{Key}{index}_x", 0);
				Vector3Int.y = PlayerPrefs.GetInt($"{Key}{index}_y", 0);
				Vector3Int.z = PlayerPrefs.GetInt($"{Key}{index}_z", 0);
			}
			return Vector3Int;
		}

		protected override void SetIndex(int index, Vector3Int value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt($"{Key}{index}_x", value.x);
				PlayerPrefs.SetGlobalInt($"{Key}{index}_y", value.y);
				PlayerPrefs.SetGlobalInt($"{Key}{index}_z", value.z);
				return;
			}
			PlayerPrefs.SetInt($"{Key}{index}_x", value.x);
			PlayerPrefs.SetInt($"{Key}{index}_y", value.y);
			PlayerPrefs.SetInt($"{Key}{index}_z", value.z);
		}
	}
}