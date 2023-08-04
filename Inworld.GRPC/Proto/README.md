# Inworld Internal DLL Generator

### When to rebuild DLL:
Sometimes the protos or related implementation packet files need to be updated. Then we need to rebuild `InworldProto.dll`
### Preparation:
**NOTE:** make sure this `internal-unity-sdk` repo project is **NOT** open in Unity Editor.

### 1. Delete the current DLL
Delete `internal-unity-sdk/Assets/Inworld.AI/Plugins/InworldProto.dll`

**NOTE:** Please make a backup for the dll in case the updated dll doesn't work.

### 2. Copy the whole folder into Unity.
Copy `InworldDLL_Bak` into `internal-unity-sdk/Assets/` 

**NOTE:** Please make sure it's parallel to the `Inworld.AI` folder, do NOT put it under `Inworld.AI`

### 3. Operations In Unity Editor.
1. Open Unity Editor and Open `internal-unity-sdk` project. 
2. Go to `Assets/Inworld.AI/InworldAI.asmdef`, under its `Assembly Definition References`, add `InworldProto.asmdef`
3. Click `Apply`.

### 4. Run the game at least once and quit Unity
The updated DLL would immediately generated when you click play button. But it's not under `Assets` folder.

### 5. Replace DLL and clean up.
**NOTE:** make sure this `internal-unity-sdk` repo project is **NOT** open in Unity Editor.
1. Go to `internal-unity-sdk/Library/ScriptAssemblies`, find `InworldProto.dll`.
2. Copy `InworldProto.dll` to `internal-unity-sdk/Assets/Inworld.AI/Plugins/InworldProto.dll`
3. Delete `internal-unity-sdk/Assets/InworldDLL_Bak` folder.
4. Open Unity, go to `Assets/Inworld.AI/InworldAI.asmdef`, remove the missing references (just deleted).

