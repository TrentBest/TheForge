// The Unmanaged Memory Block for Input
using UnityEngine;

public struct InputStateEntity
{
    public int ActionHash; // e.g., Hash of "Move", "Jump", "Interact"
    public Vector2 Axis;
    public bool IsPressed;
    public bool WasPressedThisFrame;
}