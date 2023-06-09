using TMPro;
using UnityEngine;
namespace Inworld
{
    public class InworldLog : MonoBehaviour
    {
        [SerializeField] TMP_Text m_LogArea;

        void Awake()
        {
            Application.logMessageReceived += OnLogReceived;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= OnLogReceived;
        }

        void OnLogReceived(string log, string backTrace, LogType type)
        {
            if (!m_LogArea)
                return;
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    m_LogArea.text += $"<color=red>{log}</color>\n";
                    break;
                case LogType.Warning:
                    m_LogArea.text += $"<color=yellow>{log}</color>\n";
                    break;
                case LogType.Log:
                    m_LogArea.text += $"{log}\n";
                    break;
            }
        }
    }
}
