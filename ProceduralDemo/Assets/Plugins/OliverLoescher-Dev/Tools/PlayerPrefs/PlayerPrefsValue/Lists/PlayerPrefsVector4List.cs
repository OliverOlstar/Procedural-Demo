using UnityEngine;

namespace ODev.PlayerPrefs
{
	public class PlayerPrefsVector4List : PlayerPrefsList<Vector4>
	{
		public PlayerPrefsVector4List(string key, bool isGlobalPref = false)
			: base(key, isGlobalPref)
		{
		}

		protected override Vector4 GetIndex(int index, bool isGlobal)
		{
			Vector4 Vector4 = Vector4.zero;
			if (isGlobal)
			{
				Vector4.x = PlayerPrefs.GetGlobalFloat($"{Key}{index}_x", 0.0f);
				Vector4.y = PlayerPrefs.GetGlobalFloat($"{Key}{index}_y", 0.0f);
				Vector4.z = PlayerPrefs.GetGlobalFloat($"{Key}{index}_z", 0.0f);
				Vector4.w = PlayerPrefs.GetGlobalFloat($"{Key}{index}_w", 0.0f);
			}
			else
			{
				Vector4.x = PlayerPrefs.GetFloat($"{Key}{index}_x", 0.0f);
				Vector4.y = PlayerPrefs.GetFloat($"{Key}{index}_y", 0.0f);
				Vector4.z = PlayerPrefs.GetFloat($"{Key}{index}_z", 0.0f);
				Vector4.w = PlayerPrefs.GetFloat($"{Key}{index}_w", 0.0f);
			}
			return Vector4;
		}

		protected override void SetIndex(int index, Vector4 value, bool isGlobal)
		{
			if (isGlobal)
			{
				PlayerPrefs.SetGlobalFloat($"{Key}{index}_x", value.x);
				PlayerPrefs.SetGlobalFloat($"{Key}{index}_y", value.y);
				PlayerPrefs.SetGlobalFloat($"{Key}{index}_z", value.z);
				PlayerPrefs.SetGlobalFloat($"{Key}{index}_w", value.w);
				return;
			}
			PlayerPrefs.SetFloat($"{Key}{index}_x", value.x);
			PlayerPrefs.SetFloat($"{Key}{index}_y", value.y);
			PlayerPrefs.SetFloat($"{Key}{index}_z", value.z);
			PlayerPrefs.SetFloat($"{Key}{index}_w", value.w);
		}
	}
}