using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance; // Singleton для легкого доступа

    public List<TextAsset> levelFiles; // Сюда перетащим все файлы уровней
    private int currentLevelIndex = 0;

    private LevelGenerator levelGenerator;
    private GameManager gameManager;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        levelGenerator = GetComponent<LevelGenerator>();
        gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        // Загружаем первый уровень при старте
        LoadLevel(currentLevelIndex);
    }
    
    private IEnumerator LoadLevelRoutine(int levelIndex)
    {
        levelGenerator.GenerateLevel(levelFiles[levelIndex]);

        yield return null; 

        gameManager.InitializeLevel();
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelFiles.Count)
        {
            currentLevelIndex = levelIndex;
            StartCoroutine(LoadLevelRoutine(currentLevelIndex));
        }
        else
        {
            Debug.Log("Все уровни пройдены!");
        }
    }

    public void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
}