using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class GameUIView : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private LevelFinishView _levelFinishView;
    [SerializeField] private CanvasGroup _fadeScreen;
    [SerializeField] private float _fadeInDuration = 0.5f;

    private GameDataModel _model;
    
    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }
    
    void Start()
    {
        StartCountdownSequenceAsync().Forget();
    }

    private async UniTaskVoid StartCountdownSequenceAsync()
    {
        CursorController.LockCursor(true);
        _model.IsInputLocked.Value = true;

        if (_fadeScreen != null)
        {
            _fadeScreen.alpha = 1f;
            _fadeScreen.blocksRaycasts = true;

            var elapsedTime = 0f;
            while (elapsedTime < _fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                _fadeScreen.alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeInDuration);
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            _fadeScreen.alpha = 0f;
            _fadeScreen.blocksRaycasts = false;
        
            for (var i = 3; i > 0; i--)
            {
                _countdownText.text = i.ToString();
                
                // PlayerLoopTiming.Update привязывает таймер к кадрам Unity (безопасно)
                // уничтожение объекта (this.GetCancellationTokenOnDestroy()) автоматически прервет таймер, чтобы не было утечек памяти
                await UniTask.Delay(TimeSpan.FromSeconds(1), delayTiming: PlayerLoopTiming.Update, cancellationToken: 
                    this.GetCancellationTokenOnDestroy());
            }

            _countdownText.text = "GO!";

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            _countdownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!_levelFinishView.IsVisible)
            {
                CursorController.LockCursor(false);
            }
        }
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        { 
            if (!_levelFinishView.IsVisible)
            {
                CursorController.LockCursor(true);
            }
        }
    }

    public void ShowLevelFinishView()
    {
        if (_levelFinishView != null)
        {
            _levelFinishView.Show();
            CursorController.LockCursor(false);
        }
    }
}
