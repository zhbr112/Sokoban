
using UnityEngine;

namespace Sokoban
{
    /// <summary>
    /// Управляет фоновой музыкой и звуковыми эффектами в игре.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }

        // Ключи для сохранения настроек
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string MUSIC_ENABLED_KEY = "MusicEnabled";

        private AudioSource backgroundMusicSource;
        private float lastVolumeBeforeMute;

        // Публичное свойство, чтобы другие скрипты могли узнать, включена ли музыка
        public bool IsMusicOn { get; private set; } = true;

        void Awake()
        {
            // Классический Singleton паттерн с сохранением объекта между сценами
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Сохраняем AudioManager при смене уровней
                backgroundMusicSource = GetComponent<AudioSource>();

                // Загружаем сохраненные настройки при запуске
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {
            // Загружаем громкость, по умолчанию 0.5
            float loadedVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
            backgroundMusicSource.volume = loadedVolume;

            // Загружаем состояние вкл/выкл, по умолчанию включено (1)
            IsMusicOn = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;

            // Если при загрузке громкость > 0, это наше "последнее" значение
            lastVolumeBeforeMute = (loadedVolume > 0) ? loadedVolume : 0.5f;

            // Если музыка должна быть выключена, применяем это состояние
            if (!IsMusicOn) backgroundMusicSource.Pause();
            else backgroundMusicSource.Play();
        }

        /// <summary>
        /// Включает или выключает фоновую музыку.
        /// </summary>
        public void ToggleMusic()
        {
            IsMusicOn = !IsMusicOn;
            if (IsMusicOn)
            {
                // Возвращаем последнюю громкость и возобновляем музыку
                SetMusicVolume(lastVolumeBeforeMute);
                backgroundMusicSource.UnPause();
            }
            else
            {
                // Запоминаем текущую громкость, если она не 0
                if (backgroundMusicSource.volume > 0)
                {
                    lastVolumeBeforeMute = backgroundMusicSource.volume;
                }
                // Устанавливаем громкость в 0
                SetMusicVolume(0f);
                backgroundMusicSource.Pause(); // Ставим музыку на паузу
            }
            // Сохраняем состояние
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, IsMusicOn ? 1 : 0);
        }

        /// <summary>
        /// Устанавливает громкость фоновой музыки.
        /// </summary>
        /// <param name="volume">Громкость от 0.0 до 1.0</param>
        public void SetMusicVolume(float volume)
        {
            backgroundMusicSource.volume = Mathf.Clamp01(volume);

            // Если громкость больше нуля, считаем, что музыка включена
            if (backgroundMusicSource.volume > 0)
            {
                IsMusicOn = true;
                lastVolumeBeforeMute = backgroundMusicSource.volume; // Запоминаем как последнее "рабочее" значение
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
    }
}
