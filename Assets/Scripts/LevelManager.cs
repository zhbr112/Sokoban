using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Sokoban
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance { get; private set; }

        [SerializeField] private List<TextAsset> levelFiles;
        private int currentLevelIndex = 0;
        private LevelGenerator.LevelStarThresholds currentThresholds;

        private LevelGenerator levelGenerator;
        private GameManager gameManager;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Обнаружен еще один экземпляр LevelManager. Уничтожается.", gameObject);
                Destroy(gameObject);
            }

            levelGenerator = GetComponent<LevelGenerator>();
            gameManager = GetComponent<GameManager>();
        }

        void Start()
        {

        }

        public void StartGame()
        {

            if (levelFiles != null && levelFiles.Count > 0)
            {

                GameProgressionManager.instance.ResetProgression();
                currentLevelIndex = 0;
                LoadLevel(currentLevelIndex);
            }
            else
            {
                Debug.LogError("Список файлов уровней (levelFiles) не назначен в LevelManager.");
            }
        }

        private IEnumerator LoadLevelRoutine(int levelIndex)
        {
            currentThresholds = levelGenerator.GenerateLevel(levelFiles[levelIndex]);

            yield return null;

            gameManager.InitializeLevel(currentThresholds);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelFiles != null && levelIndex >= 0 && levelIndex < levelFiles.Count)
            {
                currentLevelIndex = levelIndex;
                StartCoroutine(LoadLevelRoutine(currentLevelIndex));
            }
            else
            {
                Debug.Log("Все уровни пройдены или неверный индекс уровня!");
            }
        }

        public void RestartLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        public void LoadNextLevel()
        {
            int nextLevelIndex = currentLevelIndex + 1;
            if (nextLevelIndex < levelFiles.Count)
            {
                LoadLevel(nextLevelIndex);
            }
            else
            {

                Debug.Log("Все уровни пройдены! Показываем финальный экран.");
                UIManager.instance.ShowGameCompleteMenu(levelFiles.Count);
            }
        }

        public void SkipLevel()
        {

            var (moves, time) = gameManager.GetCurrentStats();

            int finalMoves = moves;
            float finalTime = time;
            int stars = 0;

            GameProgressionManager.instance.RecordLevelStats(currentLevelIndex, finalMoves, finalTime, stars);

            LoadNextLevel();
        }

        public int GetCurrentLevelIndex()
        {
            return currentLevelIndex;
        }
    }
}