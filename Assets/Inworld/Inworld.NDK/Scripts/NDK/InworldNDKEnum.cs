/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
namespace Inworld.NDK
{
    public static class InworldNDKEnum
    {
        static internal string GetEmotion(int nEmotionCode)
        {
            switch (nEmotionCode) 
            {
                case 0: return "NEUTRAL";
                case 1: return "DISGUST";
                case 2: return "CONTEMPT";
                case 3: return "BELLIGERENCE";
                case 4: return "DOMINEERING";
                case 5: return "CRITICISM";
                case 6: return "ANGER";
                case 7: return "TENSION";
                case 8: return "TENSE_HUMOR";
                case 9: return "DEFENSIVENESS";
                case 10: return "WHINING";
                case 11: return "SADNESS";
                case 12: return "STONEWALLING";
                case 13: return "INTEREST";
                case 14: return "VALIDATION";
                case 15: return "AFFECTION";
                case 16: return "HUMOR";
                case 17: return "SURPRISE";
                case 18: return "JOY";
                default: return "UNKNOWN"; 
            }
        }
        static internal string GetStrength(int nStrength)
        {
            switch (nStrength) 
            {
                case 0: return "UNSPECIFIED";
                case 1: return "WEAK";
                case 2: return "STRONG";
                case 3: return "NORMAL";
                default: return "UNKNOWN"; 
            }
        }
        static internal string GetAction(int code) 
        {
            switch (code) 
            {
                case 0: return "UNKNOWN";
                case 1: return "AUDIO_SESSION_START";
                case 2: return "AUDIO_SESSION_END";
                case 3: return "INTERACTION_END";
                case 4: return "TTS_PLAYBACK_START";
                case 5: return "TTS_PLAYBACK_END";
                case 6: return "TTS_PLAYBACK_MUTE";
                case 7: return "TTS_PLAYBACK_UNMUTE";
                case 8: return "WARNING";
                case 9: return "SESSION_END";
                default: return "INVALID";
            }
        }
    }
}
