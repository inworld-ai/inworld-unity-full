/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

namespace Inworld.Assets
{
    public abstract class InworldFacialAnimation : InworldAnimation
    {
        void FixedUpdate()
        {
            BlinkEyes();
            ProcessLipSync();
        }
        protected abstract void ProcessLipSync();

        protected abstract void BlinkEyes();

        protected abstract void Reset();

    }
}
