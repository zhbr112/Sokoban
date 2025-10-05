using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sokoban
{
    public class LevelGenerator : MonoBehaviour
    {
        // --- Символы тайлов на карте ---
        private enum TileType
        {
            Wall = '#',
            Box = '$',
            Goal = '*',
            Player = '@',
            Floor = ' ', // Пол создается под всеми объектами
        }

        // --- Теги для поиска объектов ---
        private const string PLAYER_TAG = "Player";
        private const string BOX_TAG = "Box";

        // --- Публичные поля для префабов (перетащите их в инспекторе) ---
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject boxPrefab;
        [SerializeField] private GameObject goalPrefab;
        [SerializeField] private GameObject playerPrefab;

        private Transform levelContainer;

        // Структура для хранения порогов для звезд
        public struct LevelStarThresholds
        {
            public int ThreeStars;
            public int TwoStars;
        }

        public LevelStarThresholds GenerateLevel(TextAsset levelFile)
        {
            ClearLevel(); // Сначала очищаем старый уровень

            // Создаем контейнер для порядка в иерархии
            levelContainer = new GameObject("LevelContainer").transform;

            List<string> allLines = new List<string>(levelFile.text.Split('\n'));
            LevelStarThresholds thresholds = new LevelStarThresholds { ThreeStars = int.MaxValue, TwoStars = int.MaxValue };

            // 1. Разделяем карту и данные о звездах
            int separatorIndex = allLines.FindIndex(line => line.Trim() == "---");
            List<string> levelRows = (separatorIndex == -1) ? allLines : allLines.Take(separatorIndex).ToList();

            // Парсим пороги для звезд
            ParseStarThresholds(allLines, separatorIndex, ref thresholds);

            // 2. Определяем размеры уровня
            int levelHeight = levelRows.Count;
            // Находим длину самой длинной строки для определения ширины
            int levelWidth = levelRows.Max(row => row.Length);

            // 3. Рассчитываем смещение (offset)
            // Мы хотим, чтобы центр уровня (width/2, -height/2) оказался в точке (0,0).
            // Поэтому каждый тайл нужно сместить влево на половину ширины и вверх на половину высоты.
            // Вычитаем 0.5f, чтобы центр сетки (а не угол) совпал с (0,0).
            Vector2 offset = new Vector2(
                -(levelWidth / 2.0f) + 0.5f,
                (levelHeight / 2.0f) - 0.5f
            );

            for (int y = 0; y < levelRows.Count; y++)
            {
                string row = levelRows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    char tileChar = row[x];
                    Vector2 position = new Vector2(x, -y) + offset;

                    // Пропускаем пустые символы и возврат каретки
                    if (tileChar == ' ' || tileChar == '\r') continue;

                    Instantiate(floorPrefab, position, Quaternion.identity, levelContainer);

                    switch ((TileType)tileChar)
                    {
                        case TileType.Wall:
                            Instantiate(wallPrefab, position, Quaternion.identity, levelContainer);
                            break;
                        case TileType.Box:
                            Instantiate(boxPrefab, position, Quaternion.identity, levelContainer);
                            break;
                        case TileType.Goal:
                            Instantiate(goalPrefab, position, Quaternion.identity, levelContainer);
                            break;
                        case TileType.Player:
                            Instantiate(playerPrefab, position, Quaternion.identity);
                            break;
                    }
                }
            }
            return thresholds;
        }

        private void ParseStarThresholds(List<string> allLines, int separatorIndex, ref LevelStarThresholds thresholds)
        {
            if (separatorIndex != -1 && allLines.Count > separatorIndex + 1)
            {
                string thresholdLine = allLines[separatorIndex + 1].Trim();
                string[] parts = thresholdLine.Split(',');

                if (parts.Length == 2 && int.TryParse(parts[0], out int threeStars) && int.TryParse(parts[1], out int twoStars))
                {
                    thresholds.ThreeStars = threeStars;
                    thresholds.TwoStars = twoStars;
                }
                else
                {
                    Debug.LogWarning("Не удалось прочитать пороги для звезд. Проверьте формат строки после '---'. Ожидается 'число,число'.");
                }
            }
        }

        public void ClearLevel()
        {
            if (levelContainer != null)
            {
                Destroy(levelContainer.gameObject);
            }

            // Уничтожаем игрока и ящики, которые не в контейнере
            var player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (player != null) Destroy(player);

            // На случай, если ящики не попали в контейнер
            var boxes = GameObject.FindGameObjectsWithTag(BOX_TAG);
            foreach (var box in boxes) Destroy(box);
        }
    }
}