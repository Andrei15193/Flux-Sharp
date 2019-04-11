#if !NET20 && !NET30 && !NET35
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    internal class MiddlewareAsyncContextAdapter<TAction> : IMiddlewareAsyncContext<TAction>
    {
        private readonly IMiddlewareAsyncContext _context;

        public MiddlewareAsyncContextAdapter(IMiddlewareAsyncContext context)
        {
            _context = context;
        }

        public TAction Action
            => (TAction)_context.Action;

        object IMiddlewareAsyncContext.Action
            => _context.Action;

        public Task NextAsync(object action, CancellationToken cancellationToken)
            => _context.NextAsync(action, cancellationToken);

        public Task NextAsync(CancellationToken cancellationToken)
            => _context.NextAsync(cancellationToken);
    }
}
#endif