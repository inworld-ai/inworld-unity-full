using System;

namespace Inworld.Util
{
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
