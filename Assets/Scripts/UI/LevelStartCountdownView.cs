using System;
using UnityEngine;
using TMPro;
using Zenject;
using Cysharp.Threading.Tasks;

public class LevelStartCountdownTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countdownText;
    
    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        StartCountdownSequenceAsync().Forget();
    }

    private async UniTaskVoid StartCountdownSequenceAsync()
    {
        //await UniTask.WaitUntil(() => PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayers);
        // Отсчет: 3... 2... 1...
        for (int i = 3; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            
            // PlayerLoopTiming.Update привязывает таймер к кадрам Unity (безопасно)
            // уничтожение объекта (this.GetCancellationTokenOnDestroy()) автоматически прервет таймер, чтобы не было утечек памяти
            await UniTask.Delay(TimeSpan.FromSeconds(1), delayTiming: PlayerLoopTiming.Update, cancellationToken: 
                this.GetCancellationTokenOnDestroy());
        }

        // Команда СТАРТ
        _countdownText.text = "GO!";

        // Ждем еще полсекунды, чтобы надпись "GO!" успели заметить
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
        
        // Прячем текст
        _countdownText.gameObject.SetActive(false);
    }
}
