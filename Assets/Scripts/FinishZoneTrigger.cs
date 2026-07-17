using System;
using UnityEngine;
using TMPro;
using Zenject;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class FinishZoneTrigger : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private int _coinsReward = 50;
    [SerializeField] private int _scoreReward = 100;

    [Header("UI")] 
    [SerializeField] private LevelFinishView _levelFinishView;

    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        this.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("Player") && !_model.IsLevelFinished.Value)
            .First() // Срабатывает строго 1 раз
            .Subscribe(other =>
            {
                FinishRoundSequenceAsync().Forget();
            })
            .AddTo(this);
    }

    private async UniTaskVoid FinishRoundSequenceAsync()
    {
        Debug.Log("Финиш достигнут!");
        
        _model.IsLevelFinished.Value = true;
        _model.IsInputLocked.Value = true;

        // 2. Начисляем заслуженные баллы и монеты
        _model.Coins.Value += _coinsReward;
        _model.Score.Value += _scoreReward;

        _levelFinishView.Show();

        // 4. Ждем 4 секунды, чтобы игрок порадовался успеху (UniTask)
        await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: this.GetCancellationTokenOnDestroy());

        // 5. Загружаем Главное Меню / Магазин (через Addressables или обычный SceneManager)
        Debug.Log("Загрузка главного меню...");

        //await Addressables.LoadSceneAsync("MainMenuSCene").ToUniTask();

        await SceneManager.LoadSceneAsync("MainMenuScene");
    }
}
