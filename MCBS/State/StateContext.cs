using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.State
{
    public class StateContext<T> where T : Enum
    {
        public StateContext(T targetState, IList<T> whitelist, StateHandler<T> stateHandler, Action onState)
        {
            ArgumentNullException.ThrowIfNull(whitelist, nameof(whitelist));
            ArgumentNullException.ThrowIfNull(stateHandler, nameof(stateHandler));

            TargetState = targetState;
            Whitelist = new(whitelist);
            StateHandler = stateHandler;
            OnState = onState;
        }

        public StateContext(T targetState, IList<T> whitelist, StateHandler<T> stateHandler) : this(targetState, whitelist, stateHandler, () => { }) { }

        public T TargetState { get; }

        public ReadOnlyCollection<T> Whitelist { get; }

        public StateHandler<T> StateHandler { get; }

        public Action OnState { get; }

        public bool IsAllowSwitchToTargetState(T currentState)
        {
            return !Equals(currentState, TargetState) && Whitelist.Contains(currentState);
        }

        public void SwitchToTargetState(T currentState)
        {
            if (Equals(currentState, TargetState))
                throw new InvalidStateException(currentState, TargetState, "状态没有发生变化");

            if (!Whitelist.Contains(currentState))
                throw new InvalidStateException(currentState, TargetState);

            if (!StateHandler.Invoke(currentState, TargetState))
                throw new InvalidStateException(currentState, TargetState);
        }

        public bool TrySwitchToTargetState(T currentState)
        {
            if (Equals(currentState, TargetState))
                return false;

            if (!Whitelist.Contains(currentState))
                return false;

            return StateHandler.Invoke(currentState, TargetState);
        }
    }
}
