using UnityEngine;
using Fusion;

namespace NonameGame
{
    public class NetworkKillzone : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (Runner == null || !Runner.IsServer) return;

            var playerController = other.GetComponent<NetworkPlayerController>();
            
            if (playerController != null)
            {
                Debug.Log($"[Killzone] Сервер зафиксировал падение объекта: {other.gameObject.name}. Отправляем игрока на чекпоинт.");
                
                playerController.RespawnAtCheckpoint();
            }
        }
    }
}