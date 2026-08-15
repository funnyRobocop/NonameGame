using UnityEngine;
using Fusion;

public class NetworkKillzone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Обрабатываем смерть строго на Сервере
        if (!Runner.IsServer) return;

        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            if (playerController != null)
            {
                Debug.Log("[Killzone] Игрок упал в воду! Отправляем на чекпоинт.");
                
                // Даем команду персонажу телепортироваться на сохраненную точку
                playerController.RespawnAtLastCheckpoint();
            }
        }
    }
}