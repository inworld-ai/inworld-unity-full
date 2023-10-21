/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
namespace Inworld.AI.Editor
{
    public interface IEditorState
    {
        public void OnOpenWindow();
        public void DrawTitle();
        public void DrawContent();
        public void DrawButtons();
        public void OnExit();
        public void OnEnter();
        public void PostUpdate();

    }
}
#endif