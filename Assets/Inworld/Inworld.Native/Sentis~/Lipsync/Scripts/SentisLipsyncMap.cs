/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Assets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Native
{
    [Serializable]
    public class VisemeData
    {
        public List<float> visemeVal;
        public string phonemeName;

        public VisemeData(string newLine)
        {
            visemeVal = new List<float>();
            string[] data = newLine.Split(',');
            phonemeName = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                visemeVal.Add(float.Parse(data[i]));
            }
        }
        public VisemeData()
        {
            visemeVal = new List<float>();
        }
    }

    public class SentisLipsyncMap : LipsyncMap
    {

    }

}
