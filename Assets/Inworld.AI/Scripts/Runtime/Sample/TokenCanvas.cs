using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample
{
    public class TokenCanvas : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_TokenInput;

        // Start is called before the first frame update
        void Start() {}

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                SendToken();
                gameObject.SetActive(false);
            }
        }
        public void SendToken()
        {
            if (string.IsNullOrEmpty(m_TokenInput.text))
            {
                Debug.LogError("Token Incorrect!");
                return;
            }
            InworldController.Instance.Init(m_TokenInput.text);
            gameObject.SetActive(false);
        }
    }
}