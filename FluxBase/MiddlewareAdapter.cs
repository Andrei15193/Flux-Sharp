using System;
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETCOREAPP1_0 || NETCOREAPP1_1
using System.Reflection;
#endif
#if !NET20 && !NET30 && !NET35
using System.Threading;
using System.Threading.Tasks;
#endif

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

#if !NET20 && !NET30 && !NET35
        public Task HandleAsync(IMiddlewareAsyncContext context, CancellationToken cancellationToken)
        {
            if (_IsCompatible<TAction>(context.Action))
                return _middleware.HandleAsync(new MiddlewareAsyncContextAdapter<TAction>(context), cancellationToken);
            else
                return context.NextAsync(cancellationToken);
        }
#endif

        private static bool _IsCompatible<T>(object value)
        {
            if (value is T)
                return true;
            else if (value == null)
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETCOREAPP1_0 || NETCOREAPP1_1
                if (typeof(T).GetTypeInfo().IsValueType)
#else
                if (typeof(T).IsValueType)
#endif
                    return Nullable.GetUnderlyingType(typeof(T)) != null;
                else
                    return true;
            else
                return false;
        }
    }
}