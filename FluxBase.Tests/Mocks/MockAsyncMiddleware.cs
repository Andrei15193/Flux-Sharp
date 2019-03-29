using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase.Tests.Mocks
{
    internal class MockAsyncMiddleware : IAsyncMiddleware
    {
        private readonly Func<IAsyncMiddlewareContext, CancellationToken, Task> _handler;

        public MockAsyncMiddleware(Func<IAsyncMiddlewareContext, CancellationToken, Task> handler)
        {
            _handler = handler;
        }

        public Task HandleAsync(IAsyncMiddlewareContext context, CancellationToken cancellationToken)
            => _handler(context, cancellationToken);
    }

    internal class MockAsyncMiddleware<TAction> : IAsyncMiddleware<TAction>
    {
        private readonly Func<IAsyncMiddlewareContext<TAction>, CancellationToken, Task> _handler;

        public MockAsyncMiddleware(Func<IAsyncMiddlewareContext<TAction>, CancellationToken, Task> handler)
        {
            _handler = handler;
        }

        public Task HandleAsync(IAsyncMiddlewareContext<TAction> context, CancellationToken cancellationToken)
            => _handler(context, cancellationToken);
    }
}