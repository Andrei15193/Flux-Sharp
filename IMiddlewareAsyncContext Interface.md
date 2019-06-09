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
* __[NextAsync(CancellationToken)](IMiddlewareAsyncContext.NextAsync(CancellationToken) Method)__ - Calls the next middleware in the chain with the same action.
* __[NextAsync(object, CancellationToken)](IMiddlewareAsyncContext.NextAsync(object, CancellationToken) Method)__ - Calls the next middleware in the chain with the given action.

### Remarks
This type is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.

# See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)
* [IMiddlewareContext\<TAction\>](IMiddlewareContext{TAction} Interface)
* [IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)