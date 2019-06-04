Dispatcher.Unregister(Store) Method
-----------------------------------

Unregisters the provided _store_ from notifications.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public bool Unregister(Store store)
```

### Parameters
* __store__ [Store](Store Class)\
The previously subscribed to messages using the [Register(Store)](Dispatcher.Register(Store) Method) method.

### Returns [bool](https://docs.microsoft.com/dotnet/api/system.boolean)
Returns `true` if the _store_ was unregistered; otherwise `false`.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _store_ is `null`.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)