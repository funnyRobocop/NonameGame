using UnityEngine;
using Fusion;

public class NetworkKillzone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer) return;

        // Ищем сетевой контроллер. Если упала кость рэгдолла, 
        // метод GetComponentInParent найдет скрипт на самом верхнем корневом объекте коровы
        var playerController = other.GetComponent<NetworkPlayerController>() ?? other.GetComponentInParent<NetworkPlayerController>();
        
        if (playerController != null)
        {
            Debug.Log($"[Killzone] Сервер зафиксировал падение объекта: {other.gameObject.name}. Отправляем игрока на чекпоинт.");
            
            playerController.RespawnAtCheckpoint();
        }
    }
}