using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sokoban
{

    public struct LevelStats
    {
        public int moves;
        public float time;
        public int stars;
    }

    public class GameProgressionManager : MonoBehaviour
    {
        public static GameProgressionManager instance { get; private set; }

        private Dictionary<int, LevelStats> levelStats = new Dictionary<int, LevelStats>();

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RecordLevelStats(int levelIndex, int moves, float time, int stars)
        {

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

        public (int totalMoves, float totalTime, int totalStars) GetTotalStats()
        {
            return (levelStats.Values.Sum(s => s.moves), levelStats.Values.Sum(s => s.time), levelStats.Values.Sum(s => s.stars));
        }

        public void ResetProgression()
        {
            levelStats.Clear();
            Debug.Log("Прогресс игрока сброшен.");
        }
    }
}