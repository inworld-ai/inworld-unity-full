namespace TextureSource
{
    using System.IO;
    using UnityEngine;
    using UnityEngine.Video;

    /// <summary>
    /// Source from video player
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Texture Source/Video", fileName = "VideoTextureSource")]
    public class VideoTextureSource : BaseTextureSource
    {
        [System.Serializable]
        public class VideoData
        {
            public VideoSource source;
            public string url;
            public VideoClip clip;

            public VideoSource Source => clip == null
                ? VideoSource.Url : VideoSource.VideoClip;

            public string URL
            {
                get
                {
                    return Path.IsPathRooted(url)
                        ? url
                        : Path.Combine(Application.dataPath, url);
                }
            }
        }

        [SerializeField]
        [Tooltip("Whether to loop the video")]
        private bool loop = true;

        [SerializeField]
        [Tooltip("Whether to play sound in the video")]
        private bool playSound = false;

        [SerializeField]
        private VideoData[] videos = default;

        private VideoPlayer player;
        private int currentIndex;
        private long currentFrame = -1;

        public override bool DidUpdateThisFrame
        {
            get
            {
                long frame = player.frame;
                bool isUpdated = frame != currentFrame;
                currentFrame = frame;
                return isUpdated;
            }
        }

        public override Texture Texture => player.texture;

        public override void Start()
        {
            GameObject go = new GameObject(nameof(VideoTextureSource));
            DontDestroyOnLoad(go);
            player = go.AddComponent<VideoPlayer>();
            player.renderMode = VideoRenderMode.APIOnly;
            player.audioOutputMode = playSound
                ? VideoAudioOutputMode.Direct
                : VideoAudioOutputMode.None;
            player.isLooping = loop;

            currentIndex = Mathf.Min(currentIndex, videos.Length - 1);

            StartVideo(currentIndex);
        }

        public override void Stop()
        {
            if (player == null)
            {
                return;
            }
            player.Stop();
            Destroy(player.gameObject);
            player = null;
        }

        public override void Next()
        {
            currentIndex = (currentIndex + 1) % videos.Length;
            StartVideo(currentIndex);
        }

        private void StartVideo(int index)
        {
            var data = videos[index];
            VideoSource source = data.Source;
            player.source = source;
            if (source == VideoSource.Url)
            {
                player.url = data.URL;
            }
            else
            {
                player.clip = data.clip;
            }
            player.Prepare();
            player.Play();

            currentFrame = -1;
        }
    }
}
