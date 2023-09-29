using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class StateMachine<T> where T : Enum
    {
        public StateMachine(T defaultState)
        {
            CurrentState = defaultState;

            _stateQueue = new();
            _stateHandlers = new();
        }

        private readonly ConcurrentQueue<T> _stateQueue;

        private readonly Dictionary<T, StateHandler<T>> _stateHandlers;

        public T CurrentState { get; private set; }

        public T NextState
        {
            get
            {
                if (_stateQueue.TryPeek(out var state))
                    return state;
                else
                    return CurrentState;
            }
        }

        public void SetStateHandler(T state, StateHandler<T> handler)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            _stateHandlers[state] = handler;
        }

        public void AddNextState(T state)
        {
            _stateQueue.Enqueue(state);
        }

        public void HandleAllState()
        {
            while (_stateQueue.TryDequeue(out var state))
            {
                if (Equals(state, CurrentState))
                    continue;

                if (!_stateHandlers.TryGetValue(state, out var handler))
                    continue;

                if (handler.Invoke(CurrentState, state))
                    CurrentState = state;
            }
        }
    }
}
