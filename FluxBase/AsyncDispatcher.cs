#if !NET20 && !NET35
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    /// <summary>Represents an interface for dispatching asynchronous actions.</summary>
    public class AsyncDispatcher : BaseDispatcher, IAsyncDispatcher
    {
        /// <summary>Asynchronously dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task DispatchAsync(object action)
            => DispatchAsync(action, CancellationToken.None);

        /// <summary>Asynchronously dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task DispatchAsync(object action, CancellationToken cancellationToken)
        {
            EnterDispatch();
            try
            {
                DispatchAction(action);
#if NET40
                return Helper.CompletedTask;
#elif NET45 || NET451 || NET452 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
                return Task.FromResult<object>(null);
#else
                return Task.CompletedTask;
#endif
            }
            catch (Exception exception)
            {
#if NET40 || NET45 || NET451 || NET452 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
                return Helper.TaskFromException(exception);
#else
                return Task.FromException(exception);
#endif
            }
            finally
            {
                ExitDispatch();
            }
        }
    }
}
#endif