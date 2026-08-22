using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Реализуем интерфейс глобальных сетевых событий Fusion 2
public class NetworkManager : SimulationBehaviour, ISceneLoadDone, IPlayerJoined
{
    [Header("Настройки спавна")]
    [SerializeField] private NetworkObject playerPrefab;

    [Header("Настройки спавна Сетевых Ловушек")]
    // Перетащите сюда в инспекторе префаб вашего движущегося барьера!
    [SerializeField] private NetworkObject barrierPrefab; 
    // Перетащите сюда префаб бампера (красного столба)!
    [SerializeField] private NetworkObject bumperPrefab;  

    // Список подключившихся игроков, которые ждут, пока сервер загрузит карту
    private List<PlayerRef> _pendingPlayers = new List<PlayerRef>();
    private bool _isSceneLoaded = false;

    // 1. СЕТЕВОЕ СОБЫТИЕ: Срабатывает на Сервере каждый раз, когда заходит ЛЮБОЙ игрок
    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer) return;

        if (!_isSceneLoaded)
        {
            if (!_pendingPlayers.Contains(player)) _pendingPlayers.Add(player);
            Debug.Log($"[Спавнер] Игрок {player.PlayerId} добавлен в очередь ожидания загрузки сцены.");
        }
        else
        {
            Debug.Log($"[Спавнер] Сцена уже готова! Мгновенно спавним зашедшего клиента {player.PlayerId}.");        
            var spawner = FindAnyObjectByType<PlayerSpawner>();
            SpawnPlayerObject(player, spawner);
        }
    }

    // 2. СЕТЕВОЕ СОБЫТИЕ: Срабатывает, когда сервер полностью загрузил геометрию карты
    public void SceneLoadDone(in SceneLoadDoneArgs sceneInfo)
    {
        if (!Runner.IsServer) return;
        
        _isSceneLoaded = true;

        SpawnObstacles();
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {        
        var spawner = FindAnyObjectByType<PlayerSpawner>();

        Debug.Log($"[Спавнер] Сервер загрузил карту. Спавним игроков из стартовой очереди: {_pendingPlayers.Count}");

        // Спавним персонажей для всех, кто ждал загрузки в меню (включая Хоста!)
        foreach (PlayerRef player in _pendingPlayers)
        {
            SpawnPlayerObject(player, spawner);
        }
        
        _pendingPlayers.Clear();
    }

    private void SpawnPlayerObject(PlayerRef player, PlayerSpawner spawner)
    {
        Debug.Log($"[СЕТЕВОЙ СПАВН] Создание игрока для Player ID: {player.PlayerId}");

        // Спавним игрока и отдаем права ввода (InputAuthority) конкретно зашедшему плееру
        Runner.Spawn(playerPrefab, spawner.GetNext().position, spawner.GetNext().rotation, player);
    }

    private void SpawnObstacles()
    {
        var spawner = FindAnyObjectByType<ObstaclesSpawnDataContainer>();
        foreach (var item in spawner.Obstacles)
        {
            // Права StateAuthority автоматически остаются за Сервером (PlayerRef.None) 
            Runner.Spawn(item.prefab, item.point.position, item.point.rotation);
        }
    }
}