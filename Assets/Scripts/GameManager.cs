using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Sokoban
{
    public class GameManager : MonoBehaviour
    {        public bool isGameWon = false;
        private GameObject player;
        private PlayerGraphics playerGraphics;
        private int movesCount;
        private float levelTimer;
        private LevelGenerator.LevelStarThresholds starThresholds;

        public static bool isGamePaused = false;

        public void InitializeLevel(LevelGenerator.LevelStarThresholds thresholds)
        {
            Debug.Log("GameManager: Инициализация уровня...");

            isGameWon = false;

            movesCount = 0;
            levelTimer = 0f;
            Time.timeScale = 1f;
            GameManager.isGamePaused = false;
            starThresholds = thresholds;

            UIManager.instance.UpdateMovesText(movesCount);
            UIManager.instance.UpdateTimeText(levelTimer);

            UIManager.instance.UpdateLevelText(LevelManager.instance.GetCurrentLevelIndex() + 1);
            

            if (LevelManager.instance.GetCurrentLevelIndex() == 0)
            {
                UIManager.instance.ShowTutorialPanel(true);
            }
            else
            {
                UIManager.instance.ShowTutorialPanel(false);
            }

            UIManager.instance.ShowLevelCompleteMenu(false);
            UIManager.instance.ShowSkipLevelButton(false);

            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerGraphics = player.GetComponent<PlayerGraphics>();
            }
            else
            {
                Debug.LogError("Инициализация провалена: Игрок не найден!");
                return;
            }
        }

        void Update()
        {

            if (!isGameWon && !isGamePaused)
            {
                levelTimer += Time.deltaTime;

                if (UIManager.instance != null)
                {
                    UIManager.instance.UpdateTimeText(levelTimer);
                }
            }
        }

        public void MovePlayer(Vector2 direction)
        {

            if (isGameWon || player == null || playerGraphics == null)
            {
                return;
            }

            if (UIManager.instance.IsTutorialPanelVisible()) {
                UIManager.instance.ShowTutorialPanel(false);
            }

            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");

                if (player == null)
                {
                    Debug.LogError("Игрок с тегом 'Player' не найден на сцене!");
                    return;
                }
            }
            playerGraphics.SetDirectionalSprite(direction);

            Vector2 currentPos = player.transform.position;

            Vector2 targetPos = currentPos + direction;

            Collider2D[] hit = Physics2D.OverlapPointAll(targetPos);

            if (!IsWalkable(targetPos))
            {
                return;
            }

            if (hit.All(x => x.CompareTag("Goal") || x.CompareTag("Floor")))
            {
                player.transform.position = targetPos;
            }

            else if (hit.Any(x => x.CompareTag("Wall")))
            {
                return;
            }

            else if (hit.Any(x => x.CompareTag("Box")))
            {

                Vector2 boxTargetPos = targetPos + direction;
                Collider2D[] hitBehindBox = Physics2D.OverlapPointAll(boxTargetPos);

                if (!IsWalkable(boxTargetPos))
                {
                    return;
                }

                if (hitBehindBox.All(x => x.CompareTag("Goal") || x.CompareTag("Floor")))
                {
                    GameObject movedBox = hit.First(x => x.CompareTag("Box")).gameObject;

                    movedBox.transform.position = boxTargetPos;

                    player.transform.position = targetPos;

                    CheckWinCondition();
                }
                else return;
            }
            movesCount++;
            UIManager.instance.UpdateMovesText(movesCount);

            CheckSkipButtonCondition();
        }

        private bool IsWalkable(Vector2 position)
        {

            Collider2D[] colliders = Physics2D.OverlapPointAll(position);

            foreach (var col in colliders)
            {
                if (col.CompareTag("Floor"))
                {
                    return true;
                }
            }

            return false;
        }

        void CheckWinCondition()
        {

            GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");

            GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");

            if (goals.Length == 0) return;

            int boxesOnGoals = 0;

            foreach (var box in boxes)
            {

                Collider2D[] colliders = Physics2D.OverlapPointAll(box.transform.position);

                bool isOnGoal = false;

                foreach (var col in colliders)
                {
                    if (col.CompareTag("Goal"))
                    {

                        isOnGoal = true;
                        break;
                    }
                }

                if (isOnGoal)
                {
                    boxesOnGoals++;
                }
            }

            Debug.Log(boxes.Length);
            Debug.Log(goals.Length);
            Debug.Log(boxesOnGoals);
            Debug.Log("");

            if (boxesOnGoals == goals.Length)
            {
                isGameWon = true;
                GameManager.isGamePaused = true;

                int stars = CalculateStars();
                Debug.Log($"ПОБЕДА! Заработано звезд: {stars}");

                UIManager.instance.ShowLevelCompleteMenu(true, stars);

                int currentLevelIndex = LevelManager.instance.GetCurrentLevelIndex();
                GameProgressionManager.instance.RecordLevelStats(currentLevelIndex, movesCount, levelTimer, stars);
            }
        }

        private void CheckSkipButtonCondition()
        {

            if (!isGameWon)
            {
                UIManager.instance.ShowSkipLevelButton(movesCount > starThresholds.TwoStars * 2);
            }
        }

        private int CalculateStars()
        {
            if (movesCount <= starThresholds.ThreeStars)
                return 3;
            if (movesCount <= starThresholds.TwoStars)
                return 2;

            return 1;
        }

        public void OnRestartButtonPressed()
        {
            LevelManager.instance.RestartLevel();
        }

        public void OnNextLevelButtonPressed()
        {
            LevelManager.instance.LoadNextLevel();
        }

        public (int moves, float time) GetCurrentStats()
        {
            return (movesCount, levelTimer);
        }
    }
}