using TheSingularityWorkshop.FsmApi.Behaviors.OnUpdate;
using TheSingularityWorkshop.FsmApi.Factories;
using TheSingularityWorkshop.MicroPackages.Providers;
using TheSingularityWorkshop.MicroPackages;
using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;


namespace TheSingularityWorkshop.MicroPackages.Packages.DigitalLogic
{
   public class DigitalLogic : IMicroPackage
    {

        string physicalPackageId = "DigitalLogic.Physical";
        string combinationalLogic = "DigitalLogic.Combinational";
        string sequentialLogic = "DigitalLogic.Sequential";
        string modularFunctionalUnits = "DigitalLogic.ModularFunctionalUnits";
        string controlAndTiming = "DigitalLogic.ControlandTiming";
        string programmableLogic = "DigitalLogic.Programmable";
        string interfaceVisualization = "DigitalLogic.Visualization";

        public DigitalLogic()
        {


        }

        public Dictionary<string, List<string>> ProcessGroupsPerUnityMessage { get; } = new Dictionary<string, List<string>>();

        public void Arbitrate(IPackageArbitrator arbitrator)
        {
            //No Arbitration
            arbitrator.None();
        }

        public void LoadPackage(IPackageArbitrator arbitrator)
        {
            //Physical Layer
            arbitrator.LoadPackage(physicalPackageId);
            //Combinational Logic
            arbitrator.LoadPackage(combinationalLogic);
            //Sequential Logic
            arbitrator.LoadPackage(sequentialLogic);
            //Modular Functional Units
            arbitrator.LoadPackage(modularFunctionalUnits);
            //Control and Timing
            arbitrator.LoadPackage(controlAndTiming);
            //Programmable Logic & Architecture
            arbitrator.LoadPackage(programmableLogic);
            //Interface & Visualization
            arbitrator.LoadPackage(interfaceVisualization);
        }
    }
}
