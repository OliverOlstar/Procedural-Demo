using UnityEngine;

namespace OCore
{
	[RequireComponent(typeof(CharacterController))]
    public class RootMotionCharacterReciever : MonoBehaviour
    {
        private RootMotionCharacter m_Parent;

        public void Init(RootMotionCharacter pParent)
        {
            m_Parent = pParent;
        }
        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            m_Parent.OnControllerColliderHit(hit);
        }
    }
}
