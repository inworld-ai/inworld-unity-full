﻿/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;

namespace Inworld.Entities
{
	[Serializable]
	public class Feedback
	{
		public bool isLike;
		public List<string> type;
		public string comment;
		public string name;
	}
}
