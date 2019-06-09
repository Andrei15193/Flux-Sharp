[FluxBase](index) / [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface) / NextAsync(object, CancellationToken) Method
------------------------------------------------------------------------------------------------------------------------------

Calls the next middleware in the chain with the given _action_.

```c#
Task NextAsync(object action, CancellationToken cancellationToken)
```

### Parameters
* __action__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The action to continue with.
* __cancellationToken__ [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken)  
A [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken) that can be used to signal cancellation.

### Returns [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task)
Returns a [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task) representing the asynchronous operation.

### Remarks
In case there is no next middleware handler configured then the _action_ will be dispatched to all registered action handlers (stores).

### See Also
* [FluxBase](index)
* [IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)