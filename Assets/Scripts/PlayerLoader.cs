using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public class PlayerLoader : MonoBehaviour
{
    [SerializeField] private Transform _charPivot;

    private GameDataModel _model;
    private GameObject _currentCharInstance;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private async UniTaskVoid LoadSkinAsync(CharacterType charType)
    {
        ClearCurrentSkin();

        if (charType == CharacterType.None)
        {
            Debug.Log("No CharType selected.");
            if (_model.PurchasedCharTypes.Count > 0)
            {
                Debug.Log("Loading first purchased CharType.");
                charType = _model.PurchasedCharTypes[0];
            }
            else
            {
                Debug.Log("No purchased CharTypes available.");
                return;
            }
        }

        try
        {
            string address = string.Empty;
            GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(address)
                .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

            _currentCharInstance = Instantiate(prefab, _charPivot);
            _currentCharInstance.transform.localPosition = Vector3.zero;
            _currentCharInstance.transform.localRotation = Quaternion.identity;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Загрузка скина была отменена.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка при загрузке скина: {e.Message}");
        }
    }

    private void ClearCurrentSkin()
    {
        if (_currentCharInstance != null)
        {
            Destroy(_currentCharInstance);
            _currentCharInstance = null;
        }

        // Addressables.Release(prefab);
    }
}
