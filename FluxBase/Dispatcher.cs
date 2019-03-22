using System;
using System.Collections.Generic;
using System.Threading;

namespace FluxBase
{
    /// <summary>Represents a dispatcher, responsible for publishing messages to subscribers (stores). Follows the publish-subscribe pattern.</summary>
    public class Dispatcher
    {
        private const int _availableState = 0;
        private const int _dispatchingState = 1;

        private int _state = _availableState;
        private readonly ICollection<Action<object>> _subscribers = new List<Action<object>>();
        private readonly LinkedList<Action<object>> _remainingSubscribers = new LinkedList<Action<object>>();
        private object _currentAction = null;
        private LinkedListNode<Action<object>> _currentSubscriber = null;

        /// <summary>Initializes a new instance of the <see cref="Dispatcher"/> class.</summary>
        public Dispatcher()
        {
        }

        /// <summary>Indicates whether the dispatcher is currently dispatching a message.</summary>
        public bool IsDispatching
            => _state == _dispatchingState;

        /// <summary>Registers the provided <paramref name="store"/> for notifications. A <see cref="Store"/> may only be registered once.</summary>
        /// <param name="store">The <see cref="Store"/> to register.</param>
        /// <returns>Returns an object as an ID that can be used to unregister the provided <paramref name="store"/> from messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        public object Register(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            return Register(store.Handle);
        }

        /// <summary>Unregisters the provided <paramref name="store"/> from notifications.</summary>
        /// <param name="store">The previously subscribed to messages using the <see cref="Register(Store)"/> method.</param>
        /// <returns>Returns <c>true</c> if the <paramref name="store"/> was unregistered; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        public bool Unregister(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));
            return _subscribers.Remove(store.Handle);
        }

        /// <summary>Registers the provided <paramref name="callback"/> for notifications. A callback may only be registered once.</summary>
        /// <param name="callback">The callback that will handle messages published by the dispatcher.</param>
        /// <returns>Returns an object as an ID that can be used to unregister the provided <paramref name="callback"/> from messages.</returns>
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

        /// <summary>Publishes a message to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching a message.</exception>
        public void Dispatch(object action)
        {
            if (Interlocked.CompareExchange(ref _state, _dispatchingState, _availableState) != _availableState)
                throw new InvalidOperationException("Cannot dispatch message while there is a message currently dispatching.");

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
                Interlocked.Exchange(ref _state, _availableState);
            }
        }

        /// <summary>Waits for the registered handler with the provided <paramref name="id"/> to complete.</summary>
        /// <param name="id">The ID object previously returned from calling the <see cref="Register(Action{object})"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
        /// <remarks>The method only blocks if the referred callback is registered and has not yet been executed.</remarks>
        public void WaitFor(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

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

        /// <summary>Waits for the registered handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of ID objects previously returned from calling the <see cref="Register(Action{object})"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
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

        /// <summary>Waits for the registered handlers with the provided <paramref name="ids"/> to complete.</summary>
        /// <param name="ids">A collection of ID objects previously returned from calling the <see cref="Register(Action{object})"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ids"/> contains <c>null</c> values.</exception>
        /// <remarks>The method only blocks for referred callbacks that are registered and have not yet been executed.</remarks>
        public void WaitFor(params object[] ids)
            => WaitFor((IEnumerable<object>)ids);

        /// <summary>Waits for the provided <paramref name="store"/> to complete.</summary>
        /// <param name="store">A <see cref="Store"/> previously subscribed using the <see cref="Register(Store)"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <c>null</c>.</exception>
        /// <remarks>The method only blocks if the referred <paramref name="store"/> is registered and has not yet been executed.</remarks>
        public void WaitFor(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            WaitFor(new Action<object>(store.Handle));
        }

        /// <summary>Waits for the provided <paramref name="stores"/> to complete.</summary>
        /// <param name="stores">A collection of <see cref="Store"/>s previously subscribed using the <see cref="Register(Store)"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet been executed.</remarks>
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
        /// <param name="stores">A collection of <see cref="Store"/>s previously subscribed using the <see cref="Register(Store)"/> method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stores"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="stores"/> contains <c>null</c> values.</exception>
        /// <remarks>The method only blocks for referred <paramref name="stores"/> that are registered and have not yet been executed.</remarks>
        public void WaitFor(params Store[] stores)
            => WaitFor((IEnumerable<Store>)stores);

        private void _CheckDeadlockWait(Action<object> callback)
        {
            var waitingSubscriber = _remainingSubscribers.First;
            while (waitingSubscriber != _currentSubscriber && waitingSubscriber.Value != callback)
                waitingSubscriber = waitingSubscriber.Next;

            if (waitingSubscriber.Value == callback)
                throw new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.");
        }
    }
}