using UnityEngine;

public struct SoldierGPUData
{
    public Vector3 Position;
    public float Facing; // Rotation on Y axis
    public int StateId;  // Idle, Marching, Combat, Dead
    public float Health;
    // Padding might be required depending on HLSL packing rules, 
    // but Vector3 + float + int + float aligns nicely to 24 bytes.
}