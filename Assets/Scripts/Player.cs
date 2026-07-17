using UnityEngine;
using Zenject;

public class Player : MonoBehaviour
{
    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Awake()
    {
        // При старте игры записываем текущую позицию игрока как самый первый чекпоинт
        _model.LastCheckpointPosition.Value = transform.position;
    }
}
