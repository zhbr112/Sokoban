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
            // Сообщаем UIManager номер уровня (берем из LevelManager)
            UIManager.instance.UpdateLevelText(LevelManager.instance.GetCurrentLevelIndex() + 1);
            
            // Показываем подсказку на первом уровне
            if (LevelManager.instance.GetCurrentLevelIndex() == 0)
            {
                UIManager.instance.ShowTutorialPanel(true);
            }
            else
            {
                UIManager.instance.ShowTutorialPanel(false);
            }

            // Прячем меню победы через UIManager
            UIManager.instance.ShowLevelCompleteMenu(false);
            UIManager.instance.ShowSkipLevelButton(false); // Скрываем кнопку пропуска при инициализации

            player = GameObject.FindGameObjectWithTag("Player"); // Находим игрока на сцене
            if (player != null) // Проверяем, что игрок найден
            {
                playerGraphics = player.GetComponent<PlayerGraphics>();
            }
            else
            {
                Debug.LogError("Инициализация провалена: Игрок не найден!");
                return; // Прерываем, если игрока нет
            }
        }

        void Update()
        {
            // Таймер работает только если игра не выиграна
            if (!isGameWon && !isGamePaused)
            {
                levelTimer += Time.deltaTime;
                // Добавляем проверку, чтобы убедиться, что UIManager уже существует
                if (UIManager.instance != null)
                {
                    UIManager.instance.UpdateTimeText(levelTimer);
                }
            }
        }

        // Этот метод будет вызываться из PlayerController
        public void MovePlayer(Vector2 direction)
        {
            // Если игра на паузе, выиграна, или нет игрока - выходим
            if (isGameWon || player == null || playerGraphics == null)
            {
                return;
            }

            // Скрываем подсказку при первом движении
            if (UIManager.instance.IsTutorialPanelVisible()) {
                UIManager.instance.ShowTutorialPanel(false);
            }

            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                // Если и сейчас не нашли, то выходим, чтобы избежать ошибки
                if (player == null)
                {
                    Debug.LogError("Игрок с тегом 'Player' не найден на сцене!");
                    return; // Прерываем выполнение метода
                }
            }
            playerGraphics.SetDirectionalSprite(direction);
            // Текущая позиция игрока
            Vector2 currentPos = player.transform.position;
            // Целевая позиция игрока
            Vector2 targetPos = currentPos + direction;

            // 1. Проверяем, что находится в целевой клетке
            Collider2D[] hit = Physics2D.OverlapPointAll(targetPos);

            if (!IsWalkable(targetPos))
            {
                return;
            }

            // Если там пусто (или цель) - двигаемся
            if (hit.All(x => x.CompareTag("Goal") || x.CompareTag("Floor")))
            {
                player.transform.position = targetPos;
            }
            // Если там стена - ничего не делаем
            else if (hit.Any(x => x.CompareTag("Wall")))
            {
                return;
            }
            // Если там ящик - самая сложная логика
            else if (hit.Any(x => x.CompareTag("Box")))
            {
                // Определяем позицию за ящиком
                Vector2 boxTargetPos = targetPos + direction;
                Collider2D[] hitBehindBox = Physics2D.OverlapPointAll(boxTargetPos);

                if (!IsWalkable(boxTargetPos))
                {
                    return;
                }

                // Если за ящиком пусто или цель - двигаем ящик и игрока
                if (hitBehindBox.All(x => x.CompareTag("Goal") || x.CompareTag("Floor")))
                {
                    GameObject movedBox = hit.First(x => x.CompareTag("Box")).gameObject;
                    // Двигаем ящик
                    movedBox.transform.position = boxTargetPos;
                    // Двигаем игрока
                    player.transform.position = targetPos;

                    // Если не в тупике, проверяем, не выиграли ли мы.
                    // Этот вызов должен быть здесь, а не внутри условия выше.
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
            // Получаем все коллайдеры в точке
            Collider2D[] colliders = Physics2D.OverlapPointAll(position);

            // Проверяем, есть ли среди них хотя бы один с тегом "Floor"
            foreach (var col in colliders)
            {
                if (col.CompareTag("Floor"))
                {
                    return true; // Найдена ходибельная поверхность
                }
            }

            return false; // Если цикл завершился, ходибельных поверхностей нет
        }

        void CheckWinCondition()
        {
            // Находим все цели
            GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
            // Находим все ящики
            GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");

            if (goals.Length == 0) return; // Нет целей - нечего проверять

            int boxesOnGoals = 0;

            foreach (var box in boxes)
            {
                // Получаем ВСЕ коллайдеры в точке, где стоит ящик
                Collider2D[] colliders = Physics2D.OverlapPointAll(box.transform.position);

                bool isOnGoal = false;
                // Проверяем каждый найденный коллайдер
                foreach (var col in colliders)
                {
                    if (col.CompareTag("Goal"))
                    {
                        // Если хотя бы один из них - это цель, значит ящик на месте
                        isOnGoal = true;
                        break; // Выходим из внутреннего цикла, дальше проверять не нужно
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
            // Если количество ящиков на целях равно общему количеству целей
            if (boxesOnGoals == goals.Length)
            {
                isGameWon = true;
                GameManager.isGamePaused = true;

                int stars = CalculateStars();
                Debug.Log($"ПОБЕДА! Заработано звезд: {stars}");

                UIManager.instance.ShowLevelCompleteMenu(true, stars);

                // Сохраняем статистику уровня
                int currentLevelIndex = LevelManager.instance.GetCurrentLevelIndex();
                GameProgressionManager.instance.RecordLevelStats(currentLevelIndex, movesCount, levelTimer, stars);
            }
        }

        private void CheckSkipButtonCondition()
        {
            // Показываем кнопку пропуска, если ходов в 2 раза больше, чем нужно для 2 звезд
            // и игра еще не выиграна
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