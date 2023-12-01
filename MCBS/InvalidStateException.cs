using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException() : base(DefaultMessage) { }

        public InvalidStateException(string? message) : base(message) { }

        public InvalidStateException(object? currentState)
        {
            CurrentState = currentState;
        }

        public InvalidStateException(object? currentState, string? message) : base(message)
        {
            CurrentState = currentState;
        }

        public InvalidStateException(object currentState, object? targetState)
        {
            ArgumentNullException.ThrowIfNull(currentState, nameof(currentState));

            CurrentState = currentState;
            TargetState = targetState;
        }

        public InvalidStateException(object currentState, object? targetState, string? message) : base(message)
        {
            ArgumentNullException.ThrowIfNull(currentState, nameof(currentState));

            CurrentState = currentState;
            TargetState = targetState;
        }

        public object? CurrentState { get; }

        public object? TargetState { get; }

        protected const string DefaultMessage = "当前状态下无法完成此操作";

        public override string Message
        {
            get
            {
                string? s = base.Message;
                string message;
                if (CurrentState is not null && TargetState is not null)
                    message = $"无法从“{CurrentState}”状态切换到“{TargetState}”状态";
                else if (CurrentState is not null)
                    message = $"当前状态下无法切换到“{TargetState}”状态";
                else
                    return s ?? string.Empty;
                if (s is null)
                    return message;
                else
                    return s + Environment.NewLine + message;
            }
        }
    }
}
