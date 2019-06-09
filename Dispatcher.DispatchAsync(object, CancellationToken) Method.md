[FluxBase](index) / [Dispatcher](Dispatcher Class) /  DispatchAsync(object, CancellationToken) Method
-----------------------------------------------------------------------------------------------------

Asynchronously dispatches an action to all subscribed callbacks.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public Task DispatchAsync(object action, CancellationToken cancellationToken)
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

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)