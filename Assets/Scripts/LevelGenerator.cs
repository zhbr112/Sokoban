using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sokoban
{
    public class LevelGenerator : MonoBehaviour
    {

        private enum TileType
        {
            Wall = '#',
            Box = '$',
            Goal = '*',
            Player = '@',
            Floor = ' ',
        }

        private const string PLAYER_TAG = "Player";
        private const string BOX_TAG = "Box";

        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject boxPrefab;
        [SerializeField] private GameObject goalPrefab;
        [SerializeField] private GameObject playerPrefab;

        private Transform levelContainer;

        public struct LevelStarThresholds
        {
            public int ThreeStars;
            public int TwoStars;
        }

        public LevelStarThresholds GenerateLevel(TextAsset levelFile)
        {
            ClearLevel();

            levelContainer = new GameObject("LevelContainer").transform;

            List<string> allLines = new List<string>(levelFile.text.Split('\n'));
            LevelStarThresholds thresholds = new LevelStarThresholds { ThreeStars = int.MaxValue, TwoStars = int.MaxValue };

            int separatorIndex = allLines.FindIndex(line => line.Trim() == "---");
            List<string> levelRows = (separatorIndex == -1) ? allLines : allLines.Take(separatorIndex).ToList();

            ParseStarThresholds(allLines, separatorIndex, ref thresholds);

            int levelHeight = levelRows.Count;

            int levelWidth = levelRows.Max(row => row.Length-1);

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

            var player = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (player != null) Destroy(player);

            var boxes = GameObject.FindGameObjectsWithTag(BOX_TAG);
            foreach (var box in boxes) Destroy(box);
        }
    }
}