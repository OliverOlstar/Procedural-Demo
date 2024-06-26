using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace OCore
{
    public class ScreenMessageWidget : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text m_Text = null;

        private readonly Queue<ScreenMessage.Message> m_Messages = new();
        private ScreenMessage.Message m_ActiveMessage = null;

        private void OnEnable()
        {
            m_Text.text = string.Empty;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void QueueMessage(ScreenMessage.Message pMessage)
        {
            if (m_ActiveMessage != null)
            {
                m_Messages.Enqueue(pMessage);
                return;
            }
            m_ActiveMessage = pMessage;
            StartCoroutine(DisplayRoutine());
        }

        private IEnumerator DisplayRoutine()
        {
            m_Text.text = m_ActiveMessage.MyMessage;

            Color color = m_Text.color;
            color.a = 0.0f;
            m_Text.color = color;
            while (color.a < 1.0f)
            {
                yield return null;
                color.a += Time.deltaTime * 5.0f;
                m_Text.color = color;
            }

            yield return new WaitForSeconds(m_ActiveMessage.Seconds);

            while (color.a > 0.0f)
            {
                yield return null;
                color.a -= Time.deltaTime * 5.0f;
                m_Text.color = color;
            }

            m_Text.text = string.Empty;

            if (m_Messages.Count > 0)
            {
                m_ActiveMessage = m_Messages.Dequeue();
                StartCoroutine(DisplayRoutine());
                yield break;
            }
            m_ActiveMessage = null;
        }
    }
}
