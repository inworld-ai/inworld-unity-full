/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Inworld
{
    public class InworldException : Exception
    {
        public InworldException(string errorMessage) : base(errorMessage) {}
    } 
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
        static internal void Log(string msg)
        {
            Debug.Log(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        static internal void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        static internal void LogError(string msg)
        {
            Debug.LogError($"[Inworld {InworldAI.Version}] {msg}");
        }
        static internal void LogException(string exception)
        {
            throw new InworldException(exception);
        }
    }
}
