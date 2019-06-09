[FluxBase](index) / IMiddlewareContext Interface
------------------------------------------------

Represents the middleware context when handling an action dispatch.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IMiddlewareContext
```

### Properties
* __[Action](IMiddlewareContext.Action Property)__ - Gets the action that is being dispatched.

### Methods
* __[Next()](IMiddlewareContext.Next() Method)__ - Calls the next middleware in the chain with the same action.
* __[Next(object)](IMiddlewareContext.Next(object) Method)__ - Calls the next middleware in the chain with the given action.

# See Also
* [FluxBase](index)
* [IMiddlewareContext\<TAction\>](IMiddlewareContext{TAction} Interface)
* [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)
* [IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)