using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class NetworkManager : SimulationBehaviour, IPlayerJoined
{
    [Header("Настройки спавна")]
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        // Автоматически запускаем Fusion при старте сцены в режиме Host (Вы сервер и игрок одновременно)
        // Если тестируете вдвоем, для второго игрока в меню запуска нужно будет выбирать Client
        StartMatchmaking();
    }

    private async void StartMatchmaking()
    {
        NetworkRunner runner = gameObject.GetComponent<NetworkRunner>();
        if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();

        var args = new StartGameArgs()
        {
            GameMode = GameMode.Host, 
            SessionName = "Room",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        await runner.StartGame(args);
    }

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
