/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
namespace Inworld.Util
{
    public enum StudioStatus
    {
        Initialized,
        InitFailed,
        ListAWorkspace,
        ListWorkspaceCompleted,
        ListWorkspaceFailed,
        ListAScene,
        ListSceneCompleted,
        ListSceneFailed,
        ListACharacter,
        ListCharacterCompleted,
        ListCharacterFailed,
        ListAKey,
        ListKeyCompleted,
        ListKeyFailed,
        ListSharedCharacterCompleted,
        ListSharedCharacterFailed
    }
    public enum InteractionStatus
    {
        HistoryChanged,
        InteractionCompleted
    }
    public enum RuntimeStatus
    {
        RequestError,
        InitSuccess,
        InitFailed,
        LoadSceneComplete,
        LoadSceneFailed
    }
    public enum Ownership
    {
        Owned,
        Shared,
        Default
    }
    public enum InworldEditorStatus
    {
        AppPlaying,
        Default,
        Init,
        WorkspaceChooser,
        SceneChooser,
        CharacterChooser,
        Error
    }
    public enum ControllerStates
    {
        Idle, // Initial state
        Initializing,
        InitFailed,
        Initialized, // Logged in the server with API Key/Secret or Oculus Nonce/ID
        Connecting, // Controller is connecting to World-Engine
        Connected, // Controller is connected to World-Engine and ready to work.
        LostConnect,
        Error // Some error occured.
    }

    public enum InworldSceneStatus
    {
        Unknown,
        CharacterInit,
        LoadSceneFailed
    }

    public enum AnimMainStatus
    {
        Neutral = 0,
        Hello = 1,
        Talking = 2,
        Goodbye = 3
    }

    public enum Gesture
    {
        Neutral = 0,
        Acknowledge = 1,
        Agree = 2,
        Angry = 3,
        Bore = 4,
        Celebrate = 5,
        Confuse = 6,
        Disagree = 7,
        Disgusted = 8,
        Dismissing = 9,
        Exclamation = 10,
        Fear = 11,
        FollowMe = 12,
        Greetings = 13,
        Happy = 14,
        Interested = 15,
        Question = 16,
        Rest = 17,
        Sad = 18,
        Surprise = 19,
        TellToListen = 20,
        Thank = 21,
        Think = 22,
        Point = 23
    }

    public enum Emotion
    {
        Neutral = 0,
        Angry = 1,
        Fear = 2,
        Happy = 3,
        Sad = 4
    }
}
