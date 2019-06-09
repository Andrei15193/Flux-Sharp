[FluxBase](index) / [Dispatcher](Dispatcher Class) /  WaitFor(Store\[\]) Method
-------------------------------------------------------------------------------

Waits for the provided _stores_ to complete.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public void WaitFor(params Store[] stores)
```

### Parameters
* __store__ [Store](Store Class)\[\]  
A collection of [Store](Store Class)s previously subscribed using the [Register(Store)](Dispatcher.Register(Store) Method) method.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _stores_ is `null`.
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _stores_ contains `null` values.

### Remarks
The method only blocks for referred _stores_ that are registered and have not yet been executed.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)