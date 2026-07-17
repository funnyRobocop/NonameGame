using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UniRx;

public class ShopItemView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string _skinAddress = "Skins/RedHero";
    [SerializeField] private int _price = 50;

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
        // 1. СЛУШАЕМ ИЗМЕНЕНИЯ СОСТОЯНИЯ ДЛЯ ОБНОВЛЕНИЯ UI КНОПКИ
        // Объединяем три потока: изменение монет, список купленных и текущий скин.
        // Кнопка будет пересчитывать свой внешний вид при любом из этих событий.
        Observable.CombineLatest(
            _model.Coins,
            _model.PurchasedSkins.ObserveEveryValueChanged(x => x.Count), // Следим за размером коллекции
            _model.CurrentSkinAddress,
            (coins, purchasedCount, currentSkin) => new { coins, currentSkin })
        .Subscribe(state =>
        {
            bool isPurchased = _model.PurchasedSkins.Contains(_skinAddress);
            bool isEquipped = state.currentSkin == _skinAddress;

            if (isEquipped)
            {
                _buttonText.text = "Экипировано";
                _actionButton.interactable = false; // Кнопка не активна, уже надето
            }
            else if (isPurchased)
            {
                _buttonText.text = "Надеть";
                _actionButton.interactable = true;
            }
            else
            {
                _buttonText.text = $"Купить ({_price})";
                // Кнопка активна, только если хватает монет
                _actionButton.interactable = state.coins >= _price;
            }
        })
        .AddTo(this);

        // 2. ОБРАБОТКА КЛИКА ПО КНОПКЕ ЧЕРЕЗ UniRx
        _actionButton.OnClickAsObservable()
            .Subscribe(_ => OnButtonClicked())
            .AddTo(this);
    }

    private void OnButtonClicked()
    {
        bool isPurchased = _model.PurchasedSkins.Contains(_skinAddress);

        if (!isPurchased)
        {
            // Покупка
            _model.Coins.Value -= _price;
            _model.PurchasedSkins.Add(_skinAddress);
            _model.CurrentSkinAddress.Value = _skinAddress;
            Debug.Log($"Скин {_skinAddress} успешно куплен!");
        }
        else
        {
            // Просто экипировка
            _model.CurrentSkinAddress.Value = _skinAddress;
            Debug.Log($"Скин {_skinAddress} экипирован!");
        }
    }
}
