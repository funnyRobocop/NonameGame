using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ShopUIView : MonoBehaviour
{
    [SerializeField] private Button _backButton;

    void Start()
    {
        CursorController.LockCursor(false);

        _backButton.OnClickAsObservable()
            .Subscribe(_ => OnBackButtonClicked().Forget())
            .AddTo(this);
    }

    private UniTask OnBackButtonClicked()
    {
        return Addressables.LoadSceneAsync("MainMenuScene").ToUniTask();
    }
}
