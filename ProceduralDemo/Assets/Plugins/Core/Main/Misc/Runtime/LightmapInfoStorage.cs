using UnityEngine;

namespace Core
{
	[ExecuteInEditMode]
	public class LightmapInfoStorage : MonoBehaviour
	{
		[System.Serializable]
		public class Lightmap { public Texture2D mNear; public Texture2D mFar; }
		public Lightmap[] m_Lightmaps = new Lightmap[0];
		public LightmapsMode m_LightmapsMode = 0;

		public void ApplyLightmaps()
		{
			if (m_Lightmaps.Length == 0)
			{
				return;
			}

			LightmapData[] lightmaps = new LightmapData[m_Lightmaps.Length];
			for (int i = 0; i < lightmaps.Length; i++)
			{
				lightmaps[i] = new LightmapData();
				lightmaps[i].lightmapDir = m_Lightmaps[i].mNear;
				lightmaps[i].lightmapColor = m_Lightmaps[i].mFar;
			}
			LightmapSettings.lightmaps = lightmaps;
			LightmapSettings.lightmapsMode = m_LightmapsMode;
		}

		void Awake()
		{
			ApplyLightmaps();
        }
	}
}
