using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

// Реализуем интерфейс глобальных сетевых событий Fusion 2
public class NetworkManager : SimulationBehaviour, ISceneLoadDone
{
    [Header("Настройки спавна")]
    [SerializeField] private NetworkObject playerPrefab; 
    [SerializeField] private Transform spawnPoint;

    public void SceneLoadDone(in SceneLoadDoneArgs sceneInfo)
    {
        // Спавнить сетевые объекты имеет право только Сервер (Host)
        if (Runner.IsServer)
        {
            // Поскольку мы уехали из меню, ищем точку спавна на загруженной карте по имени
            if (spawnPoint == null)
            {
                GameObject spawnerObj = GameObject.Find("PlayerSpawner");
                if (spawnerObj != null)
                {
                    spawnPoint = spawnerObj.transform;
                }
            }

            // Вычисляем координаты безопасного появления
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.up * 5f;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            // Получаем ссылку на локального игрока
            PlayerRef localPlayer = Runner.LocalPlayer;

            Debug.Log($"[Глобальный Спавнер] Сцена карты готова. Создаем корову для игрока {localPlayer.PlayerId} в координатах {spawnPos}...");

            // Спавним корову на сервере и передаем клиенту InputAuthority (права управления)
            Runner.Spawn(playerPrefab, spawnPos, spawnRot, localPlayer);
        }
    }
}