using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using System.Threading.Tasks;

public class NetworkLobbyManager : MonoBehaviour
{
    [Header("Элементы интерфейса UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField roomNameInputField;

    [Header("Настройки сетевых сцен")]
    [SerializeField] private int gameSceneIndex = 3; // Индекс нашей игровой карты в Build Settings

    private NetworkRunner _runner;

    private void Start()
    {
        // Находим или добавляем NetworkRunner на этом объекте
        _runner = GetComponent<NetworkRunner>();
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();

        // Привязываем методы к кнопкам UI
        if (hostButton != null) hostButton.onClick.AddListener(() => StartGameSession(GameMode.Host));
        if (clientButton != null) clientButton.onClick.AddListener(() => StartGameSession(GameMode.Client));
    }

    private async void StartGameSession(GameMode mode)
    {
        // Блокируем кнопки, чтобы игрок не нажал их дважды во время загрузки
        SetUiState(false);

        // Получаем имя комнаты из инпута. Если пусто — ставим дефолтное
        string roomName = roomNameInputField != null && !string.IsNullOrEmpty(roomNameInputField.text) 
            ? roomNameInputField.text 
            : "Room";

        Debug.Log($"[Сеть] Запуск сессии в режиме {mode}. Имя комнаты: {roomName}");

        // Настраиваем параметры запуска во Fusion 2
        var args = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            // Важно: Указываем индекс игровой сцены (1), куда Fusion перенесет всех игроков после подключения
            Scene = SceneRef.FromIndex(gameSceneIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        // Запускаем сетевой поток
        var result = await _runner.StartGame(args);

        if (result.Ok)
        {
            Debug.Log("[Сеть] Подключение к сессии прошло успешно!");
            // Если мы запустили режим Host, Fusion автоматически загрузит сцену по индексу gameSceneIndex
            // и перетащит туда всех подключившихся клиентов
        }
        else
        {
            Debug.LogError($"[Сбой сети] Не удалось запустить сессию: {result.ShutdownReason}");
            SetUiState(true); // Возвращаем кнопкам активность при ошибке
        }
    }

    private void SetUiState(bool isEnabled)
    {
        if (hostButton != null) hostButton.interactable = isEnabled;
        if (clientButton != null) clientButton.interactable = isEnabled;
        if (roomNameInputField != null) roomNameInputField.interactable = isEnabled;
    }
}
