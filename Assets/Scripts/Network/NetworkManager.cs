using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// Реализуем интерфейс глобальных сетевых событий Fusion 2
public class NetworkManager : SimulationBehaviour, ISceneLoadDone, IPlayerJoined
{
    [Header("Настройки спавна")]
    [SerializeField] private NetworkObject playerPrefab; 
    [SerializeField] private Transform _spawnPoint;

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
            SpawnPlayerObject(player);
        }
    }

    // 2. СЕТЕВОЕ СОБЫТИЕ: Срабатывает, когда сервер полностью загрузил геометрию карты
    public void SceneLoadDone(in SceneLoadDoneArgs sceneInfo)
    {
        if (!Runner.IsServer) return;
        
        _isSceneLoaded = true;
        
        // Находим точку спавна на загруженной сцене
        GameObject spawnerObj = GameObject.Find("PlayerSpawner");
        if (spawnerObj != null) _spawnPoint = spawnerObj.transform;

        Debug.Log($"[Спавнер] Сервер загрузил карту. Спавним игроков из стартовой очереди: {_pendingPlayers.Count}");

        // Спавним персонажей для всех, кто ждал загрузки в меню (включая Хоста!)
        foreach (PlayerRef player in _pendingPlayers)
        {
            SpawnPlayerObject(player);
        }
        
        _pendingPlayers.Clear();
    }

    private void SpawnPlayerObject(PlayerRef player)
    {
        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : Vector3.up * 5f;
        Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

        Debug.Log($"[СЕТЕВОЙ СПАВН] Создание коровы для Player ID: {player.PlayerId} на точке {spawnPos}");

        // Спавним корову и отдаем права ввода (InputAuthority) конкретно зашедшему плееру
        Runner.Spawn(playerPrefab, spawnPos, spawnRot, player);
    }
}