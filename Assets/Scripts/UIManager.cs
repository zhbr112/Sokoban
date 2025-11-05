using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Sokoban
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }

        private const string LEVEL_PREFIX = "Уровень: ";
        private const string MOVES_PREFIX = "Ходы: ";
        private const string TIME_PREFIX = "Время: ";

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Menus & Panels")]
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private GameObject levelCompleteMenu;
        [SerializeField] private Button skipLevelButton;

        [Header("Level Complete Elements")]
        [SerializeField] private List<Image> starImages;
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        [Header("Game Complete Elements")]
        [SerializeField] private GameObject gameCompleteMenu;
        [SerializeField] private TextMeshProUGUI totalMovesText;
        [SerializeField] private TextMeshProUGUI totalTimeText;
        [SerializeField] private TextMeshProUGUI totalStarsText;

        [Header("Main Menu Elements")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button openAuthButton;
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private Button quitGameButton;

        [Header("About Panel Elements")]
        [SerializeField] private GameObject aboutPanel;
        [SerializeField] private Button openAboutButton;
        [SerializeField] private Button closeAboutButton;

        [Header("Auth Elements")]
        [SerializeField] private GameObject authMenu;
        [SerializeField] private Button guestModeButton;
        [SerializeField] private Button closeAuthButton;
        [SerializeField] private GameObject hudPanel;

        [Header("Settings Elements")]
        [SerializeField] private Button openMusicMenuButton;
        [SerializeField] private Button returnToMenuButton;
        [SerializeField] private Image musicButtonImage;
        [SerializeField] private Sprite musicOnSprite;
        [SerializeField] private Sprite musicOffSprite;
        [SerializeField] private Slider musicVolumeSlider;

        [Header("Leaderboard Elements")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Button openLeaderboardButton;
        [SerializeField] private Button closeLeaderboardButton;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private Transform leaderboardContentContainer;
        [SerializeField] private GameObject leaderboardSeparatorPrefab;

        [Header("Music Selection Menu")]
        [SerializeField] private GameObject musicSelectionMenu;
        [SerializeField] private Button musicTrackButtonPrefab;
        [SerializeField] private Transform musicTrackButtonsContainer;
        [SerializeField] private Button restartGameButton;

        [Header("Tutorial Elements")]
        [SerializeField] private GameObject tutorialPanel;
        private List<Button> musicTrackButtons = new List<Button>();
        private bool isLeaderboardGridSetup = false;

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

        void OnDestroy()
        {

        }

        void Start()
        {

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGamePressed);
            }
            if (openAuthButton != null)
            {
                openAuthButton.onClick.AddListener(OnOpenAuthPressed);
            }
            if (openSettingsButton != null)
            {

                openSettingsButton.onClick.AddListener(() => ToggleSettingsMenu(true));
            }

            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(QuitGame);
            }

            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (openAboutButton != null)
            {
                openAboutButton.onClick.AddListener(() => ToggleAboutPanel(true));
            }

            if (closeAboutButton != null)
            {
                closeAboutButton.onClick.AddListener(() => ToggleAboutPanel(false));
            }

            if (openMusicMenuButton != null)
            {
                openMusicMenuButton.onClick.AddListener(OnOpenMusicMenuPressed);
            }

            if (musicVolumeSlider != null)
            {

                musicVolumeSlider.value = AudioManager.instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (skipLevelButton != null)
            {
                skipLevelButton.onClick.AddListener(OnSkipLevelButtonPressed);
                skipLevelButton.gameObject.SetActive(false);
            }

            if (openLeaderboardButton != null)
            {
                openLeaderboardButton.onClick.AddListener(OnOpenLeaderboardPressed);
            }

            if (closeLeaderboardButton != null)
            {
                closeLeaderboardButton.onClick.AddListener(() => ToggleLeaderboardPanel(false));
            }

            if (restartGameButton != null)
            {
                restartGameButton.onClick.AddListener(OnRestartGamePressed);
            }

            PopulateMusicSelectionMenu();

            UpdateMusicVisuals();

            if (guestModeButton != null)
            {
                guestModeButton.onClick.AddListener(AuthManager.instance.OnGuestModeButtonClicked);
            }

            if (closeAuthButton != null)
            {
                closeAuthButton.onClick.AddListener(CloseAuthMenu);
            }

            mainMenuPanel.SetActive(true);
            authMenu.SetActive(false);
            hudPanel.SetActive(false);
            levelCompleteMenu.SetActive(false);
            gameCompleteMenu.SetActive(false);
            settingsMenu.SetActive(false);
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);
        }

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

            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = $"{TIME_PREFIX}{minutes:00}:{seconds:00}";
        }

        public void ToggleSettingsMenu(bool show)
        {
            if (settingsMenu != null)
            {
                settingsMenu.SetActive(show);

                if (show)
                {

                    PauseGame();
                }
                else
                {
                    ResumeGame();
                    ToggleMusicSelectionMenu(false);
                }
            }
        }

        public void ToggleLeaderboardPanel(bool show)
        {
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(show);
            }
        }

        private void ToggleAboutPanel(bool show)
        {
            if (aboutPanel != null)
            {
                aboutPanel.SetActive(show);
            }
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(!show);
            }
        }

        public void ToggleMusicSelectionMenu(bool show)
        {
            if (musicSelectionMenu != null)
            {
                musicSelectionMenu.SetActive(show);

                if (show)
                {

                    UpdateMusicButtonHighlight();
                }
            }
        }

        private void PauseGame()
        {
            Time.timeScale = 0f;
            GameManager.isGamePaused = true;
            Debug.Log("Игра на паузе");
        }

        private void ResumeGame()
        {   

            Time.timeScale = 1f;
            GameManager.isGamePaused = false;
            Debug.Log("Игра снята с паузы");
        }

        public void ShowLevelCompleteMenu(bool show, int starsEarned = 0)
        {
            if (levelCompleteMenu != null)
            {
                levelCompleteMenu.SetActive(show);
                ShowSkipLevelButton(false);
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

                var (totalMoves, totalTime, totalStars) = GameProgressionManager.instance.GetTotalStats();

                int maxStars = totalLevels * 3;

                totalMovesText.text = $"{totalMoves}";

                int minutes = Mathf.FloorToInt(totalTime / 60);
                int seconds = Mathf.FloorToInt(totalTime % 60);
                totalTimeText.text = $"{minutes:00}:{seconds:00}";

                totalStarsText.text = $"{totalStars} / {maxStars}";

                gameCompleteMenu.SetActive(true);

                AuthManager.instance.TrySubmitGameResult(totalStars, totalMoves, (int)totalTime);
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

            for (int i = 0; i < starImages.Count; i++)
            {

                starImages[i].sprite = (i < starsEarned) ? starFilledSprite : starEmptySprite;
            }
        }

        public void ShowSkipLevelButton(bool show)
        {
            if (skipLevelButton != null && skipLevelButton.gameObject.activeSelf != show)
            {

                bool canShow = show && !levelCompleteMenu.activeSelf;
                skipLevelButton.gameObject.SetActive(canShow);
            }
        }

        public void ReturnToMainMenu()
        {

            ResumeGame();

            settingsMenu.SetActive(false);
            hudPanel.SetActive(false);
            authMenu.SetActive(false);
            levelCompleteMenu.SetActive(false);
            gameCompleteMenu.SetActive(false);
            leaderboardPanel.SetActive(false);
            if (aboutPanel != null) aboutPanel.SetActive(false);
            if (musicSelectionMenu != null) musicSelectionMenu.SetActive(false);
            if (tutorialPanel != null) tutorialPanel.SetActive(false);

            mainMenuPanel.SetActive(true);
        }

        public void QuitGame()
        {
            Debug.Log("Попытка выхода из игры...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnStartGamePressed()
        {
            AuthManager.instance.StartAsGuest();
        }

        private void OnOpenAuthPressed()
        {
            mainMenuPanel.SetActive(false);
            authMenu.SetActive(true);
        }

        private void OnRestartGamePressed()
        {
            gameCompleteMenu.SetActive(false);
            hudPanel.SetActive(false);

            mainMenuPanel.SetActive(true);
        }

        private void CloseAuthMenu()
        {
            authMenu.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        private void OnOpenMusicMenuPressed()
        {         
            ToggleMusicSelectionMenu(!musicSelectionMenu.activeSelf);
        }

        private void OnOpenLeaderboardPressed()
        {

            if (!isLeaderboardGridSetup)
            {

                isLeaderboardGridSetup = true;
            }
            AuthManager.instance.OnFetchLeaderboardClicked();
            ToggleLeaderboardPanel(true);
        }

        private void OnSkipLevelButtonPressed()
        {
            LevelManager.instance.SkipLevel();
        }

        private void OnMusicVolumeChanged(float volume)
        {
            AudioManager.instance.SetMusicVolume(volume);

            UpdateMusicVisuals();
        }

        private void UpdateMusicVisuals()
        {
            if (musicButtonImage == null) return;
            musicButtonImage.sprite = AudioManager.instance.IsMusicOn ? musicOnSprite : musicOffSprite;

            if (musicVolumeSlider != null)
            {

                musicVolumeSlider.SetValueWithoutNotify(AudioManager.instance.GetMusicVolume());
            }
        }

        private void PopulateMusicSelectionMenu()
        {
            if (musicTrackButtonPrefab == null || musicTrackButtonsContainer == null || AudioManager.instance.MusicTracks == null)
            {
                Debug.LogWarning("Не назначены префаб кнопки, контейнер или список треков для меню выбора музыки.");
                return;
            }

            musicTrackButtons.Clear();

            foreach (Transform child in musicTrackButtonsContainer)
            {
                Destroy(child.gameObject);
            }

            var musicTracks = AudioManager.instance.MusicTracks;
            for (int i = 0; i < musicTracks.Length; i++)
            {
                Button buttonInstance = Instantiate(musicTrackButtonPrefab, musicTrackButtonsContainer);
                TextMeshProUGUI buttonText = buttonInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = musicTracks[i].name;
                }

                int trackIndex = i;
                buttonInstance.onClick.AddListener(() => {
                    AudioManager.instance.ChangeMusicTrack(trackIndex);
                    UpdateMusicButtonHighlight();
                });
                buttonInstance.gameObject.name = $"TrackButton_{i}_{musicTracks[i].name}";
                musicTrackButtons.Add(buttonInstance);
            }
        }

        public void OnLoginSuccess()
        {
            mainMenuPanel.SetActive(false);
            authMenu.SetActive(false);
            hudPanel.SetActive(true);
            LevelManager.instance.StartGame();
        }

        public void PopulateLeaderboard(LeaderboardEntryDto[] entries)
        {
            if (leaderboardContentContainer == null || leaderboardEntryPrefab == null)
            {
                Debug.LogError("Не назначены элементы UI для таблицы лидеров в UIManager!");
                return;
            }

            foreach (Transform child in leaderboardContentContainer)
            {
                Destroy(child.gameObject);
            }

            GameObject headerGO = Instantiate(leaderboardEntryPrefab, leaderboardContentContainer);
            LeaderboardEntryUI headerUI = headerGO.GetComponent<LeaderboardEntryUI>();
            if (headerUI != null)
            {
                headerUI.SetHeader();
            }
            else
            {

                Debug.LogError($"На префабе {leaderboardEntryPrefab.name} отсутствует компонент LeaderboardEntryUI. Заголовок не может быть создан.");
                Destroy(headerGO);
            }

            if (leaderboardSeparatorPrefab != null)
            {
                Instantiate(leaderboardSeparatorPrefab, leaderboardContentContainer);
            }
            else
            {
                Debug.LogWarning("Префаб разделителя (leaderboardSeparatorPrefab) не назначен в UIManager.");
            }

            for (int i = 0; i < entries.Length; i++)
            {
                GameObject entryGO = Instantiate(leaderboardEntryPrefab, leaderboardContentContainer);
                LeaderboardEntryUI entryUI = entryGO.GetComponent<LeaderboardEntryUI>();

                if (entryUI != null)
                {

                    entryUI.SetData(i + 1, entries[i]);
                }
                else
                {
                    Debug.LogError($"На префабе {leaderboardEntryPrefab.name} отсутствует компонент LeaderboardEntryUI. Пожалуйста, добавьте его.");
                    Destroy(entryGO);
                }

                if (leaderboardSeparatorPrefab != null)
                {
                    Instantiate(leaderboardSeparatorPrefab, leaderboardContentContainer);
                }
            }
        }

        public void ShowTutorialPanel(bool show)
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(show);
            }
        }

        public bool IsTutorialPanelVisible()
        {
            return tutorialPanel != null && tutorialPanel.activeSelf;
        }

        private void UpdateMusicButtonHighlight()
        {
            int currentTrackIndex = AudioManager.instance.GetCurrentTrackIndex();

            Color highlightColor = new Color(0.7f, 1f, 0.7f, 1f);
            Color defaultColor = Color.white;

            ColorBlock highlightedColors = musicTrackButtons[0].colors;
            highlightedColors.normalColor = new Color(0.7f, 1f, 0.7f, 1f);
            highlightedColors.selectedColor = highlightedColors.normalColor;

            ColorBlock defaultColors = musicTrackButtons[0].colors;
            defaultColors.normalColor = Color.white;
            defaultColors.selectedColor = defaultColors.normalColor;

            for (int i = 0; i < musicTrackButtons.Count; i++)
            {

                var colors = musicTrackButtons[i].colors;

                if (i == currentTrackIndex)
                {

                    colors.normalColor = highlightColor;
                    colors.selectedColor = highlightColor;
                    musicTrackButtons[i].colors = highlightedColors;
                }
                else
                {

                    colors.normalColor = defaultColor;
                    colors.selectedColor = defaultColor;
                    musicTrackButtons[i].colors = defaultColors;
                }
                musicTrackButtons[i].colors = colors;
            }
        }
    }
}