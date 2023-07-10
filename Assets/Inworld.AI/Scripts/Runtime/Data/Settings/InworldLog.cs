/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;
namespace Inworld.Util
{
    public class InworldError
    {
        public string statusCode;
        public string detail;
        const string k_Pattern = @"StatusCode=""([^""]*)"", Detail=""([^""]*)""";
        public static InworldError FromString(string error)
        {
            InworldError result = new InworldError();
            Match match = Regex.Match(error, k_Pattern);

            if (match.Success && match.Groups.Count > 1)
            {
                result.statusCode = match.Groups[1].Value;
                result.detail = match.Groups[2].Value;
            }
            else
            {
                result.statusCode = "Error";
                result.detail = error;
            }
            return result;
        }
        public string Message => $"{statusCode}: {detail}";
    }
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
