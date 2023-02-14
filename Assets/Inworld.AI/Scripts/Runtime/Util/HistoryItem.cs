/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Packets;
namespace Inworld
{
    /// <summary>
    ///     History Item is the data class receiving from server.
    /// </summary>
    public class HistoryItem
    {
        public TextEvent Event;
        /**
         * If this is final response utterance in interaction.
         */
        public bool Final;
        public HistoryItem(TextEvent textEvent)
        {
            Event = textEvent;
        }

        public string UtteranceId => Event.PacketId.UtteranceId;
        public string InteractionId => Event.PacketId.InteractionId;
        public bool IsAgent => Event.Routing.Source.IsAgent();
    }
}
