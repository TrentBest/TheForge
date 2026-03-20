using NUnit.Framework;
using System.Collections.Generic;
using TheSingularityWorkshop.FSM_API;

// Inheriting from IStateContext allows the Game itself to be the context for its own FSMs
public interface IInExperienceGame : IStateContext
{
    /// <summary>
    /// A map of logical sequence names to the actual FSM processing group names.
    /// The containing Experience uses these to call FSM_API.FSM_API.Interaction.Update(groupName).
    /// </summary>
    Dictionary<string, string> ProcessingGroups { get; }

    /// <summary>
    /// Declares what the game needs from the host to function.
    /// </summary>
    List<InputRequirement> GetRequiredInputs();

    /// <summary>
    /// The "Magic" method. The host calls this to 'hand over' the environment.
    /// The game uses the 'context' to scale its GUI and logic.
    /// </summary>
    FSMHandle InjectionAs(ExperienceInjectionContext context, IInputBridge inputBridge);


}