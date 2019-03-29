#if !NET20 && !NET35
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    internal class AsyncMiddlewareAdapter<TAction> : IAsyncMiddleware
    {
        private readonly IAsyncMiddleware<TAction> _middleware;

        public AsyncMiddlewareAdapter(IAsyncMiddleware<TAction> middleware)
        {
            _middleware = middleware;
        }

        public Task HandleAsync(IAsyncMiddlewareContext context, CancellationToken cancellationToken)
        {
            if (Helper.IsCompatible<TAction>(context.Action))
                return _middleware.HandleAsync(new AsyncMiddlewareContextAdapter<TAction>(context), cancellationToken);
            else
                return context.NextAsync(cancellationToken);
        }
    }
}
#endif