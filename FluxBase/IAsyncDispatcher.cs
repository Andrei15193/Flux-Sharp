#if !NET20 && !NET35
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    /// <summary>Represents an interface for dispatching asynchronous actions.</summary>
    public interface IAsyncDispatcher
    {
        /// <summary>Asynchronously dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DispatchAsync(object action);

        /// <summary>Asynchronously dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DispatchAsync(object action, CancellationToken cancellationToken);
    }
}
#endif