/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


using Inworld.Packet;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using Inworld.Entities;
using UnityEngine;
using UnityEngine.TestTools;


namespace Inworld.Test.Sync
{
	public class InworldRuntimeSyncTest : InworldRuntimeTest
	{
		protected override IEnumerator InitTest()
		{
			InworldController.Client.CurrentScene = k_TestScene;
			InworldController.Instance.Init();
			yield return StatusCheck(5, InworldConnectionStatus.Initialized);
			Assert.IsTrue(InworldController.IsTokenValid);
			InworldController.Client.StartSession();
			yield return StatusCheck(5, InworldConnectionStatus.Connected);
			yield return LiveSessionCheck(10);
		}
		protected override IEnumerator FinishTest()
		{
			yield break;
		}
	}
	public class ConnectionTests : InworldRuntimeSyncTest
	{
		[UnityTest]
		public IEnumerator SyncTest_ReconnectionTest()
		{
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			InworldCharacterData character = InworldController.Client.LiveSessionData.Values.First();
			Assert.IsTrue(character.brainName == k_TestQuin);
			Assert.IsTrue(character.givenName == k_TestQuinName);
			string agentID = character.agentId;
			m_Conversation.Clear();
			InworldController.Client.SendText(agentID, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			InworldController.Client.Disconnect();
			m_Conversation.Clear();
			InworldController.Client.SendText(agentID, "Hello");
			Assert.IsFalse(m_Conversation.Any(p => p is TextPacket));
			Assert.IsFalse(m_Conversation.Any(p => p is AudioPacket));
			yield return InitTest();
			m_Conversation.Clear();
			// YAN: Everything should be the same but AgentID Should be changed.
			character = InworldController.Client.LiveSessionData.Values.First(); 
			Assert.IsTrue(character.brainName == k_TestQuin);
			Assert.IsTrue(character.givenName == k_TestQuinName);
			Assert.IsFalse(character.agentId == agentID);
			InworldController.Client.SendText(character.agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));		
		}
	}
	public class TextInteractionTests : InworldRuntimeSyncTest
	{
		[UnityTest]
		public IEnumerator SyncTest_SendText()
		{
			m_Conversation.Clear();
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest]
		public IEnumerator SyncTest_SendTextAfterUnknownPacket()
		{
			m_Conversation.Clear();
			OnPacketReceived(new InworldPacket());
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest]
		public IEnumerator SyncTest_SayVerbatim()
		{
			m_Conversation.Clear();
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "How are you?");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket textPacket && textPacket.text.text == k_VerbatimResponse));
		}
	}

	public class EmotionInteractionTests : InworldRuntimeSyncTest
	{
		[UnityTest]
		public IEnumerator SyncTest_EmotionChange()
		{
			m_Conversation.Clear();
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "You're feeling sad");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is EmotionPacket emoPacket 
			                                      && emoPacket.emotion.behavior == SpaffCode.SADNESS 
			                                      && emoPacket.emotion.strength == Strength.STRONG));
		}
	}

	public class UnitarySessionTests : InworldRuntimeSyncTest
	{
		[UnityTest]
		public IEnumerator SyncTest_ChangeScene()
		{
			m_Conversation.Clear();
			InworldController.Client.LoadScene(k_TestSceneVoice);
			yield return new WaitWhile(() => k_TestScene == m_CurrentInworldScene);
			Assert.IsTrue(m_CurrentInworldScene == k_TestSceneVoice);
			m_Conversation.Clear();
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(2)]
		public IEnumerator SyncTest_SwitchSceneBehavior()
		{
			// 1.1 Original Scene: Test Innquin.
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			InworldCharacterData characterInnequin = InworldController.Client.LiveSessionData.Values.First();
			Assert.IsTrue(characterInnequin.brainName == k_TestQuin);
			Assert.IsTrue(characterInnequin.givenName == k_TestQuinName);
			m_Conversation.Clear();
			// 1.2 Speak to Test Innequin.
			InworldController.Client.SendText(characterInnequin.agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			// 2.1 Load Scene: Test Voice.
			InworldController.Client.LoadScene(k_TestSceneVoice);
			yield return new WaitWhile(() => k_TestScene == m_CurrentInworldScene);
			Assert.IsTrue(m_CurrentInworldScene == k_TestSceneVoice);
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			// 2.2 Check Character: Voice Guy.
			InworldCharacterData characterVoice = InworldController.Client.LiveSessionData.Values.First();
			Assert.IsTrue(characterVoice.brainName == k_TestVoiceGuy);
			Assert.IsTrue(characterVoice.givenName == k_TestVoiceName);
			m_Conversation.Clear();
			// 2.3 Speak to Innequin (False)
			InworldController.Client.SendText(characterInnequin.agentId, "Hello");
			yield return ConversationCheck(2, false);
			// 2.4 Speak to Voice Guy (True)
			InworldController.Client.SendText(characterVoice.agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			// 3.1 Load Scene back
			InworldController.Client.LoadScene(k_TestScene);
			yield return new WaitWhile(() => k_TestScene == m_CurrentInworldScene);
			yield return new WaitForSeconds(1f);
			Assert.IsTrue(m_CurrentInworldScene == k_TestScene);
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			// 3.2 Check Character: Test Innequin.
			characterInnequin = InworldController.Client.LiveSessionData.Values.First();
			Assert.IsTrue(characterInnequin.brainName == k_TestQuin);
			Assert.IsTrue(characterInnequin.givenName == k_TestQuinName);
			m_Conversation.Clear();
			// 3.3 Speak to Voice Guy (False)
			InworldController.Client.SendText(characterVoice.agentId, "Hello");
			yield return ConversationCheck(2, false);
			// 3.4 Speak to Innequin (True)
			InworldController.Client.SendText(characterInnequin.agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
	}
	public class AudioInteractionTests : InworldRuntimeSyncTest
	{
		[UnityTest, Order(1)]
		public IEnumerator SyncTest_SendAudio()
		{
			m_Conversation.Clear();
			string agentID = InworldController.Client.LiveSessionData.Values.First().agentId;
			InworldController.Client.StartAudio(agentID); ;
			yield return new WaitForSeconds(0.1f);
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			InworldController.Client.SendAudio(agentID, k_AudioChunkBase64);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudio(agentID);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(2)]
		public IEnumerator SyncTest_FreqSwitchAudioSession()
		{
			m_Conversation.Clear();
			string agentID = InworldController.Client.LiveSessionData.Values.First().agentId;
			for (int i = 0; i < 10; i++)
			{
				InworldController.Client.StartAudio(agentID); 
				yield return new WaitForSeconds(0.1f);
				//InworldController.Audio.AutoDetectPlayerSpeaking = false;
				InworldController.Client.StopAudio(agentID);
				yield return new WaitForSeconds(0.1f);
			}
			InworldController.Client.StartAudio(agentID); 
			yield return new WaitForSeconds(0.1f);
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			InworldController.Client.SendAudio(agentID, k_AudioChunkBase64);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudio(agentID);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
		[UnityTest, Order(3)]
		public IEnumerator SyncTest_InterleaveTextAudio()
		{
			m_Conversation.Clear();
			InworldController.Client.SendText(InworldController.Client.LiveSessionData.Values.First().agentId, "Hello");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
			m_Conversation.Clear();
			//InworldController.Audio.AutoDetectPlayerSpeaking = false;
			string agentID = InworldController.Client.LiveSessionData.Values.First().agentId;
			InworldController.Client.StartAudio(agentID); ;
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.SendAudio(agentID, k_AudioChunkBase64);
			yield return new WaitForSeconds(0.1f);
			InworldController.Client.StopAudio(agentID);
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket));
			Assert.IsTrue(m_Conversation.Any(p => p is AudioPacket));
		}
	}

	public class TriggerInteractionTests : InworldRuntimeSyncTest
	{
		[UnityTest, Order(1)]
		public IEnumerator SyncTest_GoalsTrigger()
		{
			m_Conversation.Clear();
			InworldController.Client.SendTrigger(InworldController.Client.LiveSessionData.Values.First().agentId, "hit_trigger");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_TriggerGoal));
			Assert.IsTrue(m_Conversation.Any(p => p is TextPacket textPacket && textPacket.text.text == k_TriggerResponse));
		}
		[UnityTest, Order(2)]
		public IEnumerator SyncTest_GoalsRepeatable()
		{
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			InworldCharacterData character = InworldController.Client.LiveSessionData.Values.First();
			m_Conversation.Clear();
			InworldController.Client.SendText(character.agentId, "Repeatable");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_RepeatableGoal));
			m_Conversation.Clear();
			InworldController.Client.SendText(character.agentId, "Repeatable");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_RepeatableGoal));
		}
		[UnityTest, Order(3)]
		public IEnumerator SyncTest_GoalsUnrepeatable()
		{
			Assert.IsTrue(InworldController.Client.LiveSessionData.Values.Count > 0);
			InworldCharacterData character = InworldController.Client.LiveSessionData.Values.First();
			m_Conversation.Clear();
			InworldController.Client.SendText(character.agentId, "Unrepeatable");
			yield return ConversationCheck(10);
			Assert.IsTrue(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_UnrepeatableGoal));
			m_Conversation.Clear();
			InworldController.Client.SendText(character.agentId, "Unrepeatable");
			yield return ConversationCheck(10);
			Assert.IsFalse(m_Conversation.Any(p => p is CustomPacket customPacket && customPacket.Trigger == k_UnrepeatableGoal));
		}
	}
}
