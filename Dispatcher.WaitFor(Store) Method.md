[FluxBase](index) / [Dispatcher](Dispatcher Class) /  WaitFor(Store) Method
---------------------------------------------------------------------------

Waits for the provided _store_ to complete.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public void WaitFor(Store store)
```

### Parameters
* __store__ [Store](Store Class)  
A [Store](Store Class) previously subscribed using the [Register(Store)](Dispatcher.Register(Store) Method) method.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _store_ is `null`.

### Remarks
The method only blocks if the referred _store_ is registered and has not yet been executed.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)