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
                var playerInit= other.GetComponentInParent<PlayerInit>();
                if (playerInit != null)
                {
                    playerInit.RespawnPlayer();
                }
                else
                {
                    Debug.LogWarning("PlayerInit component not found on the player object.");
                }

                Debug.Log("Игрок упал! Респаун на чекпоинте.");
            })
            .AddTo(this);
    }
}
