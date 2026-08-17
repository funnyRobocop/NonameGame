using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class NetworkManager : SimulationBehaviour, IPlayerJoined
{
    [Header("Настройки спавна")]
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    // вызывается Fusion 2 автоматически на сервере, когда любой игрок входит в комнату
    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        // Спавнить объекты в Host-режиме разрешено ТОЛЬКО серверу (StateAuthority)
        if (Runner.IsServer)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.up * 2f;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            // Вместо GameObject.Instantiate во Fusion используется ТОЛЬКО Runner.Spawn!
            // Последний аргумент (player) критически важен: он дает вошедшему игроку InputAuthority (право управлять)
            Runner.Spawn(playerPrefab, spawnPos, spawnRot, player);
            Debug.Log($"[Сеть] Игрок {player.PlayerId} зашел!" + spawnPos);
        }
    }
}
