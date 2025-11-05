using TMPro;
using UnityEngine;

namespace Sokoban
{

    public class LeaderboardEntryUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI starsText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timeText;

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

        public void SetHeader()
        {

            if (rankText != null) rankText.text = "<b>#</b>";
            if (usernameText != null) usernameText.text = "<b>Игрок</b>";
            if (starsText != null) starsText.text = "<b>Звезды</b>";
            if (movesText != null) movesText.text = "<b>Ходы</b>";
            if (timeText != null) timeText.text = "<b>Время</b>";

        }
    }
}