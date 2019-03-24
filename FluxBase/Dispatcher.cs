using System;

namespace FluxBase
{
    /// <summary>Represents a dispatcher, responsible for dispatching actions to subscribers (stores). Follows the publish-subscribe pattern.</summary>
    public class Dispatcher : BaseDispatcher, IDispatcher
    {
        /// <summary>Initializes a new instance of the <see cref="Dispatcher"/> class.</summary>
        public Dispatcher()
        {
        }

        /// <summary>Dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        public void Dispatch(object action)
        {
            EnterDispatch();
            try
            {
                DispatchAction(action);
            }
            finally
            {
                ExitDispatch();
            }
        }
    }
}