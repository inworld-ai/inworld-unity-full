using UnityEngine;

namespace Inworld.UI
{
    public class CharacterButton : InworldUIElement
    {
        [SerializeField] InworldCharacterData m_Data;
        [SerializeField] InworldCharacter m_Char;

        public void SetData(InworldCharacterData data)
        {
            m_Data = data;
            if (data.thumbnail)
                m_Icon.texture = data.thumbnail;
        }

        public void SelectCharacter()
        {
            InworldCharacter iwChar = GetCharacter();
            if (!iwChar)
            {
                iwChar = Instantiate(m_Char, InworldController.Instance.transform);
                iwChar.transform.name = m_Data.givenName;
            }
            iwChar.Data = m_Data;
            iwChar.RegisterLiveSession();
            InworldController.Instance.CurrentCharacter = iwChar;
        }
        InworldCharacter GetCharacter()
        {
            foreach (Transform child in InworldController.Instance.transform)
            {
                InworldCharacter iwChar = child.GetComponent<InworldCharacter>();
                if (iwChar && iwChar.Data.brainName == m_Data.brainName)
                    return iwChar;
            }
            return null;
        }
    }
}
