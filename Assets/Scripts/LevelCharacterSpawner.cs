using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;

public class LevelCharacterSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;

    private CharacterFactory _characterFactory;

    [Inject]
    public void Construct(CharacterFactory characterFactory)
    {
        _characterFactory = characterFactory;
    }

    private void Start()
    {
        SpawnPlayerAsync().Forget();
    }

    private async UniTaskVoid SpawnPlayerAsync()
    {
        GameObject player = await _characterFactory.CreateCharacterAsync(_spawnPoint);
    }
}
