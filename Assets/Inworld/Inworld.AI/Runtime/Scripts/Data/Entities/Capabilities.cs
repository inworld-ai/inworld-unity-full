/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;

namespace Inworld.Entities
{
    [Serializable]
    public class Capabilities
    {
        public bool audio;
        public bool emotions;
        public bool interruptions;
        public bool narratedActions;
        public bool regenerateResponse;
        public bool text;
        public bool triggers;
        public bool phonemeInfo;
        public bool relations;
        public bool debugInfo;
        public bool multiAgent;
        public bool multiModalActionPlanning;
        public bool pingPongReport;
        public bool perceivedLatencyReport;
        public bool logs = true;
        public bool logs_warning = true;
        public bool logs_info = true;
        public bool logs_debug;

        public Capabilities() {}
        public Capabilities(Capabilities rhs)
        {
            audio = rhs.audio;
            emotions = rhs.emotions;
            interruptions = rhs.interruptions;
            narratedActions = rhs.narratedActions;
            regenerateResponse = rhs.regenerateResponse;
            text = rhs.text;
            triggers = rhs.triggers;
            phonemeInfo = rhs.phonemeInfo;
            relations = rhs.relations;
            debugInfo = rhs.debugInfo;
            multiAgent = rhs.multiAgent;
            multiModalActionPlanning = rhs.multiModalActionPlanning;
            pingPongReport = rhs.pingPongReport;
            perceivedLatencyReport = rhs.perceivedLatencyReport;
            logs = rhs.logs;
            logs_info = rhs.logs_info;
            logs_debug = rhs.logs_debug;
            logs_warning = rhs.logs_warning;
        }
        public void CopyFrom(Capabilities rhs)
        {
            audio = rhs.audio;
            emotions = rhs.emotions;
            interruptions = rhs.interruptions;
            narratedActions = rhs.narratedActions;
            regenerateResponse = rhs.regenerateResponse;
            text = rhs.text;
            triggers = rhs.triggers;
            phonemeInfo = rhs.phonemeInfo;
            relations = rhs.relations;
            debugInfo = rhs.debugInfo;
            multiAgent = rhs.multiAgent;
            multiModalActionPlanning = rhs.multiModalActionPlanning;
            pingPongReport = rhs.pingPongReport;
            perceivedLatencyReport = rhs.perceivedLatencyReport;
            logs = rhs.logs;
            logs_info = rhs.logs_info;
            logs_debug = rhs.logs_debug;
            logs_warning = rhs.logs_warning;
        }
        public override string ToString()
        {
            string result = "";
            if (audio)
                result += "AUDIO ";
            if (emotions)
                result += "EMOTIONS ";
            if (interruptions)
                result += "INTERRUPTIONS ";
            if (narratedActions)
                result += "NARRATIVE ";
            if (regenerateResponse)
                result += "REGENERATE_RESPONSE ";
            if (text)
                result += "TEXT ";
            if (triggers)
                result += "TRIGGER ";
            if (phonemeInfo)
                result += "PHONEME ";
            if (relations)
                result += "RELATIONS ";
            if (multiAgent)
                result += "MULTI_AGENT ";
            if (multiModalActionPlanning)
                result += "MAP ";
            if (pingPongReport)
                result += "PING_PONG ";
            if (perceivedLatencyReport)
                result += "PERCEIVED_LATENCY ";
            if (logs)
                result += "LOGS ";
            if (logs_info)
                result += "LOGS INFO ";
            if (logs_debug)
                result += "LOGS DEBUG ";
            if (logs_warning)
                result += "LOGS WARNING ";
            return result;
        }
    }
}
