[FluxBase](index) / [Dispatcher](Dispatcher Class) / Dispatch(ActionData) Method
---------------------------------------------------------------------------------

Publishes a message to all subscribed callbacks.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public void Dispatch(ActionData actionData)
```

### Parameters
* __actionData__ [ActionData](ActionData Class)  
The message to dispatch.

### Exceptions
* __[InvalidOperationException](https://docs.microsoft.com/dotnet/api/system.invalidoperationexception)__ - Thrown when the dispatcher is already dispatching a message.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)