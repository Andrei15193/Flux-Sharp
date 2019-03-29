using System;
using System.Collections.Generic;

namespace FluxBase
{
    /// <summary>Represents a dispatcher, responsible for dispatching actions to subscribers (stores). Follows the publish-subscribe pattern.</summary>
    public class Dispatcher : BaseDispatcher, IDispatcher
    {
        private readonly LinkedList<IMiddleware> _middlewarePipeline = new LinkedList<IMiddleware>();

        /// <summary>Initializes a new instance of the <see cref="Dispatcher"/> class.</summary>
        public Dispatcher()
        {
        }

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <param name="middleware">The <see cref="IMiddleware"/> to configure.</param>
        /// <returns>Returns the ID of the configured middleware.</returns>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public object Use(IMiddleware middleware)
            => _middlewarePipeline.AddLast(middleware);

        /// <summary>Configures the given <paramref name="middleware"/> as the last handler in the pipeline.</summary>
        /// <param name="middleware">The <see cref="IMiddleware"/> to configure.</param>
        /// <remarks>
        /// The middleware pipeline is called in the same order they are configured, configuring a middleware handler
        /// multiple times will not reorder it. The respective instance will be called multiple times.
        /// </remarks>
        public void Use<TAction>(IMiddleware<TAction> middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            Use(new MiddlewareAdapter<TAction>(middleware));
        }

        /// <summary>Dispatches an action to all subscribed callbacks.</summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="InvalidOperationException">Thrown when the dispatcher is already dispatching an action.</exception>
        public void Dispatch(object action)
        {
            EnterDispatch();
            try
            {
                if (_middlewarePipeline.First == null)
                    DispatchAction(action);
                else
                {
                    var firstMiddlewareNode = _middlewarePipeline.First;
                    firstMiddlewareNode.Value.Handle(new MiddlewareContext(firstMiddlewareNode, action, this));
                }
            }
            finally
            {
                ExitDispatch();
            }
        }

        /// <summary>Dispatches an action to the next middleware in the pipeline.</summary>
        /// <param name="id">The ID of the current middleware in the pipeline.</param>
        /// <param name="action">The action to dispatch.</param>
        protected void DispatchNext(object id, object action)
        {
            if (id is LinkedListNode<IMiddleware> middlewareNode && middlewareNode.List == _middlewarePipeline)
                if (middlewareNode.Next == null)
                    DispatchAction(action);
                else
                    middlewareNode.Next.Value.Handle(new MiddlewareContext(middlewareNode.Next, action, this));
            else
                throw new ArgumentException("The provided id does not correspond to a configured middleware.", nameof(id));
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
                => _dispatcher.DispatchAction(action);

            public void Next(object action)
                => _dispatcher.DispatchNext(_id, action);

            public void Next()
                => _dispatcher.DispatchNext(_id, Action);
        }
    }
}