[FluxBase](index) / [IMiddlewareContext](IMiddlewareContext Interface) / Next() Method
--------------------------------------------------------------------------------------

Calls the next middleware in the chain with the same [Action](IMiddlewareContext.Action Property).

```c#
void Next()
```

### Remarks
In case there is no next middleware handler configured then the [Action](IMiddlewareContext.Action Property) will be dispatched to all registered action handlers (stores).

### See Also
* [FluxBase](index)
* [IMiddlewareContext](IMiddlewareContext Interface)