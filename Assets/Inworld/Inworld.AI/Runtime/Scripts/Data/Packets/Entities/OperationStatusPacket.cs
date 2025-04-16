/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;

namespace Inworld.Packet
{
    [Serializable]
    public class OperationStatusPacket : InworldPacket
    {
        public OperationStatusEvent operationStatus;

        public OperationStatusPacket()
        {

        }
    }

    [Serializable]
    public class OperationStatusEvent
    {
        public Status status;
    }

    [Serializable]
    public class Status
    {
        public int code;
        public string message;

        public override string ToString() => $"Operation Status: {code} : {message}";
    }
}
