// Done
using Audio;
using Managers;
using Player;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Options : MonoBehaviour, IUiPanel
    {
        [SerializeField] private Toggle healthBarToggle;
        [SerializeField] private float mixerMultiplier = 25.0f;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        
        private PlayerController _player;
        private CanvasGroup _canvasGroup;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public bool ShowMenuButtons => true;
        public bool ShowBackground => true;
        public bool DisablePlayerInput => true;
        public bool HasTooltips => false;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _player = GameManager.Instance.Player;
            healthBarToggle.onValueChanged.AddListener(OnHealthBarToggleChanged);
            bgmSlider.onValueChanged.AddListener(BgmSliderValue);
            sfxSlider.onValueChanged.AddListener(SfxSliderValue);
        }

        private void SfxSliderValue(float value)
        {
            FusionAudioManager.Instance.SetTrackVolume(AudioTrackType.PlayerFX, value);
            FusionAudioManager.Instance.SetSpatialAudioVolume(value);
        }

        private void BgmSliderValue(float value)
        {
            FusionAudioManager.Instance.SetTrackVolume(AudioTrackType.Background, value);
        }
        
        private void OnHealthBarToggleChanged(bool value)
        {
            //_player.Health.MiniHealthBarActive = value;
        }

        public void OnOpened()
        {
            // Sync sliders with current audio values (without triggering callbacks)
            bgmSlider.onValueChanged.RemoveListener(BgmSliderValue);
            bgmSlider.value = FusionAudioManager.Instance.GetTrackTargetVolume(AudioTrackType.Background);
            bgmSlider.onValueChanged.AddListener(BgmSliderValue);

            sfxSlider.onValueChanged.RemoveListener(SfxSliderValue);
            sfxSlider.value = FusionAudioManager.Instance.GetTrackTargetVolume(AudioTrackType.PlayerFX);
            sfxSlider.onValueChanged.AddListener(SfxSliderValue);
            
            // Sync health bar toggle (if player exists)
            if (_player && _player.Health)
            {
                healthBarToggle.onValueChanged.RemoveListener(OnHealthBarToggleChanged);
                healthBarToggle.isOn = false; // _player.Health.MiniHealthBarActive;
                healthBarToggle.onValueChanged.AddListener(OnHealthBarToggleChanged);
            }
        }
    }
}