#if !NET20 && !NET35
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    /// <summary>
    /// Represents an asynchronous middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.
    /// </summary>
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
    public interface IAsyncMiddleware
    {
        /// <summary>Asynchronously handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(IAsyncMiddlewareContext context, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents an asynchronous middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.
    /// </summary>
    /// <typeparam name="TAction">The type of action.</typeparam>
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
    public interface IAsyncMiddleware<TAction>
    {
        /// <summary>Asynchronously handles a currently executing dispatch.</summary>
        /// <param name="context">The context of the current dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(IAsyncMiddlewareContext<TAction> context, CancellationToken cancellationToken);
    }
}
#endif