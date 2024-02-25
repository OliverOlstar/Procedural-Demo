using UnityEngine;

public class PlayerPrefsColorList : PlayerPrefsList<Color>
{
	public PlayerPrefsColorList(string key, bool isGlobalPref = false)
		: base(key, isGlobalPref)
	{
	}

	protected override Color GetIndex(int index, bool isGlobal)
	{
		Color color = Color.white;
		if (isGlobal)
		{
			color.r = PlayerPrefs.GetGlobalFloat($"{Key}{index}_r", 0.0f);
			color.g = PlayerPrefs.GetGlobalFloat($"{Key}{index}_g", 0.0f);
			color.b = PlayerPrefs.GetGlobalFloat($"{Key}{index}_b", 0.0f);
			color.a = PlayerPrefs.GetGlobalFloat($"{Key}{index}_a", 0.0f);
		}
		else
		{
			color.r = PlayerPrefs.GetFloat($"{Key}{index}_r", 0.0f);
			color.g = PlayerPrefs.GetFloat($"{Key}{index}_g", 0.0f);
			color.b = PlayerPrefs.GetFloat($"{Key}{index}_b", 0.0f);
			color.a = PlayerPrefs.GetFloat($"{Key}{index}_a", 0.0f);
		}
		return color;
	}

	protected override void SetIndex(int index, Color value, bool isGlobal)
	{
		if (isGlobal)
		{
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_r", value.r);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_g", value.g);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_b", value.b);
			PlayerPrefs.SetGlobalFloat($"{Key}{index}_a", value.a);
			return;
		}
		PlayerPrefs.SetFloat($"{Key}{index}_r", value.r);
		PlayerPrefs.SetFloat($"{Key}{index}_g", value.g);
		PlayerPrefs.SetFloat($"{Key}{index}_b", value.b);
		PlayerPrefs.SetFloat($"{Key}{index}_a", value.a);
	}
}