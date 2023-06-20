using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
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
        [Conditional("INWORLD_DEBUG")]
        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        public static void LogError(string msg)
        {
            Debug.LogError(msg);
        }
        public static void LogException(string exception)
        {
            throw new Exception(exception);
        }
    }
}
