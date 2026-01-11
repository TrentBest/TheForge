using TheSingularityWorkshop.FSM_API;

using UnityEngine;

namespace TheSingularityWorkshop.FSM_API.SimpleRotationScalarTranslationDemo
{

    public class SimpleRotationScalarTranslationDemo : MonoBehaviour, IStateContext
    {
        public Transform XaxisCube;
        public Transform YaxisCube;
        public Transform ZaxisCube;



        // IStateContext Properties
        public bool IsValid { get; set; }
        public string Name { get; set; } = "CompoundDemoFSM";

        void Awake()
        {
            // 1. Setup the main FSM that controls the demo lifecycle
            if (!FSM_API.Interaction.Exists("SimpleRotationScalarTranslationDemoFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("SimpleRotationScalarTranslationDemoFSM", -1, "Update")
                    .State("Executing", OnEnterExecuting, null, null)
                    .BuildDefinition();
            }
            FSM_API.Create.CreateInstance("SimpleRotationScalarTranslationDemoFSM", this, "Update");
            IsValid = true;
        }

        private void OnEnterExecuting(IStateContext context)
        {
            if (context is SimpleRotationScalarTranslationDemo demo)
            {
                ScaleContext scX = new ScaleContext(demo.XaxisCube, new Vector3(1, 0, 0), 0.5f, 3f, 0.5f);
                ScaleContext scY = new ScaleContext(demo.YaxisCube, new Vector3(0, 1, 0), 0.5f, 3f, 0.5f);
                ScaleContext scZ = new ScaleContext(demo.ZaxisCube, new Vector3(1, 1, 0), 0.5f, 3f, 0.5f);
               
                TranslationContext tcX = new TranslationContext(demo.XaxisCube, new Vector3(1, 0, 0), 1f, 2f, -2f);
                TranslationContext tcY = new TranslationContext(demo.YaxisCube, new Vector3(0, 1, 0), 1f, 2f, -2f);
                TranslationContext tcZ = new TranslationContext(demo.ZaxisCube, new Vector3(0, 0, 1), 1f, 2f, -2f);

                RotationContext rcX = new RotationContext(demo.XaxisCube, new Vector3(1, 0, 0), 90f, 45f, -45f);
                RotationContext rcY = new RotationContext(demo.YaxisCube, new Vector3(0, 1, 0), 90f, 45f, -45f);
                RotationContext rcZ = new RotationContext(demo.ZaxisCube, new Vector3(0, 0, 1), 90f, 45f, -45f);

                

            }
        }
    }
}

