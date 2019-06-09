[FluxBase](index) / [IDispatcher](IDispatcher Interface) / DispatchAsync(object, CancellationToken) Method
----------------------------------------------------------------------------------------------------------

Asynchronously dispatches an action to all subscribed callbacks.

```c#
Task DispatchAsync(object action, CancellationToken cancellationToken)
```

### Parameters
* __action__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The action to dispatch.
* __cancellationToken__ [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken)  
A [CancellationToken](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtoken) that can be used to signal cancellation.

### Returns [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task)
Returns a [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task) representing the asynchronous operation.

### Exceptions
* __[InvalidOperationException](https://docs.microsoft.com/dotnet/api/system.invalidoperationexception)__ - Thrown when the dispatcher is already dispatching an action.

### Remarks
This method is not available for .NET Framework 2.0, .NET Framework 3.0 and .NET Framework 3.5 builds.

### See Also
* [FluxBase](index)
* [IDispatcher](IDispatcher Interface)