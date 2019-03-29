using System;
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETCOREAPP1_0 || NETCOREAPP1_1
using System.Reflection;
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
            if (Helper.IsCompatible<TAction>(context.Action))
                _middleware.Handle(new MiddlewareContextAdapter<TAction>(context));
            else
                context.Next();
        }
    }
}