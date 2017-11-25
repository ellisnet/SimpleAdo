using System;

namespace SimpleAdo
{
    /// <summary> Delegate for handling StateChange events. </summary>
    /// <param name="sender"> Source of the event. </param>
    /// <param name="e"> State change event information. </param>
    public delegate void StateChangeEventHandler(object sender, StateChangeEventArgs e);

    /// <summary>
    /// Additional information for state change events. This class cannot be inherited.
    /// </summary>
    public sealed class StateChangeEventArgs : EventArgs
    {
        /// <summary> The current state. </summary>
        private readonly ConnectionState _currentState;
        /// <summary> State of the original. </summary>
        private readonly ConnectionState _originalState;

        /// <summary> Constructor. </summary>
        /// <param name="originalState"> The original state. </param>
        /// <param name="currentState"> The current state. </param>
        public StateChangeEventArgs(ConnectionState originalState, ConnectionState currentState)
        {
            _originalState = originalState;
            _currentState = currentState;
        }

        /// <summary> Gets or sets the current state. </summary>
        /// <value> The current state. </value>
        public ConnectionState CurrentState => _currentState;

        /// <summary> Gets or sets the state of the original. </summary>
        /// <value> The original state. </value>
        public ConnectionState OriginalState => _originalState;
    }
}
