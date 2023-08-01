using Inworld.Interactions;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldRPMCharacter : InworldCharacter
    {
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter == null)
            {
                InworldController.Instance.CurrentCharacter = this;
                InworldController.Instance.StartAudio();
            }
        }
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
    }
    
}

