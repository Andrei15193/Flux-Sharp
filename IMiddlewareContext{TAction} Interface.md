[FluxBase](index) / IMiddlewareContext\<TAction\> Interface
-----------------------------------------------------------

Represents a typed middleware context when handling an action dispatch.

Base interface: [IMiddlewareContext](IMiddlewareContext Interface).

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IMiddlewareContext<TAction> : IMiddlewareContext
```

### Generic Parameters
* __TAction__ The type of actions being handled.

### Properties
* __[Action](IMiddlewareContext{TAction}.Action Property)__ - Gets the action that is being dispatched.

# See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)
* [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)
* [IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)