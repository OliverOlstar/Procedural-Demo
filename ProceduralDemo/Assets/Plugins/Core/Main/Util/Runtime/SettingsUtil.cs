public static class SettingsUtil
{
	private const string PLAYERPREFPREFIX = "ECGSETTINGS_";
	private static bool? s_SFXState = null;
	private static bool? s_MusicState = null;
	private static string s_Language = null;
	private static int s_QualitySetting = -1;

	public static void Start()
	{
		//call at game start to change audio settings
	}
	
	public static bool SFXOn
	{
		get
		{
			if (!s_SFXState.HasValue)
			{
				s_SFXState = PlayerPrefs.GetInt(Core.Str.Build(PLAYERPREFPREFIX, "SFX"), 1) != 0;
			}

			return s_SFXState.Value;
		}

		set
		{
			s_SFXState = value;
			PlayerPrefs.SetInt(Core.Str.Build(PLAYERPREFPREFIX, "SFX"), s_SFXState.Value ? 1:0);
		}
	}
	
	public static bool MusicOn
	{
		get
		{
			if (!s_MusicState.HasValue)
			{
				s_MusicState = PlayerPrefs.GetInt(Core.Str.Build(PLAYERPREFPREFIX, "Music"), 1) != 0;
			}

			return s_MusicState.Value;
		}
		
		set
		{
			s_MusicState = value;
			PlayerPrefs.SetInt(Core.Str.Build(PLAYERPREFPREFIX, "Music"), s_MusicState.Value ? 1:0);
		}
	}
	
	public static string Language
	{
		get
		{
			if (!string.IsNullOrEmpty(s_Language))
			{
				s_Language = PlayerPrefs.GetString(Core.Str.Build(PLAYERPREFPREFIX, "Language"), "English");
			}

			return s_Language;
		}
		
		set
		{
			s_Language = value;
			PlayerPrefs.SetString(Core.Str.Build(PLAYERPREFPREFIX, "Language"), s_Language);
		}
	}
	
	public static int QualitySetting
	{
		get
		{
			if (s_QualitySetting < 0)
			{
				s_QualitySetting = PlayerPrefs.GetInt(Core.Str.Build(PLAYERPREFPREFIX, "Quality"), 2);
			}

			return s_QualitySetting;
		}
		
		set
		{
			s_QualitySetting = value;
			PlayerPrefs.SetInt(Core.Str.Build(PLAYERPREFPREFIX, "Quality"), s_QualitySetting);
		}
	}
}
