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

    private void Awake()
    {
        // При старте игры записываем текущую позицию игрока как самый первый чекпоинт
        _model.LastCheckpointPosition.Value = transform.position;
        _characterController = GetComponentInChildren<CharacterController>();
    }

    public void RespawnPlayer()
    {
        // Перемещаем игрока на позицию последнего сохраненного чекпоинта
        _characterController.enabled = false;
        _characterController.transform.position = _model.LastCheckpointPosition.Value;
        _characterController.enabled = true;
        Debug.Log($"Игрок респаунится на чекпоинте: {_model.LastCheckpointPosition.Value}");
    }
}
