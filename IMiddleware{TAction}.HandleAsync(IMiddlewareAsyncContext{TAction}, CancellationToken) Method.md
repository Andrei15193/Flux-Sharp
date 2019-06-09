[FluxBase](index) / [IMiddleware\<TAction\>](IMiddleware{TAction} Interface) / HandleAsync(IMiddlewareAsyncContext\<TAction\>, CancellationToken) Method
--------------------------------------------------------------------------------------------------------------------------------------------------------

Asynchronously handles a currently executing dispatch.

```c#
Task HandleAsync(IMiddlewareAsyncContext<TAction> context, CancellationToken cancellationToken)
```

### Parameters
* __context__ [IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)  
The context of the current dispatch.
* __cancellationToken__ [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken)  
A [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken) that can be used to signal cancellation.

### Returns [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task)
Returns a [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task) representing the asynchronous operation.

### Remarks
This method is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.

### See Also
* [FluxBase](index)
* [IMiddleware\<TAction\>](IMiddleware{TAction} Interface)