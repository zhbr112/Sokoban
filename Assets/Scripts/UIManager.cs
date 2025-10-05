using UnityEngine;
using TMPro; // Важно для работы с TextMeshPro
using System.Collections.Generic;
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
        [SerializeField] private Button skipLevelButton;

        [Header("Level Complete Elements")]
        [SerializeField] private List<Image> starImages; // Список компонентов Image для звезд
        [SerializeField] private Sprite starFilledSprite; // Спрайт закрашенной звезды
        [SerializeField] private Sprite starEmptySprite;  // Спрайт пустой звезды

        [Header("Game Complete Elements")]
        [SerializeField] private GameObject gameCompleteMenu;
        [SerializeField] private TextMeshProUGUI totalMovesText;
        [SerializeField] private TextMeshProUGUI totalTimeText;
        [SerializeField] private TextMeshProUGUI totalStarsText;


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

            if (skipLevelButton != null)
            {
                skipLevelButton.onClick.AddListener(OnSkipLevelButtonPressed);
                skipLevelButton.gameObject.SetActive(false); // Скрываем кнопку при старте
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

        public void ShowLevelCompleteMenu(bool show, int starsEarned = 0)
        {
            if (levelCompleteMenu != null)
            {
                levelCompleteMenu.SetActive(show);
                ShowSkipLevelButton(false); // Скрываем кнопку пропуска, когда уровень пройден
                if (show)
                {
                    UpdateStarsDisplay(starsEarned);
                }
            }
        }

        public void ShowGameCompleteMenu(int totalLevels)
        {
            if (gameCompleteMenu != null)
            {
                // Получаем итоговую статистику
                var (totalMoves, totalTime, totalStars) = GameProgressionManager.instance.GetTotalStats();

                // Максимальное количество звезд (по 3 на уровень)
                int maxStars = totalLevels * 3;

                // Обновляем текстовые поля
                totalMovesText.text = $"Всего ходов: {totalMoves}";

                int minutes = Mathf.FloorToInt(totalTime / 60);
                int seconds = Mathf.FloorToInt(totalTime % 60);
                totalTimeText.text = $"Общее время: {minutes:00}:{seconds:00}";

                totalStarsText.text = $"Собрано звезд: {totalStars} / {maxStars}";

                gameCompleteMenu.SetActive(true);
                ShowSkipLevelButton(false);
            }
        }

        private void UpdateStarsDisplay(int starsEarned)
        {
            if (starImages == null || starImages.Count == 0)
            {
                Debug.LogWarning("Список изображений звезд (starImages) не назначен в UIManager.");
                return;
            }

            // Проходим по всем изображениям звезд и устанавливаем нужный спрайт
            for (int i = 0; i < starImages.Count; i++)
            {
                // Если индекс меньше количества заработанных звезд, ставим "заполненную", иначе "пустую"
                starImages[i].sprite = (i < starsEarned) ? starFilledSprite : starEmptySprite;
            }
        }

        public void ShowSkipLevelButton(bool show)
        {
            if (skipLevelButton != null && skipLevelButton.gameObject.activeSelf != show)
            {
                // Показываем кнопку, только если уровень не пройден
                bool canShow = show && !levelCompleteMenu.activeSelf;
                skipLevelButton.gameObject.SetActive(canShow);
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

        private void OnSkipLevelButtonPressed()
        {
            LevelManager.instance.SkipLevel();
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