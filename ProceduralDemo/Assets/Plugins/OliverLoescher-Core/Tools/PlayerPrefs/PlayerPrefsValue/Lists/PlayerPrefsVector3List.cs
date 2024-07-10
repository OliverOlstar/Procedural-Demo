using UnityEngine;

public class PlayerPrefsVector3List : PlayerPrefsList<Vector3>
{
	public PlayerPrefsVector3List(string key, bool isGlobalPref = false)
		: base(key, isGlobalPref)
	{
	}

	protected override Vector3 GetIndex(int index, bool isGlobal)
	{
		Vector3 Vector3 = Vector3.zero;
		if (isGlobal)
		{
			Vector3.x = PlayerPrefs.GetGlobalFloat($"{Key}{index}_x", 0.0f);
			Vector3.y = PlayerPrefs.GetGlobalFloat($"{Key}{index}_y", 0.0f);
			Vector3.z = PlayerPrefs.GetGlobalFloat($"{Key}{index}_z", 0.0f);
		}
		else
		{
			Vector3.x = PlayerPrefs.GetFloat($"{Key}{index}_x", 0.0f);
			Vector3.y = PlayerPrefs.GetFloat($"{Key}{index}_y", 0.0f);
			Vector3.z = PlayerPrefs.GetFloat($"{Key}{index}_z", 0.0f);
		}
		return Vector3;
	}

	protected override void SetIndex(int index, Vector3 value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_x", value.x);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_y", value.y);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_z", value.z);
			return;
		}
		PlayerPrefs.SetFloat($"{Key}{index}_x", value.x);
		PlayerPrefs.SetFloat($"{Key}{index}_y", value.y);
		PlayerPrefs.SetFloat($"{Key}{index}_z", value.z);
	}
}