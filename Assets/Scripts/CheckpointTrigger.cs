using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Zenject;

public class CheckpointTrigger : MonoBehaviour
{

    private bool _isActivated;
    
    private GameDataModel _model;


    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        this.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("Player") && !_isActivated)
            .Subscribe(_ =>
            {
                _isActivated = true;
                
                _model.LastCheckpointPosition.Value = transform.position + Vector3.up * 1f; 
                _model.LastCheckpointRotation.Value = transform.rotation;
                
                Debug.Log($"Чекпоинт сохранен: {transform.position}");
                
                // Здесь можно запустить анимацию флага или смену цвета ворот
            })
            .AddTo(this);
    }
}
