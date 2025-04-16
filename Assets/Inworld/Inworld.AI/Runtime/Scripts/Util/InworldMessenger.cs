/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System.Collections.Generic;
using System.Linq;
namespace Inworld.Entities
{
	/// <summary>
	/// This class is used to send/receive special interactive functions to/from Inworld server,
	/// via CustomPackets.
	/// </summary>
	public static class InworldMessenger
	{
		const string k_GoalEnable = "inworld.goal.enable";
		const string k_GoalDisable = "inworld.goal.disable";
		const string k_GoalComplete = "inworld.goal.complete";
		const string k_RelationUpdate = "inworld.relation.update";
		const string k_ConversationNextTurn = "inworld.conversation.next_turn";
		const string k_Uninterruptible = "inworld.uninterruptible";
		const string k_Error = "inworld.debug.error";
		const string k_Critical = "inworld.debug.critical-error";
		const string k_GoAway = "inworld.debug.goaway";
		const string k_IncompleteInteraction = "inworld.debug.setup-incomplete-interaction";
		const string k_Task = "inworld.task";
		const string k_TaskSucceeded = "inworld.task.succeeded";
		const string k_TaskFailed = "inworld.task.failed";

		static readonly Dictionary<string, InworldMessage> s_Message;

		static InworldMessenger()
		{
			s_Message = new Dictionary<string, InworldMessage>
			{
				[k_GoalEnable] = InworldMessage.GoalEnable,
				[k_GoalDisable] = InworldMessage.GoalDisable,
				[k_GoalComplete] = InworldMessage.GoalComplete,
				[k_RelationUpdate] = InworldMessage.RelationUpdate,
				[k_ConversationNextTurn] = InworldMessage.ConversationNextTurn,
				[k_Uninterruptible] = InworldMessage.Uninterruptible,
				[k_Error] = InworldMessage.Error,
				[k_Critical] = InworldMessage.Critical,
				[k_GoAway] = InworldMessage.GoAway,
				[k_IncompleteInteraction] = InworldMessage.IncompleteInteraction,
				[k_Task] = InworldMessage.Task
			};
		}
		public static string NextTurn => k_ConversationNextTurn;
		public static int GoalCompleteHead => k_GoalComplete.Length + 1; // YAN: With a dot in the end.
		public static bool GetTask(CustomPacket taskPacket, out string taskName)
        {
            taskName = null;
            if (taskPacket.custom.type != CustomType.TASK)
                return false;

            string triggerName = taskPacket.custom.name;
            
			if (!string.IsNullOrEmpty(triggerName) && triggerName.StartsWith($"{k_Task}."))
				taskName = triggerName.Replace($"{k_Task}.", "");

			return !string.IsNullOrEmpty(triggerName);
		}
		public static InworldMessage ProcessPacket(CustomPacket packet) => 
			(from data in s_Message where packet.custom.name.StartsWith(data.Key) select data.Value).FirstOrDefault();
		public static bool EnableGoal(string goalName, string brainName) => InworldController.Client.SendTriggerTo($"{k_GoalEnable}.{goalName}", null, brainName);
		
		public static bool DisableGoal(string goalName, string brainName) => InworldController.Client.SendTriggerTo($"{k_GoalDisable}.{goalName}", null, brainName);
		
		public static bool SendTaskSucceeded(string taskID, string brainName) => InworldController.Client.SendTriggerTo(k_TaskSucceeded, new Dictionary<string, string>()
			{ { "task_id", taskID } }, brainName);

		public static bool SendTaskFailed(string taskID, string reason, string brainName)
		{
			if (reason.Length >= 100)
			{
				InworldAI.LogWarning("Failed to send TaskFailed message: reason length must be < 100 characters");
				return false;
			}
			
			return InworldController.Client.SendTriggerTo(k_TaskFailed, new Dictionary<string, string>()
				{ { "task_id", taskID }, { "reason", reason } }, brainName);
		}

		public static bool DebugSendError() => InworldController.Instance.SendTrigger(k_Error);
		
		public static bool DebugSendCritical() => InworldController.Instance.SendTrigger(k_Critical);
		
		public static bool DebugSendGoAway() => InworldController.Instance.SendTrigger(k_GoAway);
		
		public static bool DebugSendIncompleteInteraction() => InworldController.Instance.SendTrigger(k_IncompleteInteraction);
	}
}
