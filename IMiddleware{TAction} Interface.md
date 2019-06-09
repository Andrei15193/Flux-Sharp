[FluxBase](index) / IMiddleware\<TAction\> Interface
----------------------------------------------------

Represents a typed middleware pipeline element for handling specific actions before and after they are actually dispatched to action handlers.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IMiddleware<TAction>
```

### Generic Parameters
* __TAction__ The type of actions to handle.

### Methods
* __[Handle(IMiddlewareContext\<TAction\>)](IMiddleware{TAction}.Handle(IMiddlewareContext{TAction}) Method)__ - Handles a currently executing dispatch.
* __[HandleAsync(IMiddlewareAsyncContext\<TAction\>, CancellationToken)](IMiddleware{TAction}.HandleAsync(IMiddlewareAsyncContext{TAction}, CancellationToken) Method)__ - Asynchronously handles a currently executing dispatch.

### Remarks
A middleware pipeline can read the action that is currently being dispatched and decide to modify it, skip all the following middleware handlers or dispatch an action bypassing all following middleware handlers.

Middleware is similar to filters as they can be used in the same way for error reporting or logging, but can be used to split an action dispatch into multiple actual action dispatches.

A middleware implementation must cover both synchronous and asynchronous flows as neither can be adapted to the other. This is mostly because asynchronous methods cannot be adapted to synchronous ones if continuations need to execute on the same thread.

The problem boils down to "blocking" the asynchronous call until it completes when we are using the synchronous method. This is impossible because all dispatches must be carried out on the UI thread (when a dispatch is initiated from the UI thread, this is the expected behaviour) in order execute action handlers (stores) on the UI thread that eventually notify the UI components and update themselves (through binding expressions).

This restriction implies that a synchronous dispatch initiated on the UI thread must complete after the action was handled by all registered stores (or action handlers). If along the way we have an asynchronous operation (e.g.: an async middleware) then control is returned from the async method before it completes and that async middleware may dispatch actions that need to be handled on the UI thread.

Waiting for the async middleware to execute on the UI thread creates a deadlock because all methods (and fragments resulting from async transformation by the compiler) execute on the UI thread and the method blocking the execution by waiting the async middleware will actually wait until a method fragment completes that is placed after itself in the execution queue. The fragment does not execute until the method waiting for it completes.

A synchronous method can be adapted to be asynchronous to some extent. For instance, we can create a TaskCompletionSource to create the task result of the asynchronous adapter and simply call the synchronous method.

```c#
public Task MethodAsync()
{
    var taskCompletionSource = new TaskCompletionSource&lt;object&gt;();
    try
    {
        Method(); // The synchronous method we are adapting
        taskCompletionSource.SetResult(null); // Can be the result from the sync method
    }
    catch (Exception exception)
    {
        taskCompletionSource.SetException(exception);
    }
    return taskCompletionSource.Task;
}
```

Unfortunately we cannot adapt our synchronous middleware to async one because if we want to continue to the next middleware element in the pipeline we need to call `context.Next()` which is synchronous in this case. This is the same issue, `context.Next()` should needs to adapt asynchronous middleware from the pipeline in the same meaner a synchronous dispatch needs to adapt asynchronous middleware. It cannot be done since all code needs to execute on the UI thread by default.

The asynchronous flow is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.

### See Also
* [FluxBase](index)
* [IMiddleware](IMiddleware Interface)