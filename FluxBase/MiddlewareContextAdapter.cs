namespace FluxBase
{
    internal class MiddlewareContextAdapter<TAction> : IMiddlewareContext<TAction>
    {
        private readonly IMiddlewareContext _context;

        public MiddlewareContextAdapter(IMiddlewareContext context)
        {
            _context = context;
        }

        public TAction Action
            => (TAction)_context.Action;

        object IMiddlewareContext.Action
            => _context.Action;

        public void Next(object action)
            => _context.Next(action);

        public void Next()
            => _context.Next();
    }
}