/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


namespace Inworld.Entities
{
    public class Client
    {
        public string id;
        public string version;
        public string description;

        public override string ToString() => $"{id}: {version} {description}";
    }
    public class ReleaseData
    {
        public PackageData[] package;
    }
    public class PackageData
    {
        public string published_at;
        public string tag_name;
    }
}
