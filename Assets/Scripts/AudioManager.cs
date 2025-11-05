
using UnityEngine;

namespace Sokoban
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string MUSIC_ENABLED_KEY = "MusicEnabled";
        private const string MUSIC_TRACK_KEY = "MusicTrackIndex";

        private AudioSource backgroundMusicSource;
        private float lastVolumeBeforeMute;
        private int currentTrackIndex = 0;

        [Tooltip("Список всех фоновых треков, доступных в игре.")]
        [SerializeField] private AudioClip[] musicTracks;

        public bool IsMusicOn { get; private set; } = true;
        public AudioClip[] MusicTracks => musicTracks;

        void Awake()
        {

            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                backgroundMusicSource = GetComponent<AudioSource>();

                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {

            float loadedVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
            backgroundMusicSource.volume = loadedVolume;
            

            IsMusicOn = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;

            currentTrackIndex = PlayerPrefs.GetInt(MUSIC_TRACK_KEY, 0);
            if (musicTracks != null && musicTracks.Length > currentTrackIndex)
            {
                backgroundMusicSource.clip = musicTracks[currentTrackIndex];
            }

            lastVolumeBeforeMute = (loadedVolume > 0) ? loadedVolume : 0.5f;

            backgroundMusicSource.loop = true;

            if (!IsMusicOn) backgroundMusicSource.Pause();
            else backgroundMusicSource.Play();
        }

        public void ToggleMusic()
        {
            IsMusicOn = !IsMusicOn;
            if (IsMusicOn)
            {

                SetMusicVolume(lastVolumeBeforeMute);
                backgroundMusicSource.UnPause();
            }
            else
            {

                if (backgroundMusicSource.volume > 0)
                {
                    lastVolumeBeforeMute = backgroundMusicSource.volume;
                }

                SetMusicVolume(0f);
                backgroundMusicSource.Pause();
            }

            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, IsMusicOn ? 1 : 0);
        }

        public void SetMusicVolume(float volume)
        {
            backgroundMusicSource.volume = Mathf.Clamp01(volume);

            if (backgroundMusicSource.volume > 0)
            {
                IsMusicOn = true;
                lastVolumeBeforeMute = backgroundMusicSource.volume;
                backgroundMusicSource.UnPause();
            }
            else
            {
                IsMusicOn = false;
            }

            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, backgroundMusicSource.volume);
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, IsMusicOn ? 1 : 0);
        }

        public float GetMusicVolume()
        {
            return backgroundMusicSource.volume;
        }

        public void ChangeMusicTrack(int trackIndex)
        {
            if (musicTracks == null || trackIndex < 0 || trackIndex >= musicTracks.Length)
            {
                Debug.LogWarning($"Попытка включить несуществующий трек с индексом {trackIndex}");
                return;
            }

            currentTrackIndex = trackIndex;
            backgroundMusicSource.clip = musicTracks[trackIndex];
            backgroundMusicSource.Play();
            PlayerPrefs.SetInt(MUSIC_TRACK_KEY, trackIndex);
        }

        public int GetCurrentTrackIndex() => currentTrackIndex;
    }
}
