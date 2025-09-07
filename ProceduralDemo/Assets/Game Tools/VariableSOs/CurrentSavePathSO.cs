using UnityEngine;

namespace ODev.VariableSOs
{
    [CreateAssetMenu(menuName = "Variables/CurrentSavePath", fileName = "CurrentSavePath", order = 0)]
    public class CurrentSavePathSO : StringVariableSO
    {
        private const string STREAMING_ASSETS_PATH = "StreamingAssets";

        public bool IsSaveablePath(string path)
        {
			return string.IsNullOrEmpty(path) || !path.Contains(STREAMING_ASSETS_PATH);
		}
	}
}