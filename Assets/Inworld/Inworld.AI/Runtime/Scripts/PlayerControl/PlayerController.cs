/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Inworld.Sample
{
    /// <summary>
    /// This is the demo use case for how to interact with Inworld.
    /// For developers please feel free to create your own.
    /// </summary>
    public class PlayerController : SingletonBehavior<PlayerController>
    {
        protected int m_CurrentUILayers;
        public UnityEvent<string> onPlayerSpeaks;
        public UnityEvent onCanvasOpen; 
        public UnityEvent onCanvasClosed;

        public int UILayer
        {
            get => m_CurrentUILayers;
            set
            {
                m_CurrentUILayers = value;
                if (m_CurrentUILayers > 0)
                {
                    InworldController.Audio.StopVoiceDetecting();
                    onCanvasOpen?.Invoke();
                }
                else
                {
                    InworldController.Audio.StartVoiceDetecting();
                    onCanvasClosed?.Invoke();
                }
            }
        }
        
        protected virtual void Update()
        {
            HandleInput();
        }
        public virtual void OpenContextEditing(string interactionID, string correlationID)
        {
            
        }

        protected virtual void HandleInput()
        {

        }
    }
}

