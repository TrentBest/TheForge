using TheSingularityWorkshop.FSM_API;

using UnityEngine;


namespace TheSingularityWorkshop.FSM_API.SimpleScalarDemo
{
    public class ScaleContext : IStateContext
    {
        public float ScaleSpeed = 0.5f;
        public float MaxScale = 3.0f;
        public float MinScale = 0.5f;

        public Vector3 ScaleAxis;

        public Vector3 OriginalScale = Vector3.one;
        public Transform TransformHandle;

        public bool IsValid { get; set; }
        public string Name { get; set; }

        public ScaleContext(Transform transform, Vector3 scaleAxis, float scaleSpeed = 1f, float maxScale = 100f, float minScale = .001f)
        {
            TransformHandle = transform;
            OriginalScale = transform.localScale;
            ScaleSpeed = scaleSpeed;
            MaxScale = maxScale;
            MinScale = minScale;
            ScaleAxis = scaleAxis;

            if (!FSM_API.Interaction.Exists("ScalarFSM"))
            {
                FSM_API.Create.CreateFiniteStateMachine("ScalarFSM", -1, "Update")
                    .State("ScalingUp", OnEnterScalingUp, OnUpdateScalingUp, OnExitScalingUp)
                    .State("ScalingDown", OnEnterScalingDown, OnUpdateScalingDown, OnExitScalingDown)
                    .Transition("ScalingUp", "ScalingDown", Apex)
                    .Transition("ScalingDown", "ScalingUp", Root)
                    .BuildDefinition();
            }
            Name = "ScalarFSM";
            IsValid = true;
        }

        private void OnEnterScalingUp(IStateContext context)
        {
            if (context is ScaleContext sc)
            {

            }
        }

        private void OnUpdateScalingUp(IStateContext context)
        {
            if (context is ScaleContext sc)
            {
                sc.TransformHandle.localScale += sc.ScaleAxis * (sc.ScaleSpeed * Time.deltaTime);
            }
        }

        private void OnExitScalingUp(IStateContext context)
        {
            if (context is ScaleContext sc)
            {

            }
        }

        private void OnEnterScalingDown(IStateContext context)
        {
            if (context is ScaleContext sc)
            {

            }
        }

        private void OnUpdateScalingDown(IStateContext context)
        {
            if (context is ScaleContext sc)
            {
                sc.TransformHandle.localScale -= sc.ScaleAxis * (sc.ScaleSpeed * Time.deltaTime);
            }
        }

        private void OnExitScalingDown(IStateContext context)
        {
            if (context is ScaleContext sc)
            {

            }
        }

        private bool Apex(IStateContext context)
        {
            if (context is ScaleContext sc)
            {
                bool atMax = true;
                // The FSM transitions only when ALL scaled axes have reached MaxScale
                if (sc.ScaleAxis.x != 0 && sc.TransformHandle.localScale.x < sc.MaxScale) atMax = false;
                if (sc.ScaleAxis.y != 0 && sc.TransformHandle.localScale.y < sc.MaxScale) atMax = false;
                if (sc.ScaleAxis.z != 0 && sc.TransformHandle.localScale.z < sc.MaxScale) atMax = false;

                return atMax;
            }
            return false;
        }

        private bool Root(IStateContext context)
        {
            if (context is ScaleContext sc)
            {
                bool atMin = true;
                // The FSM transitions only when ALL scaled axes have reached MinScale
                if (sc.ScaleAxis.x != 0 && sc.TransformHandle.localScale.x > sc.MinScale) atMin = false;
                if (sc.ScaleAxis.y != 0 && sc.TransformHandle.localScale.y > sc.MinScale) atMin = false;
                if (sc.ScaleAxis.z != 0 && sc.TransformHandle.localScale.z > sc.MinScale) atMin = false;

                return atMin;
            }
            return false;
        }
    }
}