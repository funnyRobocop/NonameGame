using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    private NetworkInputData _localInputData;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Runner.AddCallbacks(this);
            
            Debug.Log($"[ЯДРО СЕТИ] Ввод принудительно зарегистрирован в колбэках Раннера!");
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasKey)
    {
        if (HasInputAuthority)
        {
            runner.RemoveCallbacks(this);
        }
    }

    void IBeforeUpdate.BeforeUpdate()
    {
        if (!HasInputAuthority) return;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Vector2 moveVector = Vector2.zero;

            if (keyboard[Key.W].isPressed) moveVector.y += 1f;
            if (keyboard[Key.S].isPressed) moveVector.y -= 1f;
            if (keyboard[Key.A].isPressed) moveVector.x -= 1f;
            if (keyboard[Key.D].isPressed) moveVector.x += 1f;

            _localInputData.MoveDirection = moveVector.normalized;
            _localInputData.JumpPressed = keyboard[Key.Space].isPressed;
            //_localInputData.SprintPressed = keyboard[Key.LeftShift].isPressed;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!HasInputAuthority) return;

        Debug.Log("[ПОТОК ВВОДА] Данные успешно переданы в сетевой тик!");

        input.Set(_localInputData);
        _localInputData.MoveDirection = Vector2.zero;
        _localInputData.JumpPressed = false;
        //_localInputData.SprintPressed = false;
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
        {
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
    }
}