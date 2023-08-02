using UnityEngine;
using Inworld.Interactions;


namespace Inworld.Sample.Innequin
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter3D : InworldCharacter
    {
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }

        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected)
                InworldController.Instance.CurrentCharacter = this;
        }
    }
}
