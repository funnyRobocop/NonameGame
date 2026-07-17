using UnityEngine;
using Zenject;
using UniRx;
using UniRx.Triggers;

public class KillZoneTrigger : MonoBehaviour
{
    private GameDataModel _model;

    [Inject]
    public void Construct(GameDataModel model)
    {
        _model = model;
    }

    private void Start()
    {
        // Слушаем падение любого объекта в пропасть
        this.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("Player"))
            .Subscribe(other =>
            {
                // Перемещаем игрока на позицию последнего сохраненного чекпоинта
                other.transform.position = _model.LastCheckpointPosition.Value;
                
                // ЕСЛИ ВЫ ИСПОЛЬЗУЕТЕ Rigidbody:
                // При телепортации обязательно нужно сбрасывать накопленную скорость падения, 
                // иначе персонаж продолжит лететь вниз даже после респауна!
                if (other.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log("Игрок упал! Респаун на чекпоинте.");
            })
            .AddTo(this);
    }
}
