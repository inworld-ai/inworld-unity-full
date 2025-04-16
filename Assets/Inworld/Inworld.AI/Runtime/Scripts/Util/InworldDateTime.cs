/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Inworld
{
    [Serializable]
    public class Duration
    {
        public long seconds;
        public int nanos; // milliseconds * 1,000,000,000
    }
    public static class InworldDateTime
    {
        static TimeSpan s_TimeSpan  = TimeSpan.Zero;
        // YAN: In Unity we use the first format.
        //      And server will return the format with 9 digits.
        //      However, DotNet can only process 7 digits at most. 
        //      We need to trim them first.
        static readonly string[] s_TimeFormat = 
        {
            "yyyy-MM-ddTHH:mm:ss.fffffffZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ssZ"
        };
        /// <summary>
        ///     Get the string of timestamp for the current UTC Time.
        /// </summary>
        public static string UtcNow => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        /// <summary>
        ///     Convert DateTime to string of timestamp.
        /// </summary>
        /// <param name="dateTime">dateTime to process.</param>
        /// <returns></returns>
        public static string ToString(DateTime dateTime) => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        /// <summary>
        ///     Convert the string of timestamp to DateTime.
        /// </summary>
        /// <param name="timestamp">string of timestamp to process</param>
        /// <returns></returns>
        public static DateTime ToDateTime(string timestamp) => DateTime.TryParseExact
        (
            // YAN: Match .fff (\d{3}) and put them in bracket $1. 
            Regex.Replace(timestamp, @"\.(\d{3})\d*Z", ".$1Z"), s_TimeFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out DateTime outTime
        ) ? outTime : DateTime.MinValue;


        public static int ToLatency(string timeStamp)
        {
            DateTime receivedTime = ToDateTime(timeStamp);
            if (s_TimeSpan == TimeSpan.Zero)
            {
                s_TimeSpan = DateTime.UtcNow - receivedTime;
                return 20;
            }
            TimeSpan delta = DateTime.UtcNow - s_TimeSpan - receivedTime;
            // YAN: Sometimes result can be even smaller than 0, due to the clock skew.
            // < 20ms is not able to be perceived. 
            int result = delta.Seconds * 1000 + delta.Milliseconds;
            return result > 20 ? result : 20;
        }
        public static Duration ToDuration(float duration) => new Duration()
        {
            seconds = (long)duration,
            nanos = (int)((duration - (long)duration) * 1000000000)
        };
    }
}
