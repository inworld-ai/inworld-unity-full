using Inworld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumpAudioData : MonoBehaviour
{
    [SerializeField] AudioClip m_Clip;
    [SerializeField] AudioSource m_Source;
    // Start is called before the first frame update
    void Start()
    {
        m_Source.PlayOneShot(m_Clip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
