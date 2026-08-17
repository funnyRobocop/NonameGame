using UnityEngine;
using Fusion;

// Наследуемся от NetworkBehaviour, чтобы безопасно хранить счетчик мест на Сервере
public class NetworkFinishZone : NetworkBehaviour
{
    // Сетевая переменная-счетчик. Она считает, сколько игроков уже пересекло черту
    [Networked] private int _finishedPlayersCount { get; set; }

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            _finishedPlayersCount = 0; // Обнуляем счетчик при старте раунда
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Фиксацию победы проводит СТРОГО Сервер (Host), чтобы исключить читы и споры за 1-е место
        if (!Runner.IsServer) return;

        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            
            // Если игрок зашел в триггер и он ЕЩЕ НЕ финишировал в этом раунде
            if (playerController != null && !playerController.IsFinished)
            {
                // Увеличиваем счетчик мест на 1
                _finishedPlayersCount++;

                int assignedPlace = _finishedPlayersCount;
                
                Debug.Log($"[МЕНЕДЖЕР ФИНИША] Игрок {other.gameObject.name} занял {assignedPlace} место!");

                // Передаем статус финиша в контроллер конкретного персонажа
                playerController.SetPlayerFinished(assignedPlace);
            }
        }
    }
}
