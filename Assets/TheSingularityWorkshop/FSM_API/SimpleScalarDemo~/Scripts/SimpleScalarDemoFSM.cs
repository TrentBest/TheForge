using System;

using TheSingularityWorkshop.FSM_API;

using UnityEngine;


namespace TheSingularityWorkshop.FSM_API.SimpleScalarDemo
{
    public class SimpleScalarDemoFSM : MonoBehaviour, IStateContext
    {
        public Transform Xaxis;
        public Transform Yaxis;
        public Transform XYaxis;
        public FSMHandle XaxisHandle;
        public FSMHandle YaxisHandle;
        public FSMHandle XYaxisHandle;

        void Awake()
        {
            if (!FSM_API.Interaction.Exists("SimpleScalarDemoFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("SimpleScalarDemoFSM", -1, "Update")
                    .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                    .BuildDefinition();
            }
            FSM_API.Create.CreateInstance("SimpleScalarDemoFSM", this, "Update");
            Name = "SimpleScalarDemoFSM";
            IsValid = true;
        }

        private void OnEnterExecuting(IStateContext context)
        {
            if (context is SimpleScalarDemoFSM ssd)
            {
                if (!FSM_API.Interaction.Exists("ScalarFSM"))
                {

                    //Now define our instances from the provided transforms
                    ScaleContext xAxisContext = new ScaleContext(ssd.Xaxis, new Vector3(1, 0, 0), 0.5f, 3f, 0.5f);
                    ssd.XaxisHandle = FSM_API.Create.CreateInstance("ScalarFSM", xAxisContext, "Update");
                    ScaleContext yAxisContext = new ScaleContext(ssd.Yaxis, new Vector3(0, 1, 0), 0.5f, 3f, 0.5f);
                    ssd.YaxisHandle = FSM_API.Create.CreateInstance("ScalarFSM", yAxisContext, "Update");
                    ScaleContext xyAxisContext = new ScaleContext(ssd.XYaxis, new Vector3(1, 1, 0), 0.5f, 3f, 0.5f);
                    ssd.XYaxisHandle = FSM_API.Create.CreateInstance("ScalarFSM", xyAxisContext, "Update");
                }
            }
        }

        private void OnUpdateExecuting(IStateContext context)
        {

        }

        private void OnExitExecuting(IStateContext context)
        {

        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool IsValid { get; set; }
        public string Name { get; set; }
    }
}