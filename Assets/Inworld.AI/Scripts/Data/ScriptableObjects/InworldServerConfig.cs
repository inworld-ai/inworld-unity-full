using UnityEngine;

namespace Inworld
{
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "Inworld/ServerConfig", order = 0)]
    public class InworldServerConfig : ScriptableObject
    {
        [Header("Server Info:")]
        public string studio;
        public string runtime;
        public string token;
        public string web;
        public string tutorialPage;
        public int port;
        public string TokenServer => $"https://{web}/{k_TokenURL}"; 
        const string k_SessionURL = "v1/session/default?session_id=";
        const string k_TokenURL = "v1/sessionTokens/token:generate";
        public string RuntimeServer => $"{runtime}:{port}";
        public string StudioServer => $"{studio}:{port}";
        
        public string LoadSceneURL(string sceneFullName) => $"https://{web}/v1/{sceneFullName}:load";
        
        public string SessionURL(string sessionID) => $"wss://{web}/{k_SessionURL}{sessionID}";
    }
}
