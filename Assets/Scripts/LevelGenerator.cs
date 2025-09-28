using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    // --- Публичные поля для префабов (перетащите их в инспекторе) ---
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject playerPrefab;

    private Transform levelContainer;

    // --- Переменные для хранения уровня ---
    private List<string> levelRows = new List<string>();
    private GameObject playerInstance; // Храним ссылку на игрока

    // В будущем, чтобы хранить состояние ящиков и целей
    // private List<GameObject> boxInstances = new List<GameObject>();

    void Start()
    {
        
    }

    public void GenerateLevel(TextAsset levelFile)
    {
        ClearLevel(); // Сначала очищаем старый уровень

        // Создаем контейнер для порядка в иерархии
        levelContainer = new GameObject("LevelContainer").transform;
        
        List<string> levelRows = new List<string>(levelFile.text.Split('\n'));
        
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
                char tile = row[x];
                Vector2 position = new Vector2(x, -y) + offset;

                if (tile == ' ' || (int)tile == 13) continue;

                Instantiate(floorPrefab, position, Quaternion.identity, levelContainer);

                switch (tile)
                {
                    case '#':
                        Instantiate(wallPrefab, position, Quaternion.identity, levelContainer);
                        break;
                    case '$':
                        Instantiate(boxPrefab, position, Quaternion.identity, levelContainer);
                        break;
                    case '*':
                        Instantiate(goalPrefab, position, Quaternion.identity, levelContainer);
                        break;
                    case '@':
                        Instantiate(playerPrefab, position, Quaternion.identity);
                        break;
                }
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
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);
        
        // На случай, если ящики не попали в контейнер
        var boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach(var box in boxes) Destroy(box);
    }
}