using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

namespace Sokoban
{

    [System.Serializable]
    public class UserCredentials
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string token;
    }

    [System.Serializable]
    public class GameResultDto
    {
        public int TotalStars;
        public int TotalMoves;
        public int TotalTime;
    }

    [System.Serializable]
    public class LeaderboardEntryDto
    {
        public string username;
        public int totalStars;
        public int totalMoves;
        public int totalTime;
    }

    [System.Serializable]
    public class LeaderboardResponse
    {
        public LeaderboardEntryDto[] entries;
    }

    public class AuthManager : MonoBehaviour
    {
        public static AuthManager instance { get; private set; }

        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public TextMeshProUGUI statusText;

        private string apiBaseUrl = "https://sokoban.1zq.ru";

        public static string AuthToken { get; private set; }
        public static bool IsGuestMode { get; private set; } = false;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogWarning("Обнаружен еще один экземпляр AuthManager. Уничтожается.", gameObject);
                Destroy(gameObject);
            }
        }

        public void OnRegisterButtonClicked()
        {
            statusText.text = "Регистрация...";
            StartCoroutine(RegisterCoroutine());
        }

        public void OnLoginButtonClicked()
        {
            statusText.text = "Вход...";
            StartCoroutine(LoginCoroutine());
        }

        public void OnGuestModeButtonClicked()
        {
            StartAsGuest();
        }

        public void StartAsGuest()
        {
            IsGuestMode = true;
            AuthToken = null;
            statusText.text = "Гостевой режим. Результаты не будут сохранены.";
            Debug.Log("Guest mode activated.");

            UIManager.instance.OnLoginSuccess();
        }

        public void OnFetchLeaderboardClicked()
        {
            statusText.text = "Загрузка таблицы лидеров...";
            StartCoroutine(FetchLeaderboardCoroutine());
        }

        private IEnumerator RegisterCoroutine()
        {
            UserCredentials credentials = new UserCredentials
            {
                username = usernameInput.text,
                password = passwordInput.text
            };
            string json = JsonUtility.ToJson(credentials);

            using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/register", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    statusText.text = "Регистрация успешна! Теперь вы можете войти.";
                    Debug.Log("Registration successful!");
                }
                else
                {
                    string serverMessage = request.downloadHandler.text;
                    Debug.LogError($"Registration failed: {request.error} | {serverMessage}");

                    if (serverMessage != null && serverMessage.Contains("already exists"))
                    {
                        statusText.text = "Пользователь с таким именем уже существует.";
                    }
                    else
                        statusText.text = "Ошибка регистрации. Попробуйте позже.";
                }
            }
        }

        private IEnumerator LoginCoroutine()
        {
            UserCredentials credentials = new UserCredentials
            {
                username = usernameInput.text,
                password = passwordInput.text
            };
            string json = JsonUtility.ToJson(credentials);

            using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/login", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text;
                    AuthResponse authResponse = JsonUtility.FromJson<AuthResponse>(responseJson);

                    AuthToken = authResponse.token;
                    statusText.text = "Вход выполнен успешно!";
                    Debug.Log($"Login successful! Token: {AuthToken}");

                    UIManager.instance.OnLoginSuccess();

                    StartCoroutine(GetSecureDataCoroutine());
                }
                else
                {
                    statusText.text = "Неверный логин или пароль.";
                    Debug.LogError($"Login failed: {request.error} | {request.downloadHandler.text}");
                }
            }
        }

        public void TrySubmitGameResult(int totalStars, int totalMoves, int totalTime)
        {
            if (IsGuestMode)
            {
                Debug.Log("В гостевом режиме результаты игры не сохраняются.");
                return;
            }

            if (string.IsNullOrEmpty(AuthToken))
            {
                Debug.LogWarning("Невозможно отправить результаты: пользователь не авторизован.");
                return;
            }

            StartCoroutine(SubmitGameResultCoroutine(totalStars, totalMoves, totalTime));
        }

        private IEnumerator SubmitGameResultCoroutine(int totalStars, int totalMoves, int totalTime)
        {
            GameResultDto gameResult = new GameResultDto { TotalStars = totalStars, TotalMoves = totalMoves, TotalTime = totalTime };
            string json = JsonUtility.ToJson(gameResult);

            using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/leaderboard/submit", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + AuthToken);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Результаты игры успешно отправлены на сервер!");
                }
                else
                {
                    Debug.LogError($"Ошибка при отправке результатов игры: {request.responseCode} - {request.downloadHandler.text}");
                }
            }
        }

        private IEnumerator FetchLeaderboardCoroutine()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/leaderboard"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;

                    string wrappedJson = "{\"entries\":" + jsonResponse + "}";
                    LeaderboardResponse leaderboardData = JsonUtility.FromJson<LeaderboardResponse>(wrappedJson);

                    if (leaderboardData != null && leaderboardData.entries != null)
                    {
                        Debug.Log($"Загружено {leaderboardData.entries.Length} записей в таблице лидеров.");
                        UIManager.instance.PopulateLeaderboard(leaderboardData.entries);
                    }
                    statusText.text = "";
                }
                else
                {
                    statusText.text = "Ошибка загрузки таблицы лидеров.";
                    Debug.LogError($"Ошибка при загрузке таблицы лидеров: {request.error} | {request.downloadHandler.text}");
                }
            }
        }

        private IEnumerator GetSecureDataCoroutine()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/securedata"))
            {

                request.SetRequestHeader("Authorization", "Bearer " + AuthToken);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Secure data response: " + request.downloadHandler.text);

                }
                else
                {
                    Debug.LogError("Failed to get secure data: " + request.error);
                }
            }
        }
    }
}