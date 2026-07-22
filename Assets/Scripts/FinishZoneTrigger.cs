using System;
using UnityEngine;
using Zenject;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class FinishZoneTrigger : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private int _coinsReward = 50;
    [SerializeField] private int _scoreReward = 100;

    private GameDataModel _model;
    private GameUIView _gameUIView;

    [Inject]
    public void Construct(GameDataModel model, GameUIView gameUIView)
    {
        _model = model;
        _gameUIView = gameUIView;
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

        _model.Coins.Value += _coinsReward;
        _model.Score.Value += _scoreReward;

        _gameUIView.ShowLevelFinishView();

        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

        await Addressables.LoadSceneAsync("MainMenuScene").ToUniTask();
    }
}
