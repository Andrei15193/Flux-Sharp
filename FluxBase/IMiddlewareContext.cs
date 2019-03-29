namespace FluxBase
{
    /// <summary>Represents the middleware context when handling an action dispatch.</summary>
    public interface IMiddlewareContext
    {
        /// <summary>Gets the action that is being dispatched.</summary>
        object Action { get; }

        /// <summary>Calls the next middleware in the chain with the given <paramref name="action"/>.</summary>
        /// <param name="action">The action to continue with.</param>
        /// <remarks>In case there is no next middleware handler configured then the <paramref name="action"/> will be dispatched to all registered action handlers (stores).</remarks>
        void Next(object action);

        /// <summary>Calls the next middleware in the chain with the same <see cref="Action"/>.</summary>
        /// <remarks>In case there is no next middleware handler configured then the <see cref="Action"/> will be dispatched to all registered action handlers (stores).</remarks>
        void Next();

        /// <summary>Dispatches the given <paramref name="action"/> to all registered action handlers (stores).</summary>
        /// <param name="action">The action to dispatch.</param>
        void Dispatch(object action);
    }

    /// <summary>Represents a typed middleware context when handling an action dispatch.</summary>
    /// <typeparam name="TAction">The type of the action.</typeparam>
    public interface IMiddlewareContext<TAction> : IMiddlewareContext
    {
        /// <summary>Gets the action that is being dispatched.</summary>
        new TAction Action { get; }
    }
}