using UnityEngine;
using UnityEngine.Video;

namespace SuperRodentGame.Content
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoBillboard : MonoBehaviour
    {
        [SerializeField] private bool playOnStart = true;

        private VideoPlayer videoPlayer;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Start()
        {
            if (playOnStart)
            {
                videoPlayer.Play();
            }
        }
    }
}
