using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameObject levelCompleteMenu;
    public bool isGameWon = false;
    private GameObject player;
    private PlayerGraphics playerGraphics;
    private int movesCount;
    private float levelTimer;

    public static bool isGamePaused = false;

    public void InitializeLevel()
    {
        Debug.Log("GameManager: Инициализация уровня...");

        isGameWon = false;

        movesCount = 0;
        levelTimer = 0f;
        Time.timeScale = 1f;
        GameManager.isGamePaused = false;

        UIManager.instance.UpdateMovesText(movesCount);
        UIManager.instance.UpdateTimeText(levelTimer);
        // Сообщаем UIManager номер уровня (берем из LevelManager)
        UIManager.instance.UpdateLevelText(LevelManager.instance.GetCurrentLevelIndex() + 1);

        // Прячем меню победы через UIManager
        UIManager.instance.ShowLevelCompleteMenu(false);

        if (levelCompleteMenu != null)
        {
            levelCompleteMenu.SetActive(false);
        }

        // Находим НОВЫЕ объекты на сцене
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerGraphics = player.GetComponent<PlayerGraphics>();
        }
        else
        {
            Debug.LogError("Инициализация провалена: Игрок не найден!");
            return; // Прерываем, если игрока нет
        }
        Debug.Log("GameManager: Инициализация прошла успешно");
    }

    void Update()
    {
        // Таймер работает только если игра не выиграна
        if (!isGameWon && !isGamePaused)
        {
            levelTimer += Time.deltaTime;
            UIManager.instance.UpdateTimeText(levelTimer);
        }
    }

    // Этот метод будет вызываться из PlayerController
    public void MovePlayer(Vector2 direction)
    {
        if (isGameWon || player == null || playerGraphics == null)
        {
            Debug.Log(isGameWon);
            Debug.Log(player == null);
            Debug.Log(playerGraphics == null);
            Debug.Log("GameManager: ошибка");
            return;
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
                // Двигаем ящик
                hit.First(x => x.CompareTag("Box")).transform.position = boxTargetPos;
                // Двигаем игрока
                player.transform.position = targetPos;

                // Проверяем, не выиграли ли мы после этого хода
                CheckWinCondition();
            }
        }
        movesCount++;
        UIManager.instance.UpdateMovesText(movesCount);
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
            Debug.Log("ПОБЕДА!");
            UIManager.instance.ShowLevelCompleteMenu(true);
            GameManager.isGamePaused = true; 
        }
    }

    public void OnRestartButtonPressed()
    {
        LevelManager.instance.RestartLevel();
    }

    public void OnNextLevelButtonPressed()
    {
        LevelManager.instance.LoadNextLevel();
    }
}