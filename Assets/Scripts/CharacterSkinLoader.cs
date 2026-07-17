using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public class CharacterSkinLoader : MonoBehaviour
{
    [SerializeField] private Transform _skinPivot;

    private GameDataModel _model;
    private GameObject _currentSkinInstance;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        // Слушаем изменение адреса скина через UniRx.
        // Как только адрес изменился — запускаем асинхронную загрузку через UniTask.
        _model.CurrentSkinAddress
            .Subscribe(address => LoadSkinAsync(address).Forget())
            .AddTo(this);
    }

    private async UniTaskVoid LoadSkinAsync(string address)
    {
        ClearCurrentSkin();

        if (string.IsNullOrEmpty(address) || address == "default_skin") 
            return;

        try
        {
            // Ждем загрузку префаба напрямую через await. 
            // Метод .ToUniTask() адаптирует асинхронную операцию Addressables под UniTask.
            GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(address)
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            // Спавним визуал
            _currentSkinInstance = Instantiate(prefab, _skinPivot);
            _currentSkinInstance.transform.localPosition = Vector3.zero;
            _currentSkinInstance.transform.localRotation = Quaternion.identity;
        }
        catch (OperationCanceledException)
        {
            // Сюда код попадет, если объект уничтожили во время загрузки (например, вышли из игры)
            Debug.Log("Загрузка скина была отменена.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке скина: {e.Message}");
        }
    }

    private void ClearCurrentSkin()
    {
        if (_currentSkinInstance != null)
        {
            Destroy(_currentSkinInstance);
            _currentSkinInstance = null;
        }
        
        // UniTask/Addressables автоматически освобождают память, 
        // если мы используем правильные методы, но для надежности 
        // при работе с конкретными адресами можно вызвать:
        // Addressables.Release(prefab); 
        // В данном случае, так как мы не храним ссылку на префаб, достаточно очистить Instance.
    }
}
