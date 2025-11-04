using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sokoban
{
    /// <summary>
    /// Хранит статистику прохождения одного уровня.
    /// </summary>
    public struct LevelStats
    {
        public int moves;
        public float time;
        public int stars;
    }

    /// <summary>
    /// Управляет общим прогрессом игрока по всем уровням.
    /// </summary>
    public class GameProgressionManager : MonoBehaviour
    {
        public static GameProgressionManager instance { get; private set; }

        // Словарь для хранения статистики по каждому уровню (ключ - индекс уровня)
        private Dictionary<int, LevelStats> levelStats = new Dictionary<int, LevelStats>();

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Сохраняем объект между сценами
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Записывает или обновляет статистику для указанного уровня.
        /// </summary>
        public void RecordLevelStats(int levelIndex, int moves, float time, int stars)
        {
            // Если мы пытаемся записать результат с 0 звезд (пропуск уровня),
            // или если для этого уровня еще нет записи, или новая запись лучше (больше звезд),
            // то обновляем статистику.
            if (stars == 0 || !levelStats.ContainsKey(levelIndex) || stars > levelStats[levelIndex].stars)
            {
                levelStats[levelIndex] = new LevelStats { moves = moves, time = time, stars = stars };
                Debug.Log($"Статистика для уровня {levelIndex + 1} ЗАПИСАНА: {moves} ходов, {time:F2}с, {stars} звезд.");
            }
            else
            {
                Debug.Log($"Статистика для уровня {levelIndex + 1} НЕ обновлена, так как существующая запись лучше.");
            }
        }

        /// <summary>
        /// Возвращает суммарную статистику по всем пройденным уровням.
        /// </summary>
        public (int totalMoves, float totalTime, int totalStars) GetTotalStats()
        {
            return (levelStats.Values.Sum(s => s.moves), levelStats.Values.Sum(s => s.time), levelStats.Values.Sum(s => s.stars));
        }

        /// <summary>
        /// Сбрасывает всю сохраненную статистику по уровням.
        /// </summary>
        public void ResetProgression()
        {
            levelStats.Clear();
            Debug.Log("Прогресс игрока сброшен.");
        }
    }
}