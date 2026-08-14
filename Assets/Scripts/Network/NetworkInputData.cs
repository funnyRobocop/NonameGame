using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    // Вектор движения WASD (или стика геймпада)
    public Vector2 MoveDirection;

    // Флаги кнопок. Сетевые переменные должны быть простыми типами (NetworkBool вместо bool)
    public NetworkBool JumpPressed;
    public NetworkBool SprintPressed;
}
