using System;
using System.Collections.Generic;
using System.Text;
using TheSingularityWorkshop.FSM_API;

namespace Assets.Scripts.MicroPackages
{
    public interface IProvider
    {
        int Id { get; }

        ProviderType ProviderType { get; }
    }

    public enum ProviderType
    {
        None = 0,
        StateOnEnter = 1,
        StateOnUpdate = 2,
        StateOnExit = 3,
        TransitionCondition = 4,
        Transition = 5,
        State = 6,
        FiniteStateMachine = 7,
        GuiElement = 8,
        GUI = 9,
    }

    public struct StateMethod
    {
        public string Name { get; }
        public string Description { get; }
        public Action<IStateContext> Method { get; }
    }



    public interface IStateOnEnterProvider : IProvider
    {
       List<StateMethod> Provided { get; }
    }

    public class StateOnEnterProvider : IStateOnEnterProvider
    {
        public List<StateMethod> Provided { get; } = new List<StateMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnEnter;

        public StateOnEnterProvider(int id, List<StateMethod> provided)
        {
            Id = id;
            Provided = provided;
        }

        public void Add(StateMethod method)
        {
            Provided.Add(method);
        }

        public void Remove(StateMethod method)
        {
            Provided.Remove(method);
        }
    }

    public interface IStateOnUpdateProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }

    public class StateOnUpdateProvider : IStateOnUpdateProvider
    {
        public List<StateMethod> Provided { get; } = new List<StateMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnUpdate;

        public StateOnUpdateProvider(int id, List<StateMethod> provided)
        {
            Id = id;
            Provided = provided;
        }

        public void Add(StateMethod method)
        {
            Provided.Add(method);
        }

        public void Remove(StateMethod method)
        {
            Provided.Remove(method);
        }
    }

    public interface IStateOnExitProvider : IProvider
    {
        List<StateMethod> Provided { get; }
    }

    public class StateOnExitProvider : IStateOnExitProvider
    {
        public List<StateMethod> Provided { get; } = new List<StateMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.StateOnExit;

        public StateOnExitProvider(int id, List<StateMethod> provided)
        {
            Id = id;
            Provided = provided;
        }

        public void Add(StateMethod method)
        {
            Provided.Add(method);
        }

        public void Remove(StateMethod method)
        {
            Provided.Remove(method);
        }
    }

    public struct TransitionMethod
    {
        public string Name { get; }
        public string Description { get; }
        public Func<bool, IStateContext> Method { get; }
    }

    public interface ITransitionConditionProvider : IProvider
    {
        List<TransitionMethod> Provided { get; }
    }

    public class TransitionConditionProvider : ITransitionConditionProvider
    {
        public List<TransitionMethod> Provided { get; } = new List<TransitionMethod>();

        public int Id { get; }

        public ProviderType ProviderType => ProviderType.TransitionCondition;

        public TransitionConditionProvider(int id, List<TransitionMethod> provided)
        {
            Id = id;
            Provided = provided;
        }

        public void Add(TransitionMethod method)
        {
            Provided.Add(method);
        }

        public void Remove(TransitionMethod method)
        {
            Provided.Remove(method);
        }
    }
    public struct Transition
    {
        public string From { get; }
        public string To { get; }
        public string Description { get; }

        public FSMTransition transition { get; }
    }

    public interface ITransitionProvider : IProvider
    {
        List<Transition> Provided { get; }
    }

    public struct State
    {
        public string Name { get; }
        public string Description { get; }
        public IStateOnEnterProvider OnEnter { get; }
        public IStateOnUpdateProvider OnOnUpdate { get; }
        public IStateOnExitProvider OnOnExit { get; }
    }

    public interface IStateProvider : IProvider
    {
        List<State> Provided { get; }
    }

    public struct FSM
    {
        public string Name { get; }
        public string Description { get; }
        public List<State> States { get; }
        public List<Transition> Transitions { get; }
    }

    public interface IFSMProvider : IProvider
    {
        List<FSM> Provided { get; }
    }
}
