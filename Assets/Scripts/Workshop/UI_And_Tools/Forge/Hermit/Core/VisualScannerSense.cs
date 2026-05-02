// File: Assets/Scripts/Workshop/Forge/Hermit/Core/VisualScannerSense.cs
using System.Collections.Generic;
using UnityEngine;
using Workshop.UI_And_Tools.Forge.Core;
using Workshop.UI_And_Tools.Physics;

namespace Workshop.UI_And_Tools.Forge.Hermit.Core
{
    public class VisualScannerSense : ISense
    {
        public Sense Type => Sense.Vision;

        private float _scanRadius;
        private LayerMask _scanMask;

        public VisualScannerSense(float scanRadius = 20f, LayerMask scanMask = default)
        {
            _scanRadius = scanRadius;
           // _scanMask = scanMask == default ? Physics.AllLayers : scanMask;
        }

        public List<PerceptionData> Observe(HermitContext context)
        {
            List<PerceptionData> perceivedObjects = new List<PerceptionData>();

            if (context == null || !context.IsValid || context.Transform == null)
                return perceivedObjects;

           // Collider[] hits = Physics.OverlapSphere(context.Transform.position, _scanRadius, _scanMask);

            //foreach (var hit in hits)
            //{
            //    // Look for objects tagged with our Affordance interface
            //    IAffordance affordance = hit.GetComponentInParent<IAffordance>();
            //    if (affordance != null)
            //    {
            //        float dist = Vector3.Distance(context.Transform.position, hit.transform.position);

            //        perceivedObjects.Add(new PerceptionData
            //        {
            //            Name = hit.gameObject.name,
            //            Description = affordance.Description,
            //            Distance = dist,
            //            NeglectScore = affordance.GetNeglectScore(),
            //            Position = hit.transform.position,     // Added position tracking
            //            TargetObject = hit.gameObject          // Added memory of the object
            //        });
            //    }
            //}

            return perceivedObjects;
        }
    }
}