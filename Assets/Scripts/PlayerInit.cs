using UnityEngine;
using Zenject;

public class PlayerInit : MonoBehaviour
{
    private GameDataModel _model;

    private CharacterController _characterController;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        _model.LastCheckpointPosition.Value = transform.position;
        _characterController = GetComponent<CharacterController>();
        _characterController.transform.SetParent(null);
    }

    public void RespawnPlayer()
    {
        _characterController.enabled = false;
        _characterController.transform.position = _model.LastCheckpointPosition.Value;
        _characterController.transform.rotation = _model.LastCheckpointRotation.Value;
        _characterController.enabled = true;
        _characterController.transform.SetParent(null);
        
        Debug.Log($"Игрок респаунится на чекпоинте: {_model.LastCheckpointPosition.Value}");
    }
}
