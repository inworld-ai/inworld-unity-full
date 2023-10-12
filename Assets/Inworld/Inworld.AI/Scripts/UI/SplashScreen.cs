/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample
{
    public class SplashScreen : SingletonBehavior<SplashScreen>
    {
        Canvas m_Canvas;
        Image m_BG;
        [SerializeField] GameObject m_DlgError;
        [SerializeField] TMP_Text m_MainText;
        [SerializeField] TMP_Text m_HintText;
        [SerializeField] TMP_Text m_Title;
        [SerializeField] TMP_Text m_Content;
        [SerializeField] string m_TitleExhaust;
        [SerializeField] string m_TitleError;
        [SerializeField] string m_ContentExhaust;
        
        // Start is called before the first frame update
        void Awake()
        {
            enabled = Init();
        }
        bool Init()
        {
            if (!m_Canvas)
                m_Canvas = GetComponent<Canvas>();
            if (!m_BG)
                m_BG = GetComponent<Image>();
            return m_Canvas && m_BG && m_DlgError && m_MainText && m_HintText && m_Title && m_Content;
        }
        void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
        }

        // Update is called once per frame
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }

        void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (m_BG.color.a > 0)
                m_MainText.text = newStatus.ToString();
            else
                m_HintText.text = newStatus.ToString();
            if (newStatus == InworldConnectionStatus.Connected)
                StartCoroutine(FadeOut());
            else
            {
                m_Canvas.enabled = true;
                switch (newStatus)
                {
                    case InworldConnectionStatus.LostConnect:
                        m_HintText.text = "Reconnecting...";
                        break;
                    case InworldConnectionStatus.Exhausted:
                        _SetDialog(m_TitleExhaust, m_ContentExhaust);
                        break;
                    case InworldConnectionStatus.Error:
                    {
                        //TODO(Yan): Instantiate InworldError Class and deserialize.
                        _SetDialog(m_TitleError, InworldController.Client.Error);
                        break;
                    }
                }
            }
        }

        IEnumerator FadeIn()
        {
            while (m_BG.color.a < 1)
            {
                Color color = m_BG.color;
                color.a += Time.deltaTime;
                m_BG.color = color;
                yield return new WaitForFixedUpdate();
            }
        }
        
        IEnumerator FadeOut()
        {
            while (m_BG.color.a > 0)
            {
                Color color = m_BG.color;
                color.a -= Time.deltaTime;
                m_BG.color = color;
                yield return new WaitForFixedUpdate();
            }
            m_HintText.text = m_MainText.text = "";
            gameObject.SetActive(false);
        }
        public void ExitApp()
        {
            //InworldController.Instance.StartTerminate();
            gameObject.SetActive(false);
        }
        void _SetDialog(string title, string content)
        {
            m_DlgError.SetActive(true);
            m_Title.text = title;
            m_Content.text = content;
        }
    }
}

