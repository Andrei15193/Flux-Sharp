Dispatcher.WaitFor(object) Method
---------------------------------

Waits for the registered handler with the provided _id_ to complete.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public void WaitFor(object id)
```

### Parameters
* __id__ [object](https://docs.microsoft.com/dotnet/api/system.object)\
The ID object previously returned from calling the [Register(Action\<ActionData\>)](Dispatcher.Register(Action{ActionData}) Method) method.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _id_ is `null`.

### Remarks
The method only blocks if the referred callback is registered and has not yet been executed.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)