using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.State
{
    public class StateManager<T> where T : Enum
    {
        public StateManager(T defaultState, IEnumerable<StateContext<T>> stateInfos)
        {
            CurrentState = defaultState;

            _queue = new();
            _contexts = stateInfos.ToDictionary(item => item.TargetState, item => item);
        }

        private readonly ConcurrentQueue<T> _queue;

        private readonly Dictionary<T, StateContext<T>> _contexts;

        public T CurrentState { get; private set; }

        public T NextState
        {
            get
            {
                if (_queue.TryPeek(out var state))
                    return state;
                else
                    return CurrentState;
            }
        }

        public void AddNextState(T state)
        {
            _queue.Enqueue(state);
        }

        public void HandleAllState()
        {
            while (_queue.TryDequeue(out var state) && _contexts.TryGetValue(state, out var context))
            {
                if (context.TrySwitchToTargetState(CurrentState))
                    CurrentState = state;
            }
        }
    }
}
