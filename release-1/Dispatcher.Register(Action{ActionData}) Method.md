Dispatcher.Register(Action<ActionData>) Method
----------------------------------------------

Registers the provided _callback_ for notifications. A callback may only be registered once.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public object Register(Action<ActionData> callback)
```

### Parameters
* __callback__ [Action](https://docs.microsoft.com/dotnet/api/system.action-1)\<[ActionData](ActionData Class)\>\
The callback that will handle messages published by the dispatcher.

### Returns [object](https://docs.microsoft.com/dotnet/api/system.object)
Returns an object as an ID that can be used to unregister the provided _callback_ from messages.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _callback_ is `null`.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)