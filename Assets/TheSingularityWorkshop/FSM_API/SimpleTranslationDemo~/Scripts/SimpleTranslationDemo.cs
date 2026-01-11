using UnityEngine;
using TheSingularityWorkshop.FSM_API;

namespace TheSingularityWorkshop.FSM_API.SimpleTranslationDemo
{
    public class SimpleTranslationDemo : MonoBehaviour, IStateContext
    {
        public Transform Xaxis;
        public Transform Yaxis;
        public Transform Zaxis;
        public FSMHandle XaxisHandle;
        public FSMHandle YaxisHandle;
        public FSMHandle ZaxisHandle;

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
            if (context is SimpleTranslationDemo std)
            {
                if (!FSM_API.Interaction.Exists("ScalarFSM"))
                {

                    float moveSpeed = 1.0f;
                    float maxDisplacement = 2.0f;
                    float minDisplacement = -2.0f;

                    // FIX: The TranslationFSM definition check is not needed here 
                    // since TranslationContext handles it. We can remove the if block.

                    // 1. X-axis: MoveAxis (1, 0, 0)
                    TranslationContext xAxisContext = new TranslationContext(std.Xaxis, new Vector3(1, 0, 0), moveSpeed, maxDisplacement, minDisplacement);
                    std.XaxisHandle = xAxisContext.Status; // FIX: Assign handle from context's Status property

                    // 2. Y-axis: MoveAxis (0, 1, 0)
                    TranslationContext yAxisContext = new TranslationContext(std.Yaxis, new Vector3(0, 1, 0), moveSpeed, maxDisplacement, minDisplacement);
                    std.YaxisHandle = yAxisContext.Status; // FIX: Assign handle from context's Status property

                    // 3. Z-axis: MoveAxis (0, 0, 1)
                    // FIX: Corrected MoveAxis to (0, 0, 1) and passed correct parameters
                    TranslationContext zAxisContext = new TranslationContext(std.Zaxis, new Vector3(0, 0, 1), moveSpeed, maxDisplacement, minDisplacement);
                    std.ZaxisHandle = zAxisContext.Status; // FIX: Assign handle from context's Status property
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