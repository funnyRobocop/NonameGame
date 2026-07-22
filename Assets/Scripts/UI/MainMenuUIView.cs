using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;

public class MainMenuUIView : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _shopButton;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup _fadeScreen; // Компонент для плавного затемнения
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private string _gameSceneAddress = "GameScene";
    [SerializeField] private string _shopSceneAddress = "ShopScene";

    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        if (_fadeScreen != null)
        {
            _fadeScreen.alpha = 0f;
            _fadeScreen.blocksRaycasts = false;
        }

        _playButton.OnClickAsObservable()
            .First() 
            .Subscribe(_ => StartGameSequenceAsync().Forget())
            .AddTo(this);
        
        _shopButton.OnClickAsObservable()
            .First() 
            .Subscribe(_ => OnBackButtonClicked().Forget())
            .AddTo(this);
    }

    private async UniTaskVoid StartGameSequenceAsync()
    {
        var cancellationToken = this.GetCancellationTokenOnDestroy();

        if (_fadeScreen != null)
        {
            _fadeScreen.blocksRaycasts = true;
            
            await FadeAsync(0f, 1f, _fadeDuration, cancellationToken);
        }

        Debug.Log("Запуск загрузки уровня...");
        
        await Addressables.LoadSceneAsync(_gameSceneAddress)
            .ToUniTask(cancellationToken: cancellationToken);
    }

    private UniTask OnBackButtonClicked()
    {
        return Addressables.LoadSceneAsync(_shopSceneAddress).ToUniTask();
    }

    private async UniTask FadeAsync(float startAlpha, float endAlpha, float duration, System.Threading.CancellationToken token)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _fadeScreen.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _fadeScreen.alpha = endAlpha;
    }
}
