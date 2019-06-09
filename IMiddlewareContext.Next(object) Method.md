[FluxBase](index) / [IMiddlewareContext](IMiddlewareContext Interface) / Next(object) Method
--------------------------------------------------------------------------------------------

Calls the next middleware in the chain with the given _action_.

```c#
void Next(object action)
```

### Parameters
* __action__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The action to continue with.

### Remarks
In case there is no next middleware handler configured then the _action_ will be dispatched to all registered action handlers (stores).

### See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)