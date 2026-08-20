using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 MoveDirection;
    public NetworkBool JumpPressed;
    public float CameraRotationY;

    public NetworkBool DashPressed;
}
