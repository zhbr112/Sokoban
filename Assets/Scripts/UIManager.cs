using UnityEngine;
using TMPro; // Важно для работы с TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager instance; // Singleton для легкого доступа

    // --- Ссылки на UI элементы (перетащим в инспекторе) ---
    [Header("HUD Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI timeText;

    [Header("Menus & Panels")]
    public GameObject settingsMenu;
    public GameObject levelCompleteMenu;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // --- Методы для обновления HUD ---

    public void UpdateLevelText(int levelNumber)
    {
        levelText.text = $"Уровень: {levelNumber}";
    }

    public void UpdateMovesText(int movesCount)
    {
        movesText.text = $"Ходы: {movesCount}";
    }

    public void UpdateTimeText(float timeInSeconds)
    {
        // Форматируем время в минуты и секунды
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        timeText.text = $"Время: {minutes:00}:{seconds:00}";
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
}