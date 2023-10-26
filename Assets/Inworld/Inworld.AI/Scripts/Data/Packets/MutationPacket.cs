/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;

namespace Inworld.Packet
{
    [Serializable]
    public class CancelResponse
    {
        public string interactionId;
    }
    [Serializable]
    public class MutationEvent
    {
        public CancelResponse cancelResponses;
    }
    [Serializable]
    public class MutationPacket : InworldPacket
    {
        public MutationEvent mutation;
        
        public MutationPacket()
        {
            type = "MUTATION";
            mutation = new MutationEvent();
        }
        public MutationPacket(InworldPacket rhs, MutationEvent evt) : base(rhs)
        {
            type = "MUTATION";
            mutation = evt;
        }
    }
}
