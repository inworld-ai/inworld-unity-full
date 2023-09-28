using Inworld.Interactions;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldRPMCharacter : InworldCharacter
    {
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
    }
    
}

