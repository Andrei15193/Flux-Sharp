[FluxBase](index) / [Dispatcher](Dispatcher Class) / WaitFor(IEnumerable\<object\>) Method
-------------------------------------------------------------------------------------------

Waits for the registered handlers with the provided _ids_ to complete.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public void WaitFor(IEnumerable<object> ids)
```

### Parameters
* __ids__ [IEnumerable](https://docs.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[object](https://docs.microsoft.com/dotnet/api/system.object)\>  
A collection of ID objects previously returned from calling the [Register(Action\<object\>)](Dispatcher.Register(Action{object}) Method) method.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _ids_ is `null`.
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _ids_ contains `null` values.

### Remarks
The method only blocks for referred callbacks that are registered and have not yet been executed.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)