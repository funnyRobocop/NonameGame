using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class CharacterFactory
{
    private readonly DiContainer _container;
    private readonly CharacterDatabase _db;
    private readonly GameDataModel _model;


    public CharacterFactory(DiContainer container, CharacterDatabase db, GameDataModel model)
    {
        _container = container;
        _db = db;
        _model = model;
    }

    public async UniTask<GameObject> CreateCharacterAsync(Transform spawnPoint)
    {
        var currentCharType = _model.CurrentCharType.Value;
        var config = _db.GetCharacterTypeData(currentCharType);

        if (config == null)
        {
            Debug.LogError($"Скин с ID {currentCharType} не найден в базе данных!");
            return null;
        }

        var bodyPrefab = await Addressables.LoadAssetAsync<GameObject>(config.AdressableKey).ToUniTask();
        GameObject characterInstance = _container.InstantiatePrefab(bodyPrefab, spawnPoint.position, spawnPoint.rotation, null);

        // Достраиваем персонажа (Одежда)

        return characterInstance;
    }
}
