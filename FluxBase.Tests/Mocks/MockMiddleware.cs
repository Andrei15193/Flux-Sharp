using System;

namespace FluxBase.Tests.Mocks
{
    internal class MockMiddleware : IMiddleware
    {
        private readonly Action<IMiddlewareContext> _handler;

        public MockMiddleware(Action<IMiddlewareContext> handler)
        {
            _handler = handler;
        }

        public void Handle(IMiddlewareContext context)
            => _handler(context);
    }

    internal class MockMiddleware<TAction> : IMiddleware<TAction>
    {
        private readonly Action<IMiddlewareContext<TAction>> _handler;

        public MockMiddleware(Action<IMiddlewareContext<TAction>> handler)
        {
            _handler = handler;
        }

        public void Handle(IMiddlewareContext<TAction> context)
            => _handler(context);
    }
}