using Inworld;
using Inworld.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InworldMultiAgent : MonoBehaviour
{
    const string nextTurn = "inworld.conversation.next_turn";
    List<string> m_AgentID = new List<string>();
        
    void OnEnable()
    {
        InworldController.Client.OnStatusChanged += OnStatusChanged;
        InworldController.CharacterHandler.OnCharacterRegistered += OnCharacterRegistered;
    }
    void OnDisable()
    {
        if (!InworldController.Instance)
            return;
        InworldController.Client.OnStatusChanged -= OnStatusChanged;
        InworldController.CharacterHandler.OnCharacterRegistered -= OnCharacterRegistered;
    }
    
    public void NextTurn(InworldCharacter character)
    {
        character.SendTrigger(nextTurn);
    }
    void OnStatusChanged(InworldConnectionStatus status)
    {
        if (status == InworldConnectionStatus.Connected)
        {
            foreach (var agent in m_AgentID)
            {
                InworldController.Instance.SendTrigger(nextTurn, agent);
            }
        }
    }
    void OnCharacterRegistered(InworldCharacterData data)
    {
        m_AgentID.Add(data.agentId);
    }
}
