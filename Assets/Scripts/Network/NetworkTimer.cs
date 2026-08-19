using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkTimer : NetworkBehaviour
{
    [Header("Настройки времени (в секундах)")]
    [SerializeField] private float roundDuration = 60f;

    [Header("Элементы UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float _remainingTime;

    // Сетевая переменная Fusion. Хранит точное время окончания раунда по часам сервера
    [Networked] private TickTimer _roundTimer { get; set; }
    
    [Networked] private NetworkBool _isTimeUp { get; set; }

    public override void Spawned()
    {
        // Таймер запускает строго Сервер в момент начала раунда
        if (Runner.IsServer)
        {
            // Создаем таймер Fusion от текущего момента на количество секунд roundDuration
            _roundTimer = TickTimer.CreateFromSeconds(Runner, roundDuration);
            _isTimeUp = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Проверяем состояние таймера каждый сетевой тик
        if (_roundTimer.Expired(Runner) && !_isTimeUp)
        {
            if (Runner.IsServer)
            {
                _isTimeUp = true;
                OnRoundTimeUp();
            }
        }
    }

    // Этот метод срабатывает каждый графический кадр Unity у всех игроков
    private void Update()
    {
        if (timerText == null) return;
        
        if (Object == null || !Object.IsValid) return;

        if (_isTimeUp)
        {
            timerText.text = "ВРЕМЯ ВЫШЛО!";
            return;
        }

        // Получаем оставшееся время в секундах из сетевого таймера Fusion
        float? remainingTime = _roundTimer.RemainingTime(Runner);

        if (remainingTime.HasValue)
        {
            if (remainingTime.Value == _remainingTime)
                return;

            _remainingTime = remainingTime.Value;

            // Форматируем секунды в красивый вид минут и секунд (например, 01:23)
            int minutes = Mathf.FloorToInt(_remainingTime / 60f);
            int seconds = Mathf.FloorToInt(_remainingTime % 60f);
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Метод вызывается на сервере, когда время полностью истекло
    private void OnRoundTimeUp()
    {
        Debug.Log("[МЕНЕДЖЕР] Время раунда истекло! Завершаем матч.");
        
        // Рассылаем всем игрокам RPC команду о принудительном завершении
        RPC_EndMatch();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndMatch()
    {
        // Отключаем ввод у всех коров на сцене, чтобы они застыли
        var allPlayers = FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            player.IsFinished = true;
        }

        // Выводим курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Через 3 секунды после надписи "Время вышло" автоматически выкидываем всех в меню
        Invoke(nameof(AutoReturnToMenu), 3f);
    }

    private async void AutoReturnToMenu()
    {
        if (Runner != null)
        {
            await Runner.Shutdown();
        }
        SceneManager.LoadScene(0);
    }
}