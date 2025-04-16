/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Inworld.Packet
{
    [Serializable]
    public class LogDetail
    {
        public string text;
        public string detail;
    }
    [Serializable]
    public class LogEvent
    {
        public string text;
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel level;
        public List<LogDetail> details;
        
        public LogEvent()
        {
            text = "";
            details = new List<LogDetail>();
        }

        public LogEvent(string log, LogLevel logLevel, List<LogDetail> logDetails)
        {
            text = log;
            level = logLevel;
            details = logDetails;
        }
    }
    [Serializable]
    public class LogPacket : InworldPacket
    {
        public LogEvent log;
        
        public LogPacket()
        {
            log = new LogEvent();
        }
        public LogPacket(string text, LogLevel logLevel, List<LogDetail> logDetails = null)
        {
            log = new LogEvent(text, logLevel, logDetails);
            PreProcess();
        }
        public LogPacket(InworldPacket rhs, LogEvent evt) : base(rhs)
        {
            log = evt;
        }
        public void Display()
        {
            if (log == null)
                return;
            switch (log.level)
            {
                case LogLevel.WARNING:
                    Debug.LogWarning($"[WARNING] {log.text}");
                    break;
                case LogLevel.INFO:
                    Debug.Log($"[INFO] {log.text}");
                    break;
                case LogLevel.DEBUG:
                    Debug.LogWarning($"[DEBUG] {log.text}");
                    break;
            }
        }
    }


}