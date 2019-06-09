[FluxBase](index) / IDispatcher Interface
-----------------------------------------

Represents an interface for dispatching actions.

Implementing types: [Dispatcher](Dispatcher Class).

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) interface.

```c#
public interface IDispatcher
```

### Methods
* __[Dispatch(object)](IDispatcher.Dispatch(object) Method)__ - Dispatches an action to all subscribed callbacks.
* __[DispatchAsync(object)](IDispatcher.DispatchAsync(object) Method)__ - Asynchronously dispatches an action to all subscribed callbacks.
* __[DispatchAsync(object, CancellationToken)](IDispatcher.DispatchAsync(object, CancellationToken) Method)__ - Asynchronously dispatches an action to all subscribed callbacks.

# See Also
* [FluxBase](index)