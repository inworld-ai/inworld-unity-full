/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using System.Collections.Generic;
using Inworld.Packets;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld.Util
{
    public class InworldKeywordListener : MonoBehaviour
    {
        [SerializeField] InworldCharacter Character;
        public List<string> keywords;
        public string trigger;
        public string triggerResponse;
        public UnityEvent OnTriggerSent;
        public UnityEvent OnTriggerReceived;

        // Start is called before the first frame update
        void Start()
        {
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
        }

        void OnPacketEvents(InworldPacket packet)
        {
            if (!InworldController.Instance.CurrentCharacter)
                return;
            string charID = InworldController.Instance.CurrentCharacter.ID;
            if (packet.Routing.Target.Id != charID && packet.Routing.Source.Id != charID)
                return;
            switch (packet)
            {
                case CustomEvent customEvent:
                    _HandleCustomEvent(customEvent);
                    break;
                case TextEvent textEvent:
                    _HandleTextEvent(textEvent);
                    break;
            }
        }

        void _HandleTextEvent(TextEvent textEvent)
        {
            if (string.IsNullOrEmpty(textEvent.Text))
                return;

            if (!keywords.Any(keyword => textEvent.Text.ToLower().Contains(keyword.ToLower())))
                return;
            Debug.Log("Sending trigger for keyword " + textEvent.Text);
            SendTrigger(trigger);
            OnTriggerSent.Invoke();
        }

        void _HandleCustomEvent(CustomEvent customEvent)
        {
            if (customEvent.TriggerName == triggerResponse)
                OnTriggerReceived.Invoke();
        }

        /// <summary>
        ///     Send target character's trigger via InworldPacket.
        /// </summary>
        /// <param name="triggerName">
        ///     The trigger to send. Both formats are acceptable.
        ///     You could send either whole string from CharacterData.trigger, or the trigger's shortName.
        /// </param>
        public void SendTrigger(string triggerName)
        {
            string[] triggerArray = triggerName.Split("triggers/");
            SendEventToAgent(triggerArray.Length == 2 ? new CustomEvent(triggerArray[1]) : new CustomEvent(triggerName));
        }

        /// <summary>
        ///     Set general events to this Character.
        /// </summary>
        /// <param name="packet">The InworldPacket to send.</param>
        public void SendEventToAgent(InworldPacket packet)
        {
            //need to confirm what id to use for scene triggers
            string ID = Character ? Character.ID : InworldController.CurrentScene.name;
            packet.Routing = Routing.FromPlayerToAgent(ID);
            InworldController.Instance.SendEvent(packet);
        }
    }
}

