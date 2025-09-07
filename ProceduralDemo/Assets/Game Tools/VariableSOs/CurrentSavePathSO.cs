using UnityEngine;

namespace ODev.VariableSOs
{
    [CreateAssetMenu(menuName = "Variables/CurrentSavePath", fileName = "CurrentSavePath", order = 0)]
    public class CurrentSavePathSO : StringVariableSO
    {
        private const string StreamingAssetsPath = "StreamingAssets";

        public bool IsSaveablePath(string path)
        {
            if (!string.IsNullOrEmpty(path) && path.Contains(StreamingAssetsPath))
            {
                return false;
            }
            return true;
        }
    }
}