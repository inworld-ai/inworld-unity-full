/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;


namespace Inworld.Entities
{
    [Serializable]
    public class Client
    {
        public string id;
        public string version;
        public string description;
    }
    [Serializable]
    public class ReleaseData
    {
        public PackageData[] package;
    }

    [Serializable]
    public class PackageData
    {
        public string published_at;
        public string tag_name;
    }
}
