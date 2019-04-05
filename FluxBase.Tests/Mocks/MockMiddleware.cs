using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase.Tests.Mocks
{
    internal class MockMiddleware : IMiddleware
    {
        private readonly Action<IMiddlewareContext> _handler;
        private readonly Func<IMiddlewareAsyncContext, CancellationToken, Task> _asyncHandler;

        public MockMiddleware(Action<IMiddlewareContext> handler, Func<IMiddlewareAsyncContext, CancellationToken, Task> asyncHandler)
        {
            _handler = handler;
            _asyncHandler = asyncHandler;
        }

        public void Handle(IMiddlewareContext context)
            => _handler(context);

        public Task HandleAsync(IMiddlewareAsyncContext context, CancellationToken cancellationToken)
            => _asyncHandler(context, cancellationToken);
    }

    internal class MockMiddleware<TAction> : IMiddleware<TAction>
    {
        private readonly Action<IMiddlewareContext<TAction>> _handler;
        private readonly Func<IMiddlewareAsyncContext<TAction>, CancellationToken, Task> _asyncHandler;

        public MockMiddleware(Action<IMiddlewareContext<TAction>> handler, Func<IMiddlewareAsyncContext<TAction>, CancellationToken, Task> asyncHandler)
        {
            _handler = handler;
            _asyncHandler = asyncHandler;
        }

        public void Handle(IMiddlewareContext<TAction> context)
            => _handler(context);

        public Task HandleAsync(IMiddlewareAsyncContext<TAction> context, CancellationToken cancellationToken)
            => _asyncHandler(context, cancellationToken);
    }
}