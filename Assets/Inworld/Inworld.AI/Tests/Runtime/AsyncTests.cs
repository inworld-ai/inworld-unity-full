/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using System.Linq;
using Inworld.Entities;
using Inworld.Packet;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Inworld.Test.Async
{
    public class InworldRuntimeAsyncTest : InworldRuntimeTest
    {
        protected InworldCharacter m_CharQuin;
        protected InworldCharacter m_CharVoiceGuy;

        protected InworldCharacter GenerateCharacter(string inputGivenName, string inputBrainName)
        {
            if (!InworldController.Instance)
                return null;
            GameObject charObj = new GameObject(inputGivenName);
            InworldCharacter character = charObj.AddComponent<InworldCharacter>();
            character.Data = new InworldCharacterData
            {
                brainName = inputBrainName,
                givenName = inputGivenName
            };
            InworldController.CharacterHandler.Register(character);
            return character;
        }
        protected override IEnumerator InitTest()
        {
            m_CharQuin = GenerateCharacter(k_TestQuinName, k_TestQuin);
            m_CharVoiceGuy = GenerateCharacter(k_TestVoiceName, k_TestVoiceGuy);
            yield break;
        }

        protected override IEnumerator FinishTest()
        {
            Assert.IsNotNull(m_CharQuin);
            Assert.IsNotNull(m_CharVoiceGuy);
            Object.DestroyImmediate(m_CharQuin.gameObject);
            Object.DestroyImmediate(m_CharVoiceGuy.gameObject);
            yield return null;
        }
    }
    public class ConnectionTests : InworldRuntimeAsyncTest
    {
        [UnityTest]
        public IEnumerator AsyncTest_ReconnectionTest()
        {
            m_Conversation.Clear();
            InworldController.Client.SendTextTo("Hello", k_TestQuin);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
            Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
            InworldController.Client.Disconnect();
            m_Conversation.Clear();
            yield return new WaitForSeconds(2f);
            InworldController.Client.SendTextTo("Hello", k_TestQuin);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
            Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
        }
    }
    public class TextInteractionTests : InworldRuntimeAsyncTest
    {
        [UnityTest]
        public IEnumerator AsyncTest_SendTextToGroup()
        {
            m_Conversation.Clear();
            // YAN: For group. Scene data needs to be correct.
            InworldController.Client.CurrentScene = k_TestScene; 
            InworldController.Client.SendTextTo("Hello");
            yield return StatusCheck(5, InworldConnectionStatus.Connected);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
            Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
        }
        [UnityTest]
        public IEnumerator AsyncTest_SendTextToCharacter()
        {
            m_Conversation.Clear();
            InworldController.Client.SendTextTo("Hello", k_TestQuin);
            yield return StatusCheck(5, InworldConnectionStatus.Connected);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
            Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
        }
        [UnityTest]
        public IEnumerator AsyncTest_SendTextAfterUnknownPacket()
        {
            m_Conversation.Clear();
            OnPacketReceived(new InworldPacket());
            InworldController.Client.SendTextTo("Hello", k_TestQuin);
            yield return StatusCheck(5, InworldConnectionStatus.Connected);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
            Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
        }
        [UnityTest]
        public IEnumerator AsyncTest_SayVerbatim()
        {
            m_Conversation.Clear();
            InworldController.Client.SendTextTo("How are you?",k_TestQuin);
            yield return StatusCheck(5, InworldConnectionStatus.Connected);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is TextPacket textPacket && textPacket.text.text == k_VerbatimResponse));
        }
    }
    public class EmotionInteractionTests : InworldRuntimeAsyncTest
    {
        [UnityTest]
        public IEnumerator AsyncTest_EmotionChange()
        {
            m_Conversation.Clear();
            InworldController.Client.SendTextTo("You're feeling sad", k_TestQuin);
            yield return StatusCheck(5, InworldConnectionStatus.Connected);
            yield return ConversationCheck(10);
            Assert.IsTrue(m_Conversation.Any(p => p is EmotionPacket emoPacket 
                                                  && emoPacket.emotion.behavior == SpaffCode.SADNESS 
                                                  && emoPacket.emotion.strength == Strength.STRONG));
        }
    }
	public class UnitarySessionTests : InworldRuntimeAsyncTest
	{
		[UnityTest]
		public IEnumerator AsyncTest_AutoChangeScene()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Hello", k_TestVoiceGuy);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(2)]
		public IEnumerator AsyncTest_SwitchSceneBehavior()
		{
			// 1.1 Original Scene: Test Innquin.
			m_Conversation.Clear();
			InworldController.Client.CurrentScene = k_TestScene;
			// 1.2 Speak to Test Innequin.
			InworldController.Client.SendTextTo("Hello", k_TestQuin);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			// 2.1 Load Scene: Test Voice.
			InworldController.Client.LoadScene(k_TestSceneVoice);
			yield return new WaitWhile(() => k_TestScene == m_CurrentInworldScene);
			Assert.IsTrue(m_CurrentInworldScene == k_TestSceneVoice);
			m_Conversation.Clear();
			// 2.2 Speak to Innequin (False)
			InworldController.Client.SendTextTo("Hello", k_TestQuin);
			yield return ConversationCheck(2, false);
			// 2.3 Speak to Voice Guy (True)
			InworldController.Client.SendTextTo("Hello", k_TestVoiceGuy);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			// 3.1 Load Scene back
			InworldController.Client.LoadScene(k_TestScene);
			yield return new WaitWhile(() => k_TestScene == m_CurrentInworldScene);
			yield return new WaitForSeconds(1f);
			Assert.IsTrue(m_CurrentInworldScene == k_TestScene);
			m_Conversation.Clear();
			// 3.2 Speak to Voice Guy (False)
			InworldController.Client.SendTextTo("Hello", k_TestVoiceGuy);
			yield return ConversationCheck(2, false);
			// 3.3 Speak to Innequin (True)
			InworldController.Client.SendTextTo("Hello",k_TestQuin);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
	}
	public class AudioInteractionTests : InworldRuntimeAsyncTest
	{
		[UnityTest, Order(1)]
		public IEnumerator AsyncTest_SendAudio()
		{
			m_Conversation.Clear();
			InworldController.Client.StartAudioTo(k_TestQuin); 
			yield return new WaitForSeconds(0.1f);
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			InworldController.Client.SendAudioTo(k_AudioChunkBase64,k_TestQuin);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudioTo();
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(2)]
		public IEnumerator AsyncTest_FreqSwitchAudioSession()
		{
			m_Conversation.Clear();
			for (int i = 0; i < 10; i++)
			{
				InworldController.Client.StartAudioTo(k_TestQuin); 
				yield return new WaitForSeconds(0.1f);
				//InworldController.Audio.AutoDetectPlayerSpeaking = false;
				InworldController.Client.StopAudioTo();
				yield return new WaitForSeconds(0.1f);
			}
			InworldController.Client.StartAudioTo(k_TestQuin); 
			yield return new WaitForSeconds(0.1f);
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			InworldController.Client.SendAudioTo(k_AudioChunkBase64, k_TestQuin);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudioTo();
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(3)]
		public IEnumerator AsyncTest_InterleaveTextAudio()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Hello", k_TestQuin);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			m_Conversation.Clear();
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			InworldController.Client.StartAudioTo(k_TestQuin); ;
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.SendAudioTo(k_AudioChunkBase64, k_TestQuin);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudioTo();
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
	}

	public class TriggerInteractionTests : InworldRuntimeAsyncTest
	{
		[UnityTest, Order(1)]
		public IEnumerator AsyncTest_GoalsTrigger()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTriggerTo("hit_trigger", null,k_TestQuin);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_TriggerGoal));
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket textPacket && textPacket.text.text == k_TriggerResponse));
		}
		[UnityTest, Order(2)]
		public IEnumerator AsyncTest_GoalsRepeatable()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Repeatable", k_TestQuin);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_RepeatableGoal));
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Repeatable", k_TestQuin);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_RepeatableGoal));
		}
		[UnityTest, Order(3)]
		public IEnumerator AsyncTest_GoalsUnrepeatable()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Unrepeatable", k_TestQuin);
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_UnrepeatableGoal));
			m_Conversation.Clear();
			InworldController.Client.SendTextTo("Unrepeatable", k_TestQuin);
			yield return ConversationCheck(10);
			Assert.IsFalse(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_UnrepeatableGoal));
		}
	}
}