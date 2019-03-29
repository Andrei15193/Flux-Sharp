#if !NET20 && !NET35
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    /// <summary>Represents an interface for dispatching asynchronous actions.</summary>
    public class AsyncDispatcher : BaseDispatcher, IAsyncDispatcher
    {
        private readonly LinkedList<IAsyncMiddleware> _middlewarePipeline = new LinkedList<IAsyncMiddleware>();

        /// <summary>Initializes a new instance of the <see cref="AsyncDispatcher"/> class.</summary>
        public AsyncDispatcher()
        {
        }

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <param name="middleware">The <see cref="IMiddleware"/> to configure.</param>
        /// <returns>Returns the ID of the configured middleware.</returns>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public object Use(IAsyncMiddleware middleware)
            => _middlewarePipeline.AddLast(middleware);

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <param name="middleware">The <see cref="IMiddleware"/> to configure.</param>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public void Use<TAction>(IAsyncMiddleware<TAction> middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            Use(new AsyncMiddlewareAdapter<TAction>(middleware));
        }

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
                if (_middlewarePipeline.First == null)
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
                else
                {
                    var firstMiddlewareNode = _middlewarePipeline.First;
                    return firstMiddlewareNode.Value.HandleAsync(new AsyncMiddlewareContext(firstMiddlewareNode, action, this), cancellationToken);
                }
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

        /// <summary>Asynchronously dispatches an action to the next middleware in the pipeline.</summary>
        /// <param name="id">The ID of the current middleware in the pipeline.</param>
        /// <param name="action">The action to dispatch.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to signal cancellation.</param>
        /// <returns>Returns a <see cref="Task"/> representing the asynchronous operation.</returns>
        protected Task DispatchNextAsync(object id, object action, CancellationToken cancellationToken)
        {
            if (id is LinkedListNode<IAsyncMiddleware> middlewareNode && middlewareNode.List == _middlewarePipeline)
                if (middlewareNode.Next == null)
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
                else
                    return middlewareNode.Next.Value.HandleAsync(new AsyncMiddlewareContext(middlewareNode.Next, action, this), cancellationToken);
            else
#if NET40 || NET45 || NET451 || NET452 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
                return Helper.TaskFromException(new ArgumentException("The provided id does not correspond to a configured middleware.", nameof(id)));
#else
                return Task.FromException(new ArgumentException("The provided id does not correspond to a configured middleware.", nameof(id)));
#endif
        }

        private sealed class AsyncMiddlewareContext : IAsyncMiddlewareContext
        {
            private readonly object _id;
            private readonly AsyncDispatcher _dispatcher;

            public AsyncMiddlewareContext(object id, object action, AsyncDispatcher dispatcher)
            {
                _id = id;
                Action = action;
                _dispatcher = dispatcher;
            }

            public object Action { get; }

            public void Dispatch(object action)
                => _dispatcher.DispatchAction(action);

            public Task NextAsync(object action, CancellationToken cancellationToken)
                => _dispatcher.DispatchNextAsync(_id, action, cancellationToken);

            public Task NextAsync(CancellationToken cancellationToken)
                => _dispatcher.DispatchNextAsync(_id, Action, cancellationToken);
        }
    }
}
#endif