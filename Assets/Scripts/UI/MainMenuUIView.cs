using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class MainMenuUIView : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _shopButton;


    void Start()
    {
        CursorController.LockCursor(false);

        _playButton.OnClickAsObservable()
            .Subscribe(_ => OnPlayButtonClicked().Forget())
            .AddTo(this);
            
        _shopButton.OnClickAsObservable()
            .Subscribe(_ => OnShopButtonClicked().Forget())
            .AddTo(this);
    }

    public UniTask OnPlayButtonClicked()
    {
        return Addressables.LoadSceneAsync("GameScene").ToUniTask();
    }

    public UniTask OnShopButtonClicked()
    {
        return Addressables.LoadSceneAsync("ShopScene").ToUniTask();
    }
}
