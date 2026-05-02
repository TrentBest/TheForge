using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.Workshop.UI_And_Tools.Forge.Builders.GuiBuilders
{
    public class ForgeFSMBuilder<TEnum> where TEnum : struct, Enum
    {
        //private readonly FSM_Builder _internalBuilder;

        public ForgeFSMBuilder(string fsmName)
        {
           // _internalBuilder = new FSM_Builder(fsmName);

            // 1. Bootstrapping: Seed the graph with the Enum's taxonomy
            //foreach (var stateName in Enum.GetNames(typeof(TEnum)))
            //{
            //    _internalBuilder.AddState(stateName);
            //}
        }

        // 2. Strongly-typed configuration
        public ForgeFSMBuilder<TEnum> WithTransition(TEnum from, TEnum to, Func<bool> condition)
        {
           // _internalBuilder.AddTransition(from.ToString(), to.ToString(), condition);
            return this;
        }

        // 3. Dynamic Expansion: The API allows injecting states that DON'T exist in TEnum!
        public ForgeFSMBuilder<TEnum> InjectDynamicState(string injectedStateName)
        {
           // _internalBuilder.AddState(injectedStateName);
            return this;
        }

       // public FSM Build() => _internalBuilder.Build();
    }
}
