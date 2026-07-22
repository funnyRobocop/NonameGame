using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UniRx;

public class ShopCharItemView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CharacterType _charType;
    [SerializeField] private int _price;

    [Header("UI References")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;

    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        _ = Observable.CombineLatest(
            _model.Coins,
            _model.PurchasedCharTypes.ObserveEveryValueChanged(x => x.Count),
            _model.CurrentCharType,
            (coins, purchasedCount, currentChar) => new { coins, currentChar })
        .Subscribe(state =>
        {
            var isPurchased = _model.PurchasedCharTypes.Contains(_charType);
            var isEquipped = state.currentChar == _charType;

            if (isEquipped)
            {
                _buttonText.text = "Экипировано";
                _actionButton.interactable = false;
            }
            else if (isPurchased)
            {
                _buttonText.text = "Надеть";
                _actionButton.interactable = true;
            }
            else
            {
                _buttonText.text = $"Купить ({_price})";
                _actionButton.interactable = state.coins >= _price;
            }
        })
        .AddTo(this);

        _actionButton.OnClickAsObservable()
            .Subscribe(_ => OnButtonClicked())
            .AddTo(this);
    }

    private void OnButtonClicked()
    {
        var isPurchased = _model.PurchasedCharTypes.Contains(_charType);

        if (!isPurchased)
        {
            _model.Coins.Value -= _price;
            _model.PurchasedCharTypes.Add(_charType);
            _model.CurrentCharType.Value = _charType;
            Debug.Log($"Char {_charType} успешно куплен!");
        }
        else
        {
            _model.CurrentCharType.Value = _charType;
            Debug.Log($"Char {_charType} экипирован!");
        }
    }
}
