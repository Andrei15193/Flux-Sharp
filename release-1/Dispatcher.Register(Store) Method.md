Dispatcher.Register(Store) Method
---------------------------------

Registers the provided _store_ for notifications. A [Store](Store Class) may only be registered once.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public object Register(Store store)
```

### Parameters
* __store__ [Store](Store Class)\
The [Store](Store Class) to register.

### Returns [object](https://docs.microsoft.com/dotnet/api/system.object)
Returns an object as an ID that can be used to unregister the provided _store_ from messages.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _store_ is `null`.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)