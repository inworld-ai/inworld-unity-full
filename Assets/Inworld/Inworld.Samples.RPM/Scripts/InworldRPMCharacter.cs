using Inworld.Interactions;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldRPMCharacter : InworldCharacter
    {
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus != InworldConnectionStatus.Connected || InworldController.Instance.CurrentCharacter)
                return;
            InworldController.Instance.CurrentCharacter = this;
        }
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
    }
    
}

