using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;

public class LevelCharacterSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineCamera _normalCamera;

    private CharacterFactory _characterFactory;

    [Inject]
    public void Construct(CharacterFactory characterFactory)
    {
        _characterFactory = characterFactory;
    }

    private void Start()
    {
        //SpawnPlayerAsync().Forget();
    }


    private async UniTaskVoid SpawnPlayerAsync()
    {
        GameObject player = await _characterFactory.CreateCharacterAsync(_spawnPoint);
        var playerInit = player.GetComponentInChildren<PlayerInit>();
        _normalCamera.Target.TrackingTarget = playerInit.NormalCameraTarget;
    }
}
