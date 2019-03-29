#if !NET20 && !NET35
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    internal class AsyncMiddlewareContextAdapter<TAction> : IAsyncMiddlewareContext<TAction>
    {
        private readonly IAsyncMiddlewareContext _context;

        public AsyncMiddlewareContextAdapter(IAsyncMiddlewareContext context)
        {
            _context = context;
        }

        public TAction Action
            => (TAction)_context.Action;

        object IAsyncMiddlewareContext.Action
            => _context.Action;

        public Task NextAsync(object action, CancellationToken cancellationToken)
            => _context.NextAsync(action, cancellationToken);

        public Task NextAsync(CancellationToken cancellationToken)
            => _context.NextAsync(cancellationToken);

        public void Dispatch(object action)
            => _context.Dispatch(action);
    }
}
#endif