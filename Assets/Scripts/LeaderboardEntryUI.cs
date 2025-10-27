using TMPro;
using UnityEngine;

namespace Sokoban
{
    /// <summary>
    /// Компонент для управления отображением одной строки в таблице лидеров.
    /// Вешается на префаб строки (leaderboardEntryPrefab).
    /// </summary>
    public class LeaderboardEntryUI : MonoBehaviour
    {
        // Свяжите эти поля с соответствующими TextMeshProUGUI в инспекторе вашего префаба.
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI starsText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;

        /// <summary>
        /// Заполняет поля UI данными из DTO.
        /// </summary>
        public void SetData(int rank, LeaderboardEntryDto data)
        {
            if (rankText != null)
                rankText.text = $"{rank}.";
            else
                Debug.LogWarning("LeaderboardEntryUI: rankText не назначен в инспекторе!");

            if (usernameText != null)
                usernameText.text = data.username;
            else
                Debug.LogWarning("LeaderboardEntryUI: usernameText не назначен в инспекторе!");

            if (starsText != null)
                starsText.text = data.totalStars.ToString();
            else
                Debug.LogWarning("LeaderboardEntryUI: starsText не назначен в инспекторе!");

            if (movesText != null)
                movesText.text = data.totalMoves.ToString();
            else
                Debug.LogWarning("LeaderboardEntryUI: movesText не назначен в инспекторе!");

            if (timeText != null)
            {
                int minutes = data.totalTime / 60;
                int seconds = data.totalTime % 60;
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
            else
                Debug.LogWarning("LeaderboardEntryUI: timeText не назначен в инспекторе!");
        }

        /// <summary>
        /// Заполняет поля UI текстом для заголовка таблицы.
        /// </summary>
        public void SetHeader()
        {
            // Используем жирный шрифт для заголовков, если возможно
            if (rankText != null) rankText.text = "<b>#</b>";
            if (usernameText != null) usernameText.text = "<b>Игрок</b>";
            if (starsText != null) starsText.text = "<b>Звезды</b>";
            if (movesText != null) movesText.text = "<b>Ходы</b>";
            if (timeText != null) timeText.text = "<b>Время</b>";

            // Можно также изменить цвет или размер шрифта для заголовка
            // if (usernameText != null) usernameText.color = Color.yellow;
        }
    }
}