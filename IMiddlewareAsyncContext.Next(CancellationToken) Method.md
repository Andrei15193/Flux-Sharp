[FluxBase](index) / [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface) / NextAsync(CancellationToken) Method
----------------------------------------------------------------------------------------------------------------------

Calls the next middleware in the chain with the same [Action](IMiddlewareAsyncContext.Action Property).

```c#
Task NextAsync(CancellationToken cancellationToken)
```

### Parameters
* __cancellationToken__ [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken)  
A [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken) that can be used to signal cancellation.

### Returns [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task)
Returns a [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task) representing the asynchronous operation.

### Remarks
In case there is no next middleware handler configured then the [Action](IMiddlewareAsyncContext.Action Property) will be dispatched to all registered action handlers (stores).

### See Also
* [FluxBase](index)
* [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)