# 3.6.0
1. Supported sending packets during disconnecting.
2. Released GroupChat mode by default.
3. Supported automatically updating group chat.
4. Added fallback voice recognition method.
5. Added Inworld's llm runtime support.

# 3.5.0
1. Updated Unity Editor Panel. Supported for Studio API Key.
2. Added experimental map/entities support.
3. Bug Fixes.

# 3.4.2
1. Upgrade with Unity's Input system for better customized key mapping.
2. Simplify Editor extension workflow.

# 3.4.1
1. Updated AEC logic to better sample the sound from microphone.
2. Add client side voice recognition (Windows only).
3. Solve conflict with WebSockets with other packages.
4. Bug fixes.

# 3.4.0
1. Supported multiagent system.
2. Updated data serializer.
3. Bug fixes.

# 3.3.2
1. Updated audio capturing.
2. Updated auto reconnection.
3. Simplified APIs for the incoming multiagent features.
4. Bug fixes.

# 3.3.1
1. Implemented feedback
2. Added character selecting in global panel.
3. Added config canvas in global panel.
4. Bug fixes.

# 3.3.0
New Features:
1. Updated auto reconnection. Optimized connection time.
2. Use tarball to replace github ref.

# 3.2.0
New Features:
1. Reorganize package structures. Put core package as reference.
2. Updated animation and character assets.

# 3.1.0
New Features:
1. Procedually loading packages for avoiding import errors.
2. Multiple audio input method support including turn based, push to talk, and aec.
3. Pausing interactions (By default it's space key).
4. Skipping interactions (By default it's left ctrl).
5. Save/load sessions for websocket.

# 3.0.0
New Features:
1. Replaced communication protocol from GRPC to Websocket and Inworld NDK
2. Reorganized package structure.
3. Replaced GLTFUtility by GLTFast.
4. Implemented echo cancellation.
5. Added WebGL support.

# 2.2.0 
New features:
1. Added Innequin model as default avatar.
2. Added UI for error process.
3. Added Relation system.
4. Added Narrative Action.
5. Added Microphone Test sample scene.
Updates:
1. Updated Phonemes.
2. Updated API for public workspace.
3. Updated getting access tokens in multiple ways (API key/secret, Access token, base64)
4. Loading User ID (Unity unique identifier) when loading scene.

# 2.1.3
Updated Inworld Lipsync.
Implemented manually connect/disconnect server scene.

# 2.1.2
Added Mac M1 support.
Updated Lip sync algorithm.
Fixed a bug of unable to chat with characters after headphone unplugged.

# 2.1.1
Added IL2CPP support for windows.
Updated demo scenes.

# 2.1.0
Updated multiple demo scenes.

# 2.0.4
Upgraded Fonts and art style in Inworld Editor.
Implemented facial expressions for Inworld Characters.
Fixed a bug that leads to building failed.
Added backend log support for Inworld Studio.

# 2.0.3
Support bind custom avatars into InworldCharacter.
Embedded lip sync automatically into the package.

# 2.0.2 
Fixed a bug that leads to Android building failed.

# 2.0.1
Fixed UI bugs in macOS.

# 2.0.0
Added studio authentication.
Added default 3D avatar support.
Added dafult animations.

# 1.2.1
Package related files into DLL.

# 1.2.0
Fixed bug of expiring token in some regions.
Fixed bug of VSPA in Android.

# 1.1.0
Add VSPA support.

# 1.0.0
Add Manual
Add API Descriptions
Update according to Unity Package Validator.

# 0.9.0
Add Unit Tests

