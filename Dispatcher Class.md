[FluxBase](index) / Dispatcher Class
------------------------------------

Represents a dispatcher, responsible for dispatching actions to subscribers (stores). Follows the publish-subscribe pattern.

Base type: [object](https://docs.microsoft.com/dotnet/api/system.object).  
Implemented interfaces: [IDispatcher](IDispatcher Interface).

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) class.

```c#
public class Dispatcher
```

### Constructors
* __[Dispatcher()](Dispatcher.Dispatcher Constructor)__ - Initializes a new instance of the [Dispatcher](Dispatcher Class) class.

### Properties
* __[IsDispatching](Dispatcher.IsDispatching Property)__ - Indicates whether the dispatcher is currently dispatching an action.

### Methods
* __[Dispatch(object)](Dispatcher.Dispatch(object) Method)__ - Dispatches an action to all subscribed callbacks.
* __[DispatchAsync(object)](Dispatcher.DispatchAsync(object) Method)__ - Asynchronously dispatches an action to all subscribed callbacks.
* __[DispatchAsync(object, CancellationToken)](Dispatcher.DispatchAsync(object, CancellationToken) Method)__ - Asynchronously dispatches an action to all subscribed callbacks.
* __[Register(Action\<object\>)](Dispatcher.Register(Action{object}) Method)__ - Registers the provided callback for notifications. A callback may only be registered once.
* __[Register(Store)](Dispatcher.Register(Store) Method)__ - Registers the provided store for notifications. A [Store](Store Class) may only be registered once.
* __[Unregister(object)](Dispatcher.Unregister(object) Method)__ - Unregisters the callback with the provided id from notifications.
* __[Unregister(Store)](Dispatcher.Unregister(Store) Method)__ - Unregisters the provided store from notifications.
* __[Use(IMiddleware)](Dispatcher.Use(IMiddleware) Method)__ - Configures the given middleware as the last handler in the pipeline.
* __[Use\<TAction\>(IMiddleware\<TAction\>)](Dispatcher.Use{TAction}(IMiddleware{TAction}) Method)__ - Configures the given middleware as the last handler in the pipeline.
* __[WaitFor(object)](Dispatcher.WaitFor(object) Method)__ - Waits for the registered handler with the provided id to complete.
* __[WaitFor(IEnumerable\<object\>)](Dispatcher.WaitFor(IEnumerable{object}) Method)__ - Waits for the registered handlers with the provided ids to complete.
* __[WaitFor(object\[\])](Dispatcher.WaitFor(object%5B%5D) Method)__ - Waits for the registered handlers with the provided ids to complete.
* __[WaitFor(Store)](Dispatcher.WaitFor(Store) Method)__ - Waits for the provided store to complete.
* __[WaitFor(IEnumerable\<Store\>)](Dispatcher.WaitFor(IEnumerable{Store}) Method)__ - Waits for the provided stores to complete.
* __[WaitFor(Store\[\])](Dispatcher.WaitFor(Store%5B%5D) Method)__ - Waits for the provided stores to complete.

# See Also
* [FluxBase](index)