using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;

public class LevelCharacterSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineCamera _normalCamera;
    [SerializeField] private CinemachineCamera _ragdollCamera;
    [SerializeField] private FollowTransform _normalCameraTarget;
    [SerializeField] private FollowTransform _ragdollCameraTarget;

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
        var playerRagdoll = player.GetComponentInChildren<PlayerRagdoll>();
        playerRagdoll.Init(_normalCamera, _ragdollCamera);
        _normalCameraTarget.SetTarget(playerRagdoll.NormalCameraTarget);
        _ragdollCameraTarget.SetTarget(playerRagdoll.RagdollCameraTarget);
    }
}
