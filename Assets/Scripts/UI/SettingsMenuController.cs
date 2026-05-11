using UnityEngine;
using UnityEngine.UI;

namespace SuperRodentGame.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle snapTurnToggle;
        [SerializeField] private Toggle vignetteToggle;

        public void LoadValuesIntoUI()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat("settings.master", 0.8f);
            if (musicVolumeSlider != null) musicVolumeSlider.value = PlayerPrefs.GetFloat("settings.music", 0.7f);
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = PlayerPrefs.GetFloat("settings.sfx", 0.8f);
            if (snapTurnToggle != null) snapTurnToggle.isOn = PlayerPrefs.GetInt("settings.snapTurn", 1) == 1;
            if (vignetteToggle != null) vignetteToggle.isOn = PlayerPrefs.GetInt("settings.vignette", 0) == 1;
        }

        public void SaveMaster(float value) => PlayerPrefs.SetFloat("settings.master", value);
        public void SaveMusic(float value) => PlayerPrefs.SetFloat("settings.music", value);
        public void SaveSfx(float value) => PlayerPrefs.SetFloat("settings.sfx", value);
        public void SaveSnapTurn(bool value) => PlayerPrefs.SetInt("settings.snapTurn", value ? 1 : 0);
        public void SaveVignette(bool value) => PlayerPrefs.SetInt("settings.vignette", value ? 1 : 0);
    }
}
