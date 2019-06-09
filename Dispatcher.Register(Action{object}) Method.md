[FluxBase](index) / [Dispatcher](Dispatcher Class) /  Register(Action\<object\>) Method
---------------------------------------------------------------------------------------

Registers the provided _callback_ for notifications. A callback may only be registered once.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public object Register(Action<object> callback)
```

### Parameters
* __callback__ [Action](https://docs.microsoft.com/dotnet/api/system.action-1)\<[object](https://docs.microsoft.com/dotnet/api/system.object)\>  
The callback that will handle dispatched actions.

### Returns [object](https://docs.microsoft.com/dotnet/api/system.object)
Returns an object as an ID that can be used to wait for the provided _callback_ to complete during dispatches or unregister the provided _callback_.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _callback_ is `null`.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)