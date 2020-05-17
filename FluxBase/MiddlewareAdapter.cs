using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FluxBase
{
    internal class MiddlewareAdapter<TAction> : IMiddleware
    {
        private readonly IMiddleware<TAction> _middleware;

        public MiddlewareAdapter(IMiddleware<TAction> middleware)
        {
            _middleware = middleware;
        }

        public void Handle(IMiddlewareContext context)
        {
            if (_IsCompatible<TAction>(context.Action))
                _middleware.Handle(new MiddlewareContextAdapter<TAction>(context));
            else
                context.Next();
        }

        public Task HandleAsync(IMiddlewareAsyncContext context, CancellationToken cancellationToken)
        {
            if (_IsCompatible<TAction>(context.Action))
                return _middleware.HandleAsync(new MiddlewareAsyncContextAdapter<TAction>(context), cancellationToken);
            else
                return context.NextAsync(cancellationToken);
        }

        private static bool _IsCompatible<T>(object value)
        {
            if (value is T)
                return true;
            else if (value == null)
                if (typeof(T).GetTypeInfo().IsValueType)
                    return Nullable.GetUnderlyingType(typeof(T)) != null;
                else
                    return true;
            else
                return false;
        }
    }
}