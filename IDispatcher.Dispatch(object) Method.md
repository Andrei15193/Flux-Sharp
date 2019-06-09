[FluxBase](index) / [IDispatcher](IDispatcher Interface) / Dispatch(object) Method
----------------------------------------------------------------------------------

Dispatches an action to all subscribed callbacks.

```c#
void Dispatch(object action)
```

### Parameters
* __action__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The action to dispatch.

### Exceptions
* __[InvalidOperationException](https://docs.microsoft.com/dotnet/api/system.invalidoperationexception)__ - Thrown when the dispatcher is already dispatching an action.

### See Also
* [FluxBase](index)
* [IDispatcher](IDispatcher Interface)