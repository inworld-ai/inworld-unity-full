/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Inworld.Packet;
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
        readonly List<LogDetail> m_InworldLogs = new();
        void Awake()
        {
            Application.logMessageReceived += OnUnityLogReceived;
            if (InworldController.Client)
                InworldController.Client.OnLogReceived += OnInworldLogReceived;
        }

        void OnDisable()
        {
            if (InworldController.Client)
                InworldController.Client.OnLogReceived -= OnInworldLogReceived;
            Application.logMessageReceived -= OnUnityLogReceived;
        }

        void OnInworldLogReceived(LogPacket logPacket)
        {
            if (logPacket?.log == null)
                return;
            m_InworldLogs.AddRange(logPacket.log.details);
            if (!InworldAI.IsDebugMode)
                return;
            foreach (LogDetail detail in logPacket.log.details)
            {
                m_LogArea.text += $"Text: {detail.text} Detail: {detail.detail}\n";
            }
        }

        void OnUnityLogReceived(string log, string backTrace, LogType type)
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
        /// <summary>
        /// Get the logs from Inworld server.
        /// </summary>
        public List<LogDetail> InworldLogs => m_InworldLogs;
        
        [Conditional("INWORLD_DEBUG")]
        internal static void Log(string msg)
        {
            Debug.Log(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        internal static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        internal static void LogError(string msg)
        {
            Debug.LogError($"[Inworld {InworldAI.Version}] {msg}");
        }
        internal static void LogException(string exception)
        {
            throw new InworldException(exception);
        }
    }
}
