using System;

namespace FluxBase
{
    /// <summary>Represents an interface for dispatching actions.</summary>
    public interface IDispatcher
    {
        /// <summary>Dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        void Dispatch(object action);
    }
}