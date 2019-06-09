[FluxBase](index) / [Dispatcher](Dispatcher Class) /  Unregister(object) Method
-------------------------------------------------------------------------------

Unregisters the callback with the provided _id_ from notifications.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public bool Unregister(object id)
```

### Parameters
* __id__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The ID object previously returned from calling the [Register(Action\<object\>)](Dispatcher.Register(Action{object}) Method) method.

### Returns [bool](https://docs.microsoft.com/dotnet/api/system.boolean)
Returns `true` if the handler was unregistered; otherwise `false`.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _id_ is `null`.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)