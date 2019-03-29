namespace FluxBase
{
    /// <summary>Represents a middleware pipeline for handling actions before and after they are actually dispatched to action handlers.</summary>
    /// <remarks>
    /// <para>
    /// A middleware pipeline can read the action that is currently being dispatched and decide to modify it, skip all the following middleware
    /// handlers or dispatch an action bypassing all following middleware handlers.
    /// </para>
    /// <para>
    /// Middleware is similar to filters as they can be used in the same way for error reporting or logging, but can be used to split an action
    /// dispatch into multiple actual action dispatches.
    /// </para>
    /// </remarks>
    public interface IMiddleware
    {
        /// <summary>Handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        void Handle(IMiddlewareContext context);
    }

    /// <summary>Represents a middleware pipeline for handling actions before and after they are actually dispatched to action handlers.</summary>
    /// <remarks>
    /// <para>
    /// A middleware pipeline can read the action that is currently being dispatched and decide to modify it, skip all the following middleware
    /// handlers or dispatch an action bypassing all following middleware handlers.
    /// </para>
    /// <para>
    /// Middleware is similar to filters as they can be used in the same way for error reporting or logging, but can be used to split an action
    /// dispatch into multiple actual action dispatches.
    /// </para>
    /// </remarks>
    public interface IMiddleware<TAction>
    {
        /// <summary>Handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        void Handle(IMiddlewareContext<TAction> context);
    }
}