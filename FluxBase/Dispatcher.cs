using System;
using System.Collections.Generic;
using System.Threading;
#if !NET20 && !NET30 && !NET35
using System.Threading.Tasks;
#endif

namespace FluxBase
{
    /// <summary>Represents a dispatcher, responsible for dispatching actions to subscribers (stores). Follows the publish-subscribe pattern.</summary>
    public class Dispatcher : IDispatcher, IDispatchWaitHandle
    {
        private const int _availableState = 0;
        private const int _dispatchingState = 1;

        private int _state = _availableState;
        private readonly ICollection<Action<object>> _subscribers = new List<Action<object>>();
        private readonly LinkedList<Action<object>> _remainingSubscribers = new LinkedList<Action<object>>();
        private object _currentAction = null;
        private LinkedListNode<Action<object>> _currentSubscriber = null;
        private readonly LinkedList<IMiddleware> _middlewarePipeline = new LinkedList<IMiddleware>();

        /// <summary>Initializes a new instance of the <see cref="Dispatcher"/> class.</summary>
        public Dispatcher()
        {
        }

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <param name="middleware">The <see cref="IMiddleware"/> to configure.</param>
        /// <returns>Returns the ID of the configured <paramref name="middleware"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is <c>null</c>.</exception>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public object Use(IMiddleware middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            return _middlewarePipeline.AddLast(middleware);
        }

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <typeparam name="TAction">The type of actions for which the middleware applies.</typeparam>
        /// <param name="middleware">The <see cref="IMiddleware{TAction}"/> to configure.</param>
        /// <returns>Returns the ID of the configured <paramref name="middleware"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is <c>null</c>.</exception>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public object Use<TAction>(IMiddleware<TAction> middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            return Use(new MiddlewareAdapter<TAction>(middleware));
        }

        /// <summary>Indicates whether the dispatcher is currently dispatching an action.</summary>
        public bool IsDispatching
            => _state == _dispatchingState;

        /// <summary>Registers the provided <paramref name="callback"/> for notifications. A callback may only be registered once.</summary>
        /// <param name="callback">The callback that will handle dispatched actions.</param>
        /// <returns>
        /// Returns an object as an ID that can be used to wait for the provided <paramref name="callback"/> to complete during dispatches
        /// or unregister the provided <paramref name="callback"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <c>null</c>.</exception>
        public object Register(Action<object> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!_subscribers.Contains(callback))
                _subscribers.Add(callback);
            return callback;
        }

        /// <summary>Unregisters the callback with the provided <paramref name="id"/> from notifications.</summary>
        /// <param name="id">The ID object previously returned from calling the <see cref="Register(Action{object})"/> method.</param>
        /// <returns>Returns <c>true</c> if the handler was unregistered; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
        public bool Unregister(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            return id is Action<object> callback && _subscribers.Remove(callback);
        }

        /// <summary>Registers the provided <paramref name="store"/> for notifications. A <see cref="Store"/> may only be registered once.</summary>
        /// <param name="store">The <see cref="Store"/> to register.</param>
        /// <returns>
        /// Returns an object as an ID that can be used to wait for the provided <paramref name="store"/> to complete during dispatches
        /// or unregister the provided <paramref name="store"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        public object Register(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            return Register(store.Handle);
        }

        /// <summary>Unregisters the provided <paramref name="store"/> from notifications.</summary>
        /// <param name="store">The previously subscribed <see cref="Store"/> to action dispatches using the <see cref="Register(Store)"/> method.</param>
        /// <returns>Returns <c>true</c> if the <paramref name="store"/> was unregistered; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        public bool Unregister(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            return _subscribers.Remove(store.Handle);
        }

        /// <summary>Dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        public void Dispatch(object action)
        {
            _EnterDispatch();
            try
            {
                if (_middlewarePipeline.First == null)
                    _DispatchAction(action);
                else
                {
                    var firstMiddlewareNode = _middlewarePipeline.First;
                    firstMiddlewareNode.Value.Handle(new MiddlewareContext(firstMiddlewareNode, action, this));
                }
            }
            finally
            {
                _ExitDispatch();
            }
        }

#if !NET20 && !NET30 && !NET35
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
            _EnterDispatch();
            try
            {
                if (_middlewarePipeline.First == null)
                    try
                    {
                        _DispatchAction(action);
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
                _ExitDispatch();
            }
        }
#endif

        /// <summary>Waits for the registered action handler with the provided <paramref name="id"/> to complete.</summary>
        /// <param name="id">The ID object identifying the action handler to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks if the referred callback is registered and has not yet been executed.</remarks>
        public void WaitFor(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (_state == _availableState)
                throw new InvalidOperationException("Cannot wait for action handler when there is no active dispatch.");

            if (id is Action<object> callback && _currentSubscriber.Value != callback && _remainingSubscribers.Contains(callback))
            {
                _CheckDeadlockWait(callback);
                _remainingSubscribers.Remove(callback);
                _currentSubscriber = _remainingSubscribers.AddAfter(_currentSubscriber, callback);

                _currentSubscriber.Value(_currentAction);

                var previousSubscriber = _currentSubscriber.Previous;
                _remainingSubscribers.Remove(_currentSubscriber);
                _currentSubscriber = previousSubscriber;
            }
        }

        /// <summary>Waits for the registered action handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of IDs identifying the action handlers to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred callbacks that are registered and have not yet been executed.</remarks>
        public void WaitFor(IEnumerable<object> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            foreach (var id in ids)
            {
                if (id == null)
                    throw new ArgumentException("Cannot contain 'null' ids.", nameof(ids));
                WaitFor(id);
            }
        }

        /// <summary>Waits for the registered action handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of IDs identifying the action handlers to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred callbacks that are registered and have not yet been executed.</remarks>
        public void WaitFor(params object[] ids)
            => WaitFor((IEnumerable<object>)ids);

        /// <summary>Waits for the provided <paramref name="store"/> to complete.</summary>
        /// <param name="store">A previously subscribed <see cref="Store"/> to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="store"/> is registered and has not yet handled the current action dispatch.</remarks>
        public void WaitFor(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            if (_state == _availableState)
                throw new InvalidOperationException("Cannot wait for store when there is no active dispatch.");

            WaitFor(new Action<object>(store.Handle));
        }

        /// <summary>Waits for the provided <paramref name="stores"/> to complete.</summary>
        /// <param name="stores">A collection of previously subscribed <see cref="Store"/>s to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet handled the current action dispatch.</remarks>
        public void WaitFor(IEnumerable<Store> stores)
        {
            if (stores == null)
                throw new ArgumentNullException(nameof(stores));

            foreach (var store in stores)
            {
                if (store == null)
                    throw new ArgumentException("Cannot contain 'null' stores.", nameof(stores));
                WaitFor(store);
            }
        }

        /// <summary>Waits for the provided <paramref name="stores"/> to complete.</summary>
        /// <param name="stores">A collection of previously subscribed <see cref="Store"/>s to wait for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when there is no action currently dispatching.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet handled the current action dispatch.</remarks>
        public void WaitFor(params Store[] stores)
            => WaitFor((IEnumerable<Store>)stores);

        private void _EnterDispatch()
        {
            if (Interlocked.CompareExchange(ref _state, _dispatchingState, _availableState) != _availableState)
                throw new InvalidOperationException("Cannot dispatch action while there is an action currently dispatching.");
        }

        private void _ExitDispatch()
        {
            Interlocked.Exchange(ref _state, _availableState);
        }

        private void _DispatchAction(object action)
        {
            try
            {
                foreach (var subscriber in _subscribers)
                    _remainingSubscribers.AddLast(subscriber);

                _currentAction = action;
                _currentSubscriber = _remainingSubscribers.First;
                while (_currentSubscriber != null)
                {
                    _currentSubscriber.Value(action);
                    var nextSubscriber = _currentSubscriber.Next;
                    _remainingSubscribers.Remove(_currentSubscriber);
                    _currentSubscriber = nextSubscriber;
                }
            }
            finally
            {
                _currentSubscriber = null;
                _remainingSubscribers.Clear();
            }
        }

        private void _DispatchNext(object id, object action)
        {
            if (id is LinkedListNode<IMiddleware> middlewareNode && middlewareNode.List == _middlewarePipeline)
                if (middlewareNode.Next == null)
                    _DispatchAction(action);
                else
                    middlewareNode.Next.Value.Handle(new MiddlewareContext(middlewareNode.Next, action, this));
            else
                throw new ArgumentException("The provided id does not correspond to a configured middleware.", nameof(id));
        }

#if !NET20 && !NET30 && !NET35
        private Task _DispatchNextAsync(object id, object action, CancellationToken cancellationToken)
        {
            if (id is LinkedListNode<IMiddleware> middlewareNode && middlewareNode.List == _middlewarePipeline)
                if (middlewareNode.Next == null)
                {
                    _DispatchAction(action);
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
#endif

        private void _CheckDeadlockWait(Action<object> callback)
        {
            var waitingSubscriber = _remainingSubscribers.First;
            while (waitingSubscriber != _currentSubscriber && waitingSubscriber.Value != callback)
                waitingSubscriber = waitingSubscriber.Next;

            if (waitingSubscriber.Value == callback)
                throw new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.");
        }

        private sealed class MiddlewareContext : IMiddlewareContext
        {
            private readonly object _id;
            private readonly Dispatcher _dispatcher;

            public MiddlewareContext(object id, object action, Dispatcher dispatcher)
            {
                _id = id;
                Action = action;
                _dispatcher = dispatcher;
            }

            public object Action { get; }

            public void Dispatch(object action)
                => _dispatcher._DispatchAction(action);

            public void Next(object action)
                => _dispatcher._DispatchNext(_id, action);

            public void Next()
                => _dispatcher._DispatchNext(_id, Action);
        }

#if !NET20 && !NET30 && !NET35
        private sealed class AsyncMiddlewareContext : IMiddlewareAsyncContext
        {
            private readonly object _id;
            private readonly Dispatcher _dispatcher;

            public AsyncMiddlewareContext(object id, object action, Dispatcher dispatcher)
            {
                _id = id;
                Action = action;
                _dispatcher = dispatcher;
            }

            public object Action { get; }

            public void Dispatch(object action)
                => _dispatcher._DispatchAction(action);

            public Task NextAsync(object action, CancellationToken cancellationToken)
                => _dispatcher._DispatchNextAsync(_id, action, cancellationToken);

            public Task NextAsync(CancellationToken cancellationToken)
                => _dispatcher._DispatchNextAsync(_id, Action, cancellationToken);
        }
#endif
    }
}