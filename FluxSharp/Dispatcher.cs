using System;
using System.Collections.Generic;
using System.Threading;

namespace FluxSharp
{
    /// <summary>Represents a dispatcher, responsible for publishing messages to subscribes (stores). Follows the publish-subscribe pattern.</summary>
    public class Dispatcher
    {
        private const int _availableState = 0;
        private const int _invokingState = 1;

        private int _state = _availableState;
        private readonly ICollection<Action<ActionData>> _subscribers = new List<Action<ActionData>>();
        private readonly LinkedList<Action<ActionData>> _remainingSubscribers = new LinkedList<Action<ActionData>>();
        private ActionData _currentActionData = null;
        private LinkedListNode<Action<ActionData>> _currentSubscriber = null;

        /// <summary>Initializes a new instance of the <see cref="Dispatcher"/> class.</summary>
        public Dispatcher()
        {
        }

        /// <summary>Registers the provided <paramref name="store"/> for messages.</summary>
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
        /// <returns>Returns <c>true</c> if the store was unregistered; otherwise <c>false</c>.</returns>
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
        public object Register(Action<ActionData> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!_subscribers.Contains(callback))
                _subscribers.Add(callback);
            return callback;
        }

        /// <summary>Unregisters the callback with the provided <paramref name="id"/> from notifications.</summary>
        /// <param name="id">The ID object previously returned from calling the <see cref="Register(Action{ActionData})"/> method.</param>
        /// <returns>Returns <c>true</c> if the handler was unregistered; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c>.</exception>
        public bool Unregister(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            return id is Action<ActionData> callback && _subscribers.Remove(callback);
        }

        /// <summary>Publishes a message to all subscribed callbacks.</summary>
        /// <param name="actionData">The message to dispatch.</param>
        public void Dispatch(ActionData actionData)
        {
            if (Interlocked.CompareExchange(ref _state, _invokingState, _availableState) != _availableState)
                throw new InvalidOperationException("Cannot dispatch message while there is a message currently dispatching.");

            try
            {
                foreach (var subscriber in _subscribers)
                    _remainingSubscribers.AddLast(subscriber);

                _currentActionData = actionData;
                _currentSubscriber = _remainingSubscribers.First;
                while (_currentSubscriber != null)
                {
                    _currentSubscriber.Value(actionData);
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
        /// <param name="id">The ID object previously returned from calling the <see cref="Register(Action{ActionData})"/> method.</param>
        public void WaitFor(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (id is Action<ActionData> callback && _currentSubscriber.Value != callback && _remainingSubscribers.Contains(callback))
            {
                _CheckDeadlockWait(callback);
                _remainingSubscribers.Remove(callback);
                _currentSubscriber = _remainingSubscribers.AddAfter(_currentSubscriber, callback);

                _currentSubscriber.Value(_currentActionData);

                var previousSubscriber = _currentSubscriber.Previous;
                _remainingSubscribers.Remove(_currentSubscriber);
                _currentSubscriber = previousSubscriber;
            }
        }

        /// <summary>Waits for the provided <paramref name="store"/> to complete.</summary>
        /// <param name="store">A <see cref="Store"/> previously subscribed using the <see cref="Register(Store)"/> method.</param>
        public void WaitFor(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            WaitFor(new Action<ActionData>(store.Handle));
        }

        private void _CheckDeadlockWait(Action<ActionData> callback)
        {
            var waitingSubscriber = _remainingSubscribers.First;
            while (waitingSubscriber != _currentSubscriber && waitingSubscriber.Value != callback)
                waitingSubscriber = waitingSubscriber.Next;

            if (waitingSubscriber.Value == callback)
                throw new InvalidOperationException("Deadlock detected. Two handlers are waiting on each other (directly or indirectly) to complete.");
        }
    }
}