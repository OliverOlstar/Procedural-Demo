using UnityEngine;

namespace OCore.PlayerPrefs
{
	public class PlayerPrefsVector2IntList : PlayerPrefsList<Vector2Int>
	{
		public PlayerPrefsVector2IntList(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override Vector2Int GetIndex(int index, bool isGlobal)
		{
			Vector2Int Vector2Int = Vector2Int.zero;
			if (isGlobal)
			{
				Vector2Int.x = PlayerPrefs.GetGlobalInt($"{Key}{index}_x", 0);
				Vector2Int.y = PlayerPrefs.GetGlobalInt($"{Key}{index}_y", 0);
			}
			else
			{
				Vector2Int.x = PlayerPrefs.GetInt($"{Key}{index}_x", 0);
				Vector2Int.y = PlayerPrefs.GetInt($"{Key}{index}_y", 0);
			}
			return Vector2Int;
		}

		protected override void SetIndex(int index, Vector2Int value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalInt($"{Key}{index}_x", value.x);
				PlayerPrefs.SetGlobalInt($"{Key}{index}_y", value.y);
				return;
			}
			PlayerPrefs.SetInt($"{Key}{index}_x", value.x);
			PlayerPrefs.SetInt($"{Key}{index}_y", value.y);
		}
	}
}