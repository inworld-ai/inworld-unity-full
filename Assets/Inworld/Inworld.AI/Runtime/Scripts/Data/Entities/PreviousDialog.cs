/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace Inworld.Entities
{
#region Legacy
    [Serializable][Obsolete]
    public enum PreviousTalker
    {
        UNKNOWN,
        PLAYER,
        CHARACTER
    }
    [Serializable][Obsolete]
    public class SessionContinuation
    {
        public PreviousDialog previousDialog;
        public string previousState;
    }
    [Serializable][Obsolete]
    public class PreviousDialog
    {
        public PreviousDialogPhrase[] phrases;
    }
    [Serializable][Obsolete]
    public class PreviousDialogPhrase
    {
        public PreviousTalker talker; 
        public string phrase;
    }
    [Serializable][Obsolete]
    public class SessionContinuationContinuationInfo
    {
        public string millisPassed;
    }
#endregion 

#region New
    [Serializable]
    public class PreviousSessionResponse
    {
        public string state;
        public string creationTime;
    }
    [Serializable]
    public class ContinuationInfo
    {
        string passedTime;
    }
    [Serializable]
    public enum ContinuationType 
    {
        CONTINUATION_TYPE_UNKNOWN = 0,
        CONTINUATION_TYPE_EXTERNALLY_SAVED_STATE = 1,
        CONTINUATION_TYPE_DIALOG_HISTORY = 2
    }
    [Serializable]
    public class HistoryItem
    {
        public Source actor;
        public string text;
    }
    [Serializable]
    public class DialogHistory
    {
        public List<HistoryItem> history;
    }
    [Serializable]
    public class Continuation
    {
        public ContinuationInfo continuationInfo;
        // Required
        // Contains type of continuation.
        public ContinuationType continuationType;
        // Dialog that was before starting with existing conversation.
        public DialogHistory dialogHistory;
        // State received from server to use later for session continuation.
        // The state sent in compressed and encrypted format.
        // Client receives it in bytearray format that's why it is not strongly typed.
        // But it is strongly typed on server side and can be deserialized to ExternallySavedState.
        // Client should not modify this state!
        public string externallySavedState;

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                switch (continuationType)
                {
                    case ContinuationType.CONTINUATION_TYPE_DIALOG_HISTORY:
                        return dialogHistory?.history?.Count > 0;
                    case ContinuationType.CONTINUATION_TYPE_EXTERNALLY_SAVED_STATE:
                        return !string.IsNullOrEmpty(externallySavedState);
                    default:
                        return false;
                }
            }
        }
        [JsonIgnore]
        public ContinuationPacket ToPacket => new ContinuationPacket
        {
            timestamp = InworldDateTime.UtcNow,
            packetId = new PacketId(),
            routing = new Routing("WORLD"),
            sessionControl = new ContinuationEvent
            {
                continuation = this
            }
        };
    }
    [Serializable]
    public class ContinuationEvent
    {
        public Continuation continuation;
    }
    [Serializable]
    public class ContinuationPacket : InworldPacket
    {
        public ContinuationEvent sessionControl;
    }

  #endregion
}
