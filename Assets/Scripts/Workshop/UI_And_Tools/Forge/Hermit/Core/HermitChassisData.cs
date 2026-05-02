using System;
using Unity.Collections;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    public enum HermitAIState { Idle, Executing, Sensing }

    [Serializable]
    public unsafe struct HermitChassisData
    {
        public FixedString64Bytes AgentId;
        public HermitAIState CurrentState;

        // --- Physical State ---
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 TargetPosition;

        // --- Slave Renderer Instance IDs ---
        public int RootObjectId;
        public int CoreLightId;
        public int ShellRendererId;

        // --- Temporal History (Unmanaged Buffer) ---
        // We store the last 50 transform snapshots for the "Back up" logic
        public int HistoryCount;
        public int CurrentHistoryIndex;
        public fixed float PositionHistoryX[50];
        public fixed float PositionHistoryY[50];
        public fixed float PositionHistoryZ[50];

        public static HermitChassisData Create(string id, Vector3 pos)
        {
            return new HermitChassisData
            {
                AgentId = new FixedString64Bytes(id),
                CurrentState = HermitAIState.Idle,
                Position = pos,
                Rotation = Quaternion.identity,
                TargetPosition = pos,
                HistoryCount = 0,
                CurrentHistoryIndex = -1
            };
        }

        public void RecordSnapshot()
        {
            int nextIndex = (CurrentHistoryIndex + 1) % 50;
            PositionHistoryX[nextIndex] = Position.x;
            PositionHistoryY[nextIndex] = Position.y;
            PositionHistoryZ[nextIndex] = Position.z;

            CurrentHistoryIndex = nextIndex;
            if (HistoryCount < 50) HistoryCount++;
        }
    }
}