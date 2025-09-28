using UnityEngine;
using TMPro; // Важно для работы с TextMeshPro
using UnityEngine.UI; // Необходимо для работы с Button и Image

namespace Sokoban
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; } // Singleton для легкого доступа

        // --- Константы для текста ---
        private const string LEVEL_PREFIX = "Уровень: ";
        private const string MOVES_PREFIX = "Ходы: ";
        private const string TIME_PREFIX = "Время: ";

        // --- Ссылки на UI элементы (перетащим в инспекторе) ---
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Menus & Panels")]
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject levelCompleteMenu;

        [Header("Settings Elements")]
        [SerializeField] private Button musicToggleButton;
        [SerializeField] private Image musicButtonImage; // Компонент Image на кнопке
        [SerializeField] private Sprite musicOnSprite;   // Спрайт для состояния "Музыка ВКЛ"
        [SerializeField] private Sprite musicOffSprite;  // Спрайт для состояния "Музыка ВЫКЛ"
        [SerializeField] private Slider musicVolumeSlider; // Слайдер для громкости

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Обнаружен еще один экземпляр UIManager. Уничтожается.", gameObject);
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Добавляем обработчик нажатия на кнопку из кода
            if (musicToggleButton != null)
            {
                musicToggleButton.onClick.AddListener(OnMusicTogglePressed);
            }

            if (musicVolumeSlider != null)
            {
                // Устанавливаем начальное значение слайдера и добавляем обработчик
                musicVolumeSlider.value = AudioManager.instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            // Обновляем иконку при запуске игры в соответствии с состоянием AudioManager
            UpdateMusicVisuals();
        }

        // --- Методы для обновления HUD ---

        public void UpdateLevelText(int levelNumber)
        {
            levelText.text = $"{LEVEL_PREFIX}{levelNumber}";
        }

        public void UpdateMovesText(int movesCount)
        {
            movesText.text = $"{MOVES_PREFIX}{movesCount}";
        }

        public void UpdateTimeText(float timeInSeconds)
        {
            // Форматируем время в минуты и секунды
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = $"{TIME_PREFIX}{minutes:00}:{seconds:00}";
        }

        // --- Методы для управления меню ---

        public void ToggleSettingsMenu(bool show)
        {
            if (settingsMenu != null)
            {
                settingsMenu.SetActive(show);

                if (show)
                {
                    // Ставим игру на паузу
                    PauseGame();
                }
                else
                {
                    // Снимаем игру с паузы
                    ResumeGame();
                }
            }
        }

        private void PauseGame()
        {
            Time.timeScale = 0f; // Останавливаем время
            GameManager.isGamePaused = true; // Сообщаем остальной игре о паузе
            Debug.Log("Игра на паузе");
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f; // Возвращаем нормальный ход времени
            GameManager.isGamePaused = false; // Сообщаем о снятии с паузы
            Debug.Log("Игра снята с паузы");
        }

        public void ShowLevelCompleteMenu(bool show)
        {
            if (levelCompleteMenu != null)
            {
                levelCompleteMenu.SetActive(show);
            }
        }

        public void QuitGame()
        {
            Debug.Log("Попытка выхода из игры...");

            // Эта директива препроцессора означает:
            // "Если мы сейчас в редакторе Unity, выполнить этот код"
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

            // "Иначе (если это собранная игра), выполнить этот код"
#else
                Application.Quit();
#endif
        }

        /// <summary>
        /// Вызывается при нажатии на кнопку включения/выключения музыки.
        /// </summary>
        private void OnMusicTogglePressed()
        {
            AudioManager.instance.ToggleMusic();
            UpdateMusicVisuals();
        }

        private void OnMusicVolumeChanged(float volume)
        {
            AudioManager.instance.SetMusicVolume(volume);
            // Обновляем иконку кнопки, так как изменение громкости до 0 (или с 0) должно ее менять
            UpdateMusicVisuals();
        }

        private void UpdateMusicVisuals()
        {
            if (musicButtonImage == null) return;
            musicButtonImage.sprite = AudioManager.instance.IsMusicOn ? musicOnSprite : musicOffSprite;

            // Также обновляем положение слайдера, если он есть
            if (musicVolumeSlider != null)
            {
                // Устанавливаем значение слайдера без вызова события, чтобы избежать зацикливания
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.instance.GetMusicVolume());
            }
        }
    }
}