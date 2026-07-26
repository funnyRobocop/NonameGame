using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Unity.Cinemachine;

public class ShopCharItemView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CharacterType _charType;
    [SerializeField] private int _price;
    [SerializeField] private bool _isRemoteSkin;

    [Header("UI References")]
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _buttonText;

    private GameDataModel _model;
    private CharacterDatabase _db;

    [Inject]
    public void Construct(GameDataModel model, CharacterDatabase db)
    {
        _model = model;
        _db = db;
    }

    private void Start()
    {
        if (_isRemoteSkin)
        {
            CheckRemoteAvailabilityAsync().Forget();
        }
        else
        {
            InitRegularShopItem();
        }

    }

    private async UniTaskVoid CheckRemoteAvailabilityAsync()
    {
        var config = _db.GetCharacterTypeData(_charType);

        if (config == null)
        {
            Debug.LogError($"Скин с ID {_charType} не найден в базе данных!");
            gameObject.SetActive(false);
            return;
        }

        var _skinAddress = config.AdressableKey;

        // Проверяем, скачан ли скин уже в кэш устройства
        long downloadSize = await Addressables.GetDownloadSizeAsync(_skinAddress).ToUniTask();

        if (downloadSize == 0)
        {
            // Размер загрузки = 0, значит файл УЖЕ в кэше. Интернет не нужен!
            InitRegularShopItem();
            return;
        }

        // Если файла нет в кэше, проверяем связь с сервером (пытаемся обновить каталог)
        try
        {
            // Пытаемся проверить зависимости файла на сервере с коротким таймаутом
            var handle = Addressables.DownloadDependenciesAsync(_skinAddress);
            await handle.ToUniTask().Timeout(System.TimeSpan.FromSeconds(3)); // Ждем ответ сервера 3 секунды

            // Если код прошел дальше — сервер ответил, скин можно качать!
            InitRegularShopItem();
        }
        catch (System.Exception)
        {
            // если таймаут истек или нет интернета
            Debug.LogWarning($"Ремоут скин {_skinAddress} недоступен. Скрываем товар.");
            gameObject.SetActive(false); // Просто выключаем кнопку этого скина в магазине
        }
    }

    private void InitRegularShopItem()
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
