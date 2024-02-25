using UnityEngine;

public class PlayerPrefsVector2List : PlayerPrefsList<Vector2>
{
	public PlayerPrefsVector2List(string key, bool isGlobalPref = false)
		: base(key, isGlobalPref)
	{
	}

	protected override Vector2 GetIndex(int index, bool isGlobal)
	{
		Vector2 Vector2 = Vector2.zero;
		if (isGlobal)
		{
			Vector2.x = PlayerPrefs.GetGlobalFloat($"{Key}{index}_x", 0.0f);
			Vector2.y = PlayerPrefs.GetGlobalFloat($"{Key}{index}_y", 0.0f);
		}
		else
		{
			Vector2.x = PlayerPrefs.GetFloat($"{Key}{index}_x", 0.0f);
			Vector2.y = PlayerPrefs.GetFloat($"{Key}{index}_y", 0.0f);
		}
		return Vector2;
	}

	protected override void SetIndex(int index, Vector2 value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_x", value.x);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_y", value.y);
			return;
		}
		PlayerPrefs.SetFloat($"{Key}{index}_x", value.x);
		PlayerPrefs.SetFloat($"{Key}{index}_y", value.y);
	}
}