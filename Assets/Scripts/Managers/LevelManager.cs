// Done
using Audio;
using UI.Common;
using UnityEngine;

namespace Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private BackgroundAudioId backgroundMusic;
        
        private void Start()
        {
            if (!FusionAudioManager.Instance.IsTrackPlaying(AudioTrackType.Background))
            {
                FusionAudioManager.Instance.PlayAudio(backgroundMusic, fadeDuration: ScreenFader.Instance.FadeDuration);
            }
        }
    }
}