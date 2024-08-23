using UnityEngine;

namespace ODev
{
	public class ScreenMessageCanvas : MonoBehaviour
    {
        [SerializeField]
        private ScreenMessageWidget m_LargeMessage = null;
        [SerializeField]
        private ScreenMessageWidget m_SmallMessage = null;

        public ScreenMessageWidget LargeMessage => m_LargeMessage;
        public ScreenMessageWidget SmallMessage => m_SmallMessage;
    }
}
