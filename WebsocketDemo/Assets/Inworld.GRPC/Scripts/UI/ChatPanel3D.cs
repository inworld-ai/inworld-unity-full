using Inworld;
using Inworld.Assets;
using Inworld.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class ChatPanel3D : MonoBehaviour
{
    public InworldCharacter Character;
    [SerializeField] RectTransform m_ContentRT;
    [SerializeField] ChatBubble m_BubbleLeft;
    [SerializeField] ChatBubble m_BubbleRight;
    [SerializeField] Image m_EmoIcon;
    readonly protected Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();
    protected string m_CurrentEmotion;
    [SerializeField] FacialAnimationData m_FaceData;
    void OnEnable()
    {
        Character = GetComponentInParent<InworldCharacter>();
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }

    void OnDisable()
    {
        if (!InworldController.Instance)
            return;
        InworldController.Instance.OnCharacterInteraction -= OnInteraction;
    }
    protected void OnInteraction(InworldPacket incomingPacket)
    {
        if (incomingPacket.routing.source.name == Character.Data.agentId || incomingPacket.routing.target.name == Character.Data.agentId)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
                    break;
                case TextPacket textPacket:
                    HandleText(textPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
                case CustomPacket customPacket:
                    break;
                default:
                    InworldAI.Log($"Received {incomingPacket}");
                    break;
            }
        }
    }
    void HandleTrigger(CustomPacket customPacket)
    {
        throw new System.NotImplementedException();
    }
    void HandleEmotion(EmotionPacket emotionPacket)
    {
        switch (emotionPacket.emotion.behavior.ToUpper())
        {
            case "AFFECTION":
            case "INTEREST":
                _ProcessEmotion("Anticipation");
                break;
            case "HUMOR":
            case "JOY":
                _ProcessEmotion("Joy");
                break;
            case "CONTEMPT":
            case "CRITICISM":
            case "DISGUST":
                _ProcessEmotion("Disgust");
                break;
            case "BELLIGERENCE":
            case "DOMINEERING":
            case "ANGER":
                _ProcessEmotion("Anger");
                break;
            case "TENSION":
            case "STONEWALLING":
            case "TENSEHUMOR":
            case "DEFENSIVENESS":
                _ProcessEmotion("Fear");
                break;
            case "WHINING":
            case "SADNESS":
                _ProcessEmotion("Sadness");
                break;
            case "SURPRISE":
                _ProcessEmotion("Surprise");
                break;
            default:
                _ProcessEmotion("Neutral");
                break;
        }
    }
    void _ProcessEmotion(string emotion)
    {
        FacialAnimation targetEmo = m_FaceData.emotions.FirstOrDefault(emo => emo.emotion == emotion);
        
        if (targetEmo != null)
        {
            m_EmoIcon.sprite = targetEmo.icon;
        }
    }
    protected virtual void HandleText(TextPacket packet)
    {
        if (packet.text == null || string.IsNullOrEmpty(packet.text.text))
            return;
        switch (packet.routing.source.type.ToUpper())
        {
            case "AGENT":
                if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                {
                    m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeft, m_ContentRT);
                    InworldCharacterData charData = InworldController.Instance.GetCharacter(packet.routing.source.name);
                    if (charData != null)
                    {
                        string charName = charData.givenName ?? "Character";
                        string title = $"{charName}: {m_CurrentEmotion}";
                        Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                        m_Bubbles[packet.packetId.utteranceId].SetBubble(title, thumbnail);
                    }
                }
                break;
            case "PLAYER":
                if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                {
                    m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleRight, m_ContentRT);
                    m_Bubbles[packet.packetId.utteranceId].SetBubble(InworldAI.User.Name, InworldAI.DefaultThumbnail);
                }
                break;
        }
        m_Bubbles[packet.packetId.utteranceId].Text = packet.text.text;
        _SetContentHeight(m_ContentRT, m_BubbleRight);
    }
    void _SetContentHeight(RectTransform scrollAnchor, InworldUIElement element)
    {
        scrollAnchor.sizeDelta = new Vector2(m_ContentRT.sizeDelta.x, scrollAnchor.childCount * element.Height);
    }
}