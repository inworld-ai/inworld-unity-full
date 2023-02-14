/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Ai.Inworld.Studio.V1Alpha;
using Ai.Inworld.Voices;
using Google.Api;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Inworld.Grpc;
using Inworld.Util;
using UnityEngine;
using UnityEngine.Scripting;
public class ForceInitialize : MonoBehaviour
{
    // Yan: Force to Init all generic enums and interfaces.
    //      Must have customized Google.Protobuf Library.
    //      Do not call this func.
    [Preserve]
    void ExampleOfForceReflectionInitializationForProtobuf()
    {
        FileDescriptor.ForceReflectionInitialization<FieldBehavior>();
        FileDescriptor.ForceReflectionInitialization<ControllerStates>();
        FileDescriptor.ForceReflectionInitialization<InworldPacket.PacketOneofCase>();
        FileDescriptor.ForceReflectionInitialization<Actor.Types.Type>();
        FileDescriptor.ForceReflectionInitialization<TextEvent.Types.SourceType>();
        FileDescriptor.ForceReflectionInitialization<AuthType>();
        FileDescriptor.ForceReflectionInitialization<Gender>();
        FileDescriptor.ForceReflectionInitialization<TTSType>();
        FileDescriptor.ForceReflectionInitialization<VoicePreset>();
        FileDescriptor.ForceReflectionInitialization<FieldType>();
        FileDescriptor.ForceReflectionInitialization<FieldOptions.Types.CType>();
        FileDescriptor.ForceReflectionInitialization<FieldOptions.Types.JSType>();
        FileDescriptor.ForceReflectionInitialization<Character.Types.CharacterDescription.Types.Pronoun>();
        ReflectionUtil.ExtensionReflectionHelper<FieldOptions, FieldBehavior> exp = new ReflectionUtil.ExtensionReflectionHelper<FieldOptions, FieldBehavior>(new Extension<FieldOptions, FieldBehavior>(0, null));
    }
}
