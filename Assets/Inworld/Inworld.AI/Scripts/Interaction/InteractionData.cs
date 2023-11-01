/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Collections.Generic;
using System.Linq;
using Inworld.Packet;

namespace Inworld.Interactions
{
    /// <summary>
    /// In Inworld. Interaction contains several utterances (sentences).
    /// And Each utterance contains several packets. (Text / Audio / Emotion Change / etc)
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// Gets/Sets the interaction ID. 
        /// </summary>
        public string InteractionID { get; set; }
        /// <summary>
        /// Gets/Sets this interaction's utterances.
        /// </summary>
        public List<Utterance> Utterances { get; set; } = new List<Utterance>();
        /// <summary>
        /// Gets/Sets the status of this interaction.
        /// </summary>
        public InteractionStatus Status { get; set; }
        /// <summary>
        /// Gets/Sets if this interaction has finished from the server message.
        /// </summary>
        public bool ReceivedInteractionEnd { get; set; }
        /// <summary>
        /// Gets/Sets the index of this interaction in History items.
        /// </summary>
        public int SequenceNumber { get; set; }
        /// <summary>
        /// Use interaction ID to navigate the actual utterance (Nullable)
        /// </summary>
        /// <param name="utteranceID"></param>
        public Utterance this[string utteranceID] => Utterances.FirstOrDefault(u => u.UtteranceID == utteranceID);
        /// <summary>
        /// Create an Interaction by ID and index
        /// </summary>
        /// <param name="interactionID"></param>
        /// <param name="sequenceNumber"></param>
        public Interaction(string interactionID, int sequenceNumber)
        {
            InteractionID = interactionID;
            SequenceNumber = sequenceNumber;
            Status = InteractionStatus.CREATED;
        }
        /// <summary>
        /// Update the status of the interaction.
        /// </summary>
        public void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (var utterance in Utterances)
            {
                utterance.UpdateStatus();
                if (utterance.Status != InteractionStatus.COMPLETED)
                    isComplete = false;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }
        /// <summary>
        /// Interrupt the current interaction.
        /// </summary>
        public void Cancel()
        {
            foreach (var utterance in Utterances)
            {
                if (utterance.Status != InteractionStatus.COMPLETED)
                    utterance.Cancel();
            }
            Status = InteractionStatus.CANCELLED;
        }
    }

    /// <summary>
    /// The utterance that contains both text and audio.
    /// </summary>
    public class AudioUtterance : Utterance
    {
        /// <summary>
        /// Generate an AudioUtterance.
        /// </summary>
        /// <param name="interaction">the ID of the interaction that contains this utterance.</param>
        /// <param name="utteranceID">its utterance ID</param>
        public AudioUtterance(Interaction interaction, string utteranceID) : base(interaction, utteranceID) {}
        
        /// <summary>
        /// Update the current status.
        /// </summary>
        public override void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (InworldPacket packet in Packets)
            {
                if (packet is TextPacket || packet is AudioPacket)
                {
                    if (packet.packetId.Status != PacketStatus.PLAYED)
                        isComplete = false;
                }
                else
                {
                    if (packet.packetId.Status != PacketStatus.PROCESSED)
                        isComplete = false;
                }
                if (!isComplete)
                    break;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }
        /// <summary>
        /// Cancel this utterance.
        /// </summary>
        public override void Cancel()
        {
            foreach (InworldPacket packet in Packets)
            {
                if (packet is TextPacket || packet is AudioPacket)
                {
                    if (packet.packetId.Status != PacketStatus.PLAYED)
                        packet.packetId.Status = PacketStatus.CANCELLED;
                }
                else
                {
                    if (packet.packetId.Status != PacketStatus.PROCESSED)
                        packet.packetId.Status = PacketStatus.CANCELLED;
                }
            }
            Status = InteractionStatus.CANCELLED;
        }
    }
    /// <summary>
    /// The general utterance class used to process packets of a sentence.
    /// </summary>
    public class Utterance
    {
        /// <summary>
        /// Gets/Sets the interaction this utterance belongs to.
        /// </summary>
        public Interaction Interaction { get; set; }
        /// <summary>
        /// Gets/Sets the ID of the utterance.
        /// </summary>
        public string UtteranceID { get; set; }
        /// <summary>
        /// Gets/Sets the packets inside this utterance.
        /// </summary>
        public List<InworldPacket> Packets { get; set; } = new List<InworldPacket>();
        /// <summary>
        /// Gets/Sets the status of the utterance.
        /// </summary>
        public InteractionStatus Status { get; set; }
        /// <summary>
        /// Create an utterance by interaction and utterance ID
        /// </summary>
        /// <param name="interaction">the interaction it belongs to</param>
        /// <param name="utteranceID">the id of this utterance.</param>
        public Utterance(Interaction interaction, string utteranceID)
        {
            Interaction = interaction;
            UtteranceID = utteranceID;
            Status = InteractionStatus.CREATED;
        }
        /// <summary>
        /// Gets the text packet of this utterance.
        /// </summary>
        /// <returns></returns>
        public TextPacket GetTextPacket()
        {
            return Packets.FirstOrDefault(packet => packet is TextPacket) as TextPacket;
        }
        /// <summary>
        /// Gets its audio packet 
        /// </summary>
        /// <returns></returns>
        public AudioPacket GetAudioPacket()
        {
            return Packets.FirstOrDefault(packet => packet is AudioPacket) as AudioPacket;
        }
        /// <summary>
        /// Gets the status of this utterance. 
        /// </summary>
        public virtual void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (InworldPacket packet in Packets)
            {
                if (packet is TextPacket)
                {
                    if (packet.packetId.Status != PacketStatus.PLAYED)
                        isComplete = false;
                }
                else
                {
                    if (packet.packetId.Status != PacketStatus.PROCESSED)
                        isComplete = false;
                }
                if (!isComplete)
                    break;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }
        /// <summary>
        /// Cancel this utterance.
        /// </summary>
        public virtual void Cancel()
        {
            foreach (InworldPacket packet in Packets)
            {
                if (packet is TextPacket)
                {
                    if (packet.packetId.Status != PacketStatus.PLAYED)
                        packet.packetId.Status = PacketStatus.CANCELLED;
                }
                else
                {
                    if (packet.packetId.Status != PacketStatus.PROCESSED)
                        packet.packetId.Status = PacketStatus.CANCELLED;
                }
            }
            Status = InteractionStatus.CANCELLED;
        }
    }
}
