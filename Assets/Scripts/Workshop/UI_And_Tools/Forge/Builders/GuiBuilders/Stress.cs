using System;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.Tools
{
    /// <summary>
    /// The Unity-native port of the FSM Stress Test tool.
    /// Runs a massive number of simultaneous FSM agents to profile engine performance.
    /// </summary>
    public class Stress : IStateContext
    {
        public int WorkerCount { get; private set; }
        public int TickInterval { get; private set; }
        public string GroupName { get; private set; }

        public List<FSMHandle> Workers { get; private set; } = new List<FSMHandle>();

        // Payload Data for the UI to read
        public int DataValue { get; set; } = 0;
        public long LastFrameOps { get; set; }

        // IStateContext Requirements
        public bool IsValid { get; set; } = false;
        public string Name { get; set; }
        public FSMHandle Status { get; set; }

        public Stress(int workerCount, int tickInterval, string groupName = "Stress")
        {
            this.WorkerCount = workerCount;
            this.TickInterval = tickInterval;
            this.GroupName = groupName;
            this.Name = "StressManager";

            // 1. Define Manager (Runs every frame)
            if (!FSM_API.Interaction.Exists("StressManagerFSM", GroupName))
            {
                FSM_API.Create.CreateProcessingGroup(GroupName);
                FSM_API.Create.CreateFiniteStateMachine("StressManagerFSM", 1, GroupName)
                    .State("Operating", null, null, null)
                    .BuildDefinition();
            }

            // 2. Define Worker (Variable Interval)
            string workerFsmName = $"WorkerFSM_{TickInterval}";
            if (!FSM_API.Interaction.Exists(workerFsmName, GroupName))
            {
                FSM_API.Create.CreateFiniteStateMachine(workerFsmName, TickInterval, GroupName)
                    .State("Active", null, OnUpdateWorker, null)
                    .BuildDefinition();
            }

            // Create Manager Instance
            Status = FSM_API.Create.CreateInstance("StressManagerFSM", this, GroupName);
            IsValid = true;
        }

        public void SpawnWorkers()
        {
            string workerFsmName = $"WorkerFSM_{TickInterval}";
            Workers.Clear();

            for (int i = 0; i < WorkerCount; i++)
            {
                // Initialize a new context for each worker
                var workerCtx = new Stress(0, TickInterval, GroupName) { Name = $"Agent_FSM_{i:D5}" };

                // Spin it up in the FSM Engine
                workerCtx.Status = FSM_API.Create.CreateInstance(workerFsmName, workerCtx, GroupName);

                // Track it so the GUI can render its data
                Workers.Add(workerCtx.Status);
            }
        }

        // --- The Payload ---
        // This is exactly what was in your console app. 
        // By changing this math, you can profile different types of engine bottlenecks!
        private void OnUpdateWorker(IStateContext context)
        {
            if (context is Stress worker)
            {
                long ops = 0;

                // 1. Math Ops
                worker.DataValue++;
                worker.DataValue *= 2;
                ops += 2;

                // 2. The String Allocation Bottleneck
                // This simulates heavy garbage collection pressure
                string s = worker.DataValue.ToString();

                // 3. Simple Branching
                if (worker.DataValue % 2 == 0)
                {
                    Mathf.Abs(worker.DataValue);
                }
                ops += 1;

                worker.LastFrameOps = ops;
            }
        }
    }
}