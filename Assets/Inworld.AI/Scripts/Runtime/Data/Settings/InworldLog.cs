/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
namespace Inworld.Util
{
    /// <summary>
    ///     Inworld Use UnityEngine's original Log system,
    ///     and does not send any data to our server.
    ///     All those log will not be displayed  in runtime or built-up application.
    ///     If "Edit > Preferences > Inworld.AI > IsVerboseLog" is unchecked,
    ///     related Inworld log would not be displayed in editor.
    ///     If you'd like to listen them,
    ///     please use Application.logMessageReceived to register events.
    /// </summary>
    public static class InworldLog
    {
        [Conditional("INWORLD_DEBUG")]
        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        public static void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        [Conditional("INWORLD_DEBUG")]
        public static void LogError(string msg)
        {
            Debug.LogError(msg);
        }
        public static void LogException(string exception)
        {
            throw new Exception(exception);
        }
    }
}
