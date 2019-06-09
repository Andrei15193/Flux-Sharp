[FluxBase](index) / IMiddlewareAsyncContext\<TAction\> Interface
----------------------------------------------------------------

Represents a typed middleware context when handling an asynchronous action dispatch.

Base interface: [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface).

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IMiddlewareAsyncContext<TAction> : IMiddlewareAsyncContext
```

### Generic Parameters
* __TAction__ The type of actions being handled.

### Properties
* __[Action](IMiddlewareAsyncContext{TAction}.Action Property)__ - Gets the action that is being dispatched.

### Remarks
This type is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.

# See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)
* [IMiddlewareContext\<TAction\>](IMiddlewareContext{TAction} Interface)
* [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)