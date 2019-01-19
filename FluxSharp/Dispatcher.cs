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
#if NET20
        private readonly Dictionary<Action<ActionData>, object> _subscribers = new Dictionary<Action<ActionData>, object>();
#else
        private readonly HashSet<Action<ActionData>> _subscribers = new HashSet<Action<ActionData>>();
#endif

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
#if NET20
            _subscribers[callback] = null;
#else
            _subscribers.Add(callback);
#endif
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
#if NET20
                foreach (var subscriber in _subscribers.Keys)
#else
                foreach (var subscriber in _subscribers)
#endif
                    subscriber(actionData);
            }
            finally
            {
                Interlocked.Exchange(ref _state, _availableState);
            }
        }
    }
}