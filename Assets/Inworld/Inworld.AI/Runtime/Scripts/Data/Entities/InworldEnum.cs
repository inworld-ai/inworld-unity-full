/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

namespace Inworld
{
    public enum Role
    {
        // Unspecified role. Do not use this, it will be responded with an error when specified as input.
        ROLE_UNSPECIFIED = 0,
        // The owner of the workspace.
        ROLE_OWNER = 1,
        // The admin of the workspace.
        ROLE_ADMIN = 2,
        // The developer of the workspace.
        ROLE_DEVELOPER = 3,
        // The viewer of the workspace.
        ROLE_VIEWER = 4
    }
    public enum InworldConnectionStatus
    {
        Idle, // Initial state
        Initializing, // Used at getting runtime token.
        Initialized, // Getting runtime Token Completed. 
        Connecting, // Start Session with Inworld Server by runtime token.
        Connected, // Controller is connected to World-Engine and ready to work.
        Exhausted, // Received when user is running out of quota.
        Error // Some error occured.
    }

    public enum InworldMessage
    {
        None,
        GoalEnable,
        GoalDisable,
        GoalComplete,
        RelationUpdate,
        ConversationNextTurn,
        Uninterruptible,
        Error,
        Critical,
        GoAway,
        IncompleteInteraction,
        Task
    }

    public enum LogLevel
    {
        // Log level was not set for the given log entry. Defaults to WARNING.
        UNSPECIFIED = 0,
        // Important log messages that usually indicate a developer issue that needs resolution.
        // For example, a goal function mutation failed due to an invalid value.
        WARNING = 1,
        // Informational logs provide the developer with general information about the system's state.
        // For example, information about completed goals in the current turn.
        INFO = 2,
        // Detailed information needed primarily for debugging specific issues.
        // For example, information about changes in variable values.
        DEBUG = 3
    }
    public enum ErrorType
    {
        UNDEFINED = -2,
        CLIENT_ERROR = -1,
        SESSION_TOKEN_EXPIRED = 0,
        // Session token is completely invalid
        SESSION_TOKEN_INVALID = 1,
        // Session's resources are temporarily exhausted
        SESSION_RESOURCES_EXHAUSTED = 2,
        // Billing tokens are exhausted -- client should buy more time or wait till end of billing period
        BILLING_TOKENS_EXHAUSTED = 3,
        // Developer account is completely disabled, either due to a ToS violation or for some other reason
        ACCOUNT_DISABLED = 4,
        // Session is invalid due to missing agents or some other reason
        SESSION_INVALID = 5,
        // Resource id is invalid or otherwise could not be found
        RESOURCE_NOT_FOUND = 6,
        // Safety policies have been violated
        SAFETY_VIOLATION = 7,
        // The session has timed out due to inactivity
        SESSION_EXPIRED = 8,
        // The audio session has timed out due to exceeding the maximum duration supported by the Audio Processor.
        AUDIO_SESSION_EXPIRED = 9,
        // The session has been paused due to inactivity
        SESSION_PAUSED = 10,
        // The entity could not be updated because the client supplied a stale version
        VERSION_CONFLICT = 11
    }
    public enum ReconnectionType
    {
        UNDEFINED = 0,
        // Client should not try to reconnect
        NO_RETRY = 1,
        // Client can try to reconnect immediately
        IMMEDIATE = 2,
        // Client can try to reconnect after given period, specified in InworldStatus.reconnect_time
        TIMEOUT = 3
    }
    public enum FeedbackType
    {
        INTERACTION_DISLIKE_TYPE_UNSPECIFIED = 0,
        // The content is irrelevant
        INTERACTION_DISLIKE_TYPE_IRRELEVANT = 1,
        // The content is unsafe
        INTERACTION_DISLIKE_TYPE_UNSAFE = 2,
        // The content is untrue
        INTERACTION_DISLIKE_TYPE_UNTRUE = 3,
        // The content uses knowledge incorrectly
        INTERACTION_DISLIKE_TYPE_INCORRECT_USE_KNOWLEDGE = 4,
        // The content contains unexpected action
        INTERACTION_DISLIKE_TYPE_UNEXPECTED_ACTION = 5,
        // The content contains unexpected goal behaviour
        INTERACTION_DISLIKE_TYPE_UNEXPECTED_GOAL_BEHAVIOR = 6,
        // The content contains repetition issue
        INTERACTION_DISLIKE_TYPE_REPETITION = 7
    }
    public enum CustomType 
    {
        UNSPECIFIED = 0,
        TRIGGER = 1,
        TASK = 2
    }
    public enum MicrophoneMode
    {
        UNSPECIFIED,
        OPEN_MIC, // For auto push
        EXPECT_AUDIO_END // For push to talk
    }
    public enum UnderstandingMode
    {
        // If UNSPECIFIED_UNDERSTANDING_MODE, the server will assume FULL mode.
        UNSPECIFIED_UNDERSTANDING_MODE = 0,
        // Once speech recognition results are final, automatically trigger the understanding part of the backend stack.
        // Recognition results are also sent to the client.
        FULL = 1,
        // Do not trigger the understanding part; only send the speech recognition results to the client.
        SPEECH_RECOGNITION_ONLY = 2
    }
    // Types of access control policy in Runtime
    public enum RuntimeAccess 
    {
        // Unspecified
        RUNTIME_ACCESS_UNSPECIFIED = 0,
        // Default policy - only the owner can read assets
        RUNTIME_ACCESS_PRIVATE = 1,
        // Public - everyone can read assets
        RUNTIME_ACCESS_PUBLIC = 2
    }
    public enum Pronoun 
    {
        // No pronoun specified / unknown
        PRONOUN_UNSPECIFIED = 0,
        // She/Her/Hers
        PRONOUN_FEMALE = 1,
        // He/Him/His
        PRONOUN_MALE = 2,
        // They/Them/Theirs
        PRONOUN_OTHER = 3
    }
    public enum PingPongType 
    {
        // No type is specified, means this is empty report.
        UNSPECIFIED = 0,
        // Sent from the server to the client.
        PING = 1,
        // Upon receiving a ping, the client has to send back a pong packet.
        PONG = 2
    }
    public enum Precision 
    {
        // Precision is not specified.
        UNSPECIFIED = 0,
        // Measured based on local Voice Activity Detection.
        FINE = 1,
        // Measured based on the client assuming when the user started to speak (e.g., using speech recognition results).
        ESTIMATED = 2,
        // Measured from the moment the player released the push-to-talk button.
        PUSH_TO_TALK = 3,
        // Measured from sending a text or a trigger.
        NON_SPEECH = 4
    }
    // List of life stages for character.
    public enum LifeStage 
    {
        LIFE_STAGE_UNSPECIFIED = 0,
        LIFE_STAGE_ADOLESCENCE = 1,
        LIFE_STAGE_YOUNG_ADULTHOOD = 2,
        LIFE_STAGE_MIDDLE_ADULTHOOD = 3,
        LIFE_STAGE_LATE_ADULTHOOD = 4,
        LIFE_STAGE_CHILDHOOD = 5
    }
    public enum ControlType
    {
        UNKNOWN = 0,
        // Speech activity starts, server should expect DataChunk, TextEvent and
        // EmotionEvent packets after that.
        AUDIO_SESSION_START = 1,
        // Speech activity ended.
        AUDIO_SESSION_END = 2,
        // Indicates that the server has already sent all TextEvent response packets for the given interaction, and there won't be any more. 
        // Other types of packets can still be received by the client after it has received this packet.
        INTERACTION_END = 3,
        // TTS response playback starts on the client.
        TTS_PLAYBACK_START = 4,
        // TTS Response playback ends on the client.
        TTS_PLAYBACK_END = 5,
        // TTS response playback is muted on the client.
        TTS_PLAYBACK_MUTE = 6,
        // TTS response playback is unmuted on the client.
        TTS_PLAYBACK_UNMUTE = 7,
        // Contains warning for client.
        WARNING = 8,
        // Indicates that server is going to close the connection.
        SESSION_END = 9,
        // Start a conversation
        CONVERSATION_START = 10,// [deprecated = true];
        // Update conversation settings. Uses payload_structured type ConversationUpdatePayload
        CONVERSATION_UPDATE = 12,
        // Server message to client with conversation id
        CONVERSATION_STARTED = 13,// [deprecated = true];
        // Conversation events. Contains payload_structured type ConversationEventPayload
        CONVERSATION_EVENT = 14,
        // Contains info about currently loaded scene. For example, scene name, description, loaded agents.
        CURRENT_SCENE_STATUS = 15,
        // Session configuration. Uses payload_structured type SessionConfigurationEvent
        SESSION_CONFIGURATION = 16
    }
    public enum SpaffCode
    {
        NEUTRAL,
        DISGUST,
        CONTEMPT,
        BELLIGERENCE,
        DOMINEERING,
        CRITICISM,
        ANGER,
        TENSION,
        TENSE_HUMOR,
        DEFENSIVENESS,
        WHINING,
        SADNESS ,
        STONEWALLING,
        INTEREST,
        VALIDATION,
        AFFECTION,
        HUMOR,
        SURPRISE,
        JOY
    }
    public enum Strength
    {
        UNSPECIFIED,
        WEAK,
        STRONG,
        NORMAL
    }
    public enum ConversationEventType
    {
        EVICTED,
        STARTED,
        UPDATED,
    }
    public enum SourceType
    {
        NONE,
        UNKNOWN,
        AGENT,
        PLAYER,
        WORLD
    }

    public enum DataType
    {
        UNSPECIFIED = 0,
        // Chunk contains audio data.
        AUDIO = 1,
        // Chunk with state data (bytes).
        STATE = 4,
        // Inspect data for active session which sent as data chunk
        INSPECT = 7,
    }
    public enum MicSampleMode
    {
        NO_MIC,
        NO_FILTER,
        PUSH_TO_TALK,
        AEC,
        TURN_BASED
    }
    public enum CharSelectingMethod
    {
        Manual,
        Auto,
        KeyCode,
        SightAngle
    }
}
