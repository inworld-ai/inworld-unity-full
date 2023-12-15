/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;

namespace Inworld.Entities
{
    [Serializable]
    public enum PreviousTalker
    {
        UNKNOWN,
        PLAYER,
        CHARACTER
    }
    [Serializable]
    public class PreviousSessionResponse
    {
        public string state;
        public string creationTime;
    }
    [Serializable]
    public class SessionContinuation
    {
        public PreviousDialog previousDialog;
        public string previousState;
    }
    [Serializable]
    public class PreviousDialog
    {
        public PreviousDialogPhrase[] phrases;
    }
    [Serializable]
    public class PreviousDialogPhrase
    {
        public PreviousTalker talker; 
        public string phrase;
    }
    [Serializable]
    public class SessionContinuationContinuationInfo
    {
        public string millisPassed;
    }

}
