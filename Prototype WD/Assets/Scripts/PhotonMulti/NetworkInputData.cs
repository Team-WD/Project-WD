using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 direction;
    public Vector2 targetPosition;
    public NetworkBool isFiring;
    public bool IsFirePressed;
    public bool IsFirePressedPrevious;
    public Vector2 AimDirection;
}
