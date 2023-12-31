﻿using System;
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

        private readonly Queue<T> _queue;

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
            lock (_queue)
            {
                _queue.Enqueue(state);
            }
        }

        public void HandleAllState()
        {
            lock (_queue)
            {
                while (_queue.TryPeek(out var nextState))
                {
                    if (_contexts.TryGetValue(nextState, out var nextContext) && nextContext.TrySwitchToTargetState(CurrentState))
                        CurrentState = nextState;
                    _queue.Dequeue();
                }

                if (_contexts.TryGetValue(CurrentState, out var currentContext))
                    currentContext.OnState.Invoke();
            }
        }
    }
}
