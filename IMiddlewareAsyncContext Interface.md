[FluxBase](index) / IMiddlewareAsyncContext Interface
-----------------------------------------------------

Represents the middleware context when handling an asynchronous action dispatch.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IMiddlewareAsyncContext
```

### Properties
* __[Action](IMiddlewareAsyncContext.Action Property)__ - Gets the action that is being dispatched.

### Methods
* __[NextAsync(CancellationToken)](IMiddlewareAsyncContext.Next(CancellationToken) Method)__ - Calls the next middleware in the chain with the same action.
* __[NextAsync(object, CancellationToken)](IMiddlewareAsyncContext.Next(object, CancellationToken) Method)__ - Calls the next middleware in the chain with the given action.

# See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)
* [IMiddlewareContext\<TAction\>](IMiddlewareContext{TAction} Interface)
* [IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)