using UnityEngine;

namespace SuperRodentGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip ambientLoop;

        public void ApplyPersistedSettings()
        {
            AudioListener.volume = PlayerPrefs.GetFloat("settings.master", 0.8f);
            if (musicSource != null)
                musicSource.volume = PlayerPrefs.GetFloat("settings.music", 0.7f);
            if (sfxSource != null)
                sfxSource.volume = PlayerPrefs.GetFloat("settings.sfx", 0.8f);
        }

        private void Start()
        {
            ApplyPersistedSettings();
            if (musicSource != null && ambientLoop != null && !musicSource.isPlaying)
            {
                musicSource.clip = ambientLoop;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void SetMusicVolume(float v)
        {
            if (musicSource != null) musicSource.volume = v;
        }

        public void SetSfxVolume(float v)
        {
            if (sfxSource != null) sfxSource.volume = v;
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
                sfxSource.PlayOneShot(clip);
        }
    }
}
