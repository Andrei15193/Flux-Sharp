#if !NET20 && !NET30 && !NET35
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    /// <summary>Represents the middleware context when handling an asynchronous action dispatch.</summary>
    public interface IMiddlewareAsyncContext
    {
        /// <summary>Gets the action that is being dispatched.</summary>
        object Action { get; }

        /// <summary>Calls the next middleware in the chain with the given <paramref name="action"/>.</summary>
        /// <param name="action">The action to continue with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>In case there is no next middleware handler configured then the <paramref name="action"/> will be dispatched to all registered action handlers (stores).</remarks>
        Task NextAsync(object action, CancellationToken cancellationToken);

        /// <summary>Calls the next middleware in the chain with the same <see cref="Action"/>.</summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>In case there is no next middleware handler configured then the <see cref="Action"/> will be dispatched to all registered action handlers (stores).</remarks>
        Task NextAsync(CancellationToken cancellationToken);
    }

    /// <summary>Represents a typed middleware context when handling an asynchronous action dispatch.</summary>
    /// <typeparam name="TAction">The type of the action.</typeparam>
    public interface IMiddlewareAsyncContext<TAction> : IMiddlewareAsyncContext
    {
        /// <summary>Gets the action that is being dispatched.</summary>
        new TAction Action { get; }
    }
}
#endif