using UnityEngine;
using Fusion;

public class NetworkCheckpoint : NetworkBehaviour
{
    private bool _isActivated;

    private void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer) return;

        if (other.CompareTag("Player") && !_isActivated)
        {
            var playerController = other.GetComponent<NetworkPlayerController>();
            if (playerController != null)
            {
                // Передаем серверу команду запомнить позицию этого чекпоинта для игрока
                // Используем позицию самого чекпоинта (или пустой объект-точку чуть выше него)
                Vector3 savePosition = transform.position + Vector3.up * 1f;
                playerController.UpdateCheckpoint(savePosition);
                _isActivated = true;
                
                Debug.Log($"[Сервер] Игрок {Object.InputAuthority} сохранил чекпоинт на позиции {savePosition}");
            }
        }
    }
}
