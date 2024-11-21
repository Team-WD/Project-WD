using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct ProjectileData : INetworkStruct
{
    public Vector2 Position;
    public Vector2 Velocity;
    public TickTimer LifeTime;
    public NetworkId OwnerId;
    public float BaseDamage;
    public bool IsCritical;
    public int FinalDamage;
    public float DistanceTraveled;
    public float PenetratingCount;
}
