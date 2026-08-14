using UnityEngine;
using Zenject;

public class PlayerInit : MonoBehaviour
{
    private GameDataModel _model;

    private CharacterController _characterController;
    private PlayerRagdoll _ragdoll;
    
    [SerializeField] private Transform _normalCameraTarget;

    public Transform NormalCameraTarget => _normalCameraTarget;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        _model.LastCheckpointPosition.Value = transform.position;
        _characterController = GetComponent<CharacterController>();
        _ragdoll = GetComponent<PlayerRagdoll>();
        _characterController.transform.SetParent(null);
    }

    public void RespawnPlayer()
    {
        _characterController.enabled = false;
        _characterController.transform.position = _model.LastCheckpointPosition.Value;
        _characterController.transform.rotation = _model.LastCheckpointRotation.Value;
        _characterController.enabled = true;
        _characterController.transform.SetParent(null);
        _ragdoll.ToggleRagdoll(false);
        
        Debug.Log($"Игрок респаунится на чекпоинте: {_model.LastCheckpointPosition.Value}");
    }
}
