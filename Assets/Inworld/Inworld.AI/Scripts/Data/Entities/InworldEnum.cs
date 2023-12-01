/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

namespace Inworld
{
    public enum InworldConnectionStatus
    {
        Idle, // Initial state
        Initializing,
        InitFailed,
        Initialized, // Logged in the server with API Key/Secret or Oculus Nonce/ID
        LoadingScene,
        LoadingSceneCompleted,
        Connecting, // Controller is connecting to World-Engine
        Connected, // Controller is connected to World-Engine and ready to work.
        LostConnect,
        Exhausted,
        Error // Some error occured.
    }

    public enum PacketType
    {
        UNKNOWN,
        TEXT,
        CONTROL,
        AUDIO,
        GESTURE,
        CUSTOM,
        CANCEL_RESPONSE,
        EMOTION,
        ACTION,
        RELATION
    }
    public enum PacketStatus
    {
        RECEIVED,
        PROCESSED,
        PLAYED,
        CANCELLED
    }

    public enum MicSampleMode
    {
        NO_MIC,
        NO_FILTER,
        PUSH_TO_TALK,
        AEC,
        TURN_BASED
    }
}
