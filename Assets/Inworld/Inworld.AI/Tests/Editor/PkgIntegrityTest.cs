/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using NUnit.Framework;
using UnityEngine;


namespace Inworld.Test
{
    public class PkgIntegrityTest
    {
        [Test]
        public void InworldAITest()
        {
            Assert.NotNull(InworldAI.Instance);
            Assert.NotNull(InworldAI.ControllerPrefab);
            Assert.NotNull(InworldAI.DefaultThumbnail);
        }
        [Test]
        public void InworldUserTest()
        {
            Assert.NotNull(InworldAI.User);
            Assert.NotNull(InworldAI.User.Name);
        }
        [Test]
        public void InworldSDKDescriptionTest()
        {
            Client sdk = InworldAI.UnitySDK;
            Assert.NotNull(sdk);
            Assert.AreEqual("unity", sdk.id);
            string description = sdk.description;
            Assert.IsNotEmpty(description);
            Assert.IsTrue(description.Contains(Application.unityVersion));
            Assert.IsTrue(description.Contains(SystemInfo.operatingSystem));
            Assert.IsTrue(description.Contains(Application.productName));
        }
        [Test]
        public void InworldPathTest()
        {
            Assert.AreEqual("Assets/Inworld", InworldAI.InworldPath);
        }
    }
}
