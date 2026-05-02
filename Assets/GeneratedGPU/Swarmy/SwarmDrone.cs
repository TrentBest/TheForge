using UnityEngine; using System.Runtime.InteropServices;
public enum SwarmDroneState { Patrol_Eyes, Decoy_Fake, Weapon_Strike, RTB_Recharge }
[StructLayout(LayoutKind.Sequential)]
public struct SwarmDrone {
    public Vector3 Position;
    public Vector3 Velocity;
    public int StateId;
    public float Battery;
}
