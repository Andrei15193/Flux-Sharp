---
title: FluxBase
---

FluxBase Namespace
------------------
* __[IDispatcher](IDispatcher Interface)__ - Represents an interface for dispatching actions.
* __[IMiddleware](IMiddleware Interface)__ - Represents a middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.
* __[IMiddlewareContext](IMiddlewareContext Interface)__ - Represents the middleware context when handling an action dispatch.
* __[IMiddlewareAsyncContext](IMiddlewareAsyncContext Interface)__ - Represents the middleware context when handling an asynchronous action dispatch.
* __[IMiddleware\<TAction\>](IMiddleware{TAction} Interface)__ - Represents a middleware pipeline element for handling actions before and after they are actually dispatched to action handlers.
* __[IMiddlewareContext\<TAction\>](IMiddlewareContext{TAction} Interface)__ - Represents a typed middleware context when handling an action dispatch.
* __[IMiddlewareAsyncContext\<TAction\>](IMiddlewareAsyncContext{TAction} Interface)__ - Represents a typed middleware context when handling an asynchronous action dispatch.
* __[Dispatcher](Dispatcher Class)__ - Represents a dispatcher, responsible for dispatching actions to subscribers (stores). Follows the publish-subscribe pattern.
* __[Store](Store Class)__ - Represents a store, responsible with managing application view state.

Previous Releases
-----------------
* [Release 1 (FluxBegin)](release-1/)
    * [Version 1.0.0](https://www.nuget.org/packages/FluxBase/1.0.0)
    * [Version 1.0.1](https://www.nuget.org/packages/FluxBase/1.0.1)

Introduction
------------
FluxBase is an implementation of Facebook's Flux architecture (see
[https://facebook.github.io/flux/docs/overview.html](https://facebook.github.io/flux/docs/overview.html) for more details)
for .NET targeting .NET Framework, .NET Core and .NET Standard allowing for the library to be used for any kind of desktop
application.

The architecture model restricts the flow of information to be unidirectional. A store may not dispatch actions, a view may
not update stores directly. A view dispatches an action which represents the user action which in turn is handled by stores
whose state updates and is reflected on the view. Once the view is updated an action can be dispatched again which triggers
the entire cycle again. Ultimately, information follows in one direction, to minimize overhead for store updates they
implement the observer pattern together with the views. When a store updates one of it properties it notifies the view about
it so the view itself can update without having to periodically query the store.

The __Dispatcher__ acts as a central messaging component, all actions flow through here. In order to ensure this there should
only be one dispatcher instance per application, a singleton. While an action is being dispatched a new one cannot be
dispatched during this time. This limits cascading updates where one dispatch is the source of another dispatch while it is
being handled by a store. Action creators, on the other hand, can dispatch actions one after the other with the condition of
waiting for each dispatch to complete before continuing. This pattern is common with asynchronous flows.

Dispatching an action initiates the cycle which ends with one or more stores being updated and eventually the view itself,
having another dispatch directly as a result of a store update (not from user interaction, e.g.: clicking a button) can lead
to a very complex behaviour of the application and prone to errors, hence the limitation.

__Stores__ contain the view state, or the information that is being shown to the user. It is not a database, it does not act
as an intermediary between the view and the service layer, it does not have any storage logic. All information is kept in memory.
Stores represent logical units of information that may be seen across views and may contain multiple entities. Stores do not have
a one-to-one mapping with entities. They are responsible with managing the state of a portion of what is viewable on the UI and
notifying upon change.

Stores are singletons themselves, this makes it easier to keep track of data that is loaded as the respective data is not
limited by the lifetime of a view. A view may be unloaded, however the data may still be available and displayed from the same
view when it is reconstructed or from a different view. A caveat here is potential memory leaks, in order to notify views about
changes a reference to the view is kept (this happens internally with .NET when subscribing to events). Since the store is a
singleton the garbace collector will maintain the reference to the view even if it is no longer used. Ensure that when manually
subscribing to events you manually unsubscribe from them as well! Generally, binding expressions should be used all the time
(binding mode should be either one time or one way to enforce unidirectional flow of information) which makes life easier as
the entire subscribing and unsubscribing to and from events is handled by the framework.

An example would be the master-detail pattern, one store would hold the information about the list of items. A second store would
be defined to represent the details of an item, keep in mind that each store is a singleton. When clicking on an item from the list
an action would be dispatched with the loaded item. The second store (detail store) will unload all previous information and load
the new information about the currently selected item. Even though there are multiple items, only one is shown at a time, there is
no nead to load the details of all items all the time. The detail store is built this way as there is only need of at most one fully
loaded item at any given moment.

As a parallel to the Model-View-ViewModel pattern, stores contain the data of a ViewModel which is updated through dispatches while
action dispatches can be associated to commands. The store is the logical container of data that is being shown while the view will
display this data. As we know, there are multiple ways to visualise data, not that we are going to have multiple views of the same
thing in an application, this rarely happens, but we will be changing the layout of our views from time to time. The data that we
display differently should not be affected by these changes as it is the exact same data. UI changes come easy this way.

__Views__ are  visual elements that are provided by the framework or custom built using framework components. Their sole responsibility
is to display information and dispatch actions upon user interactions or UI timers. They display information provided by the stores,
eventually applying some conversions on the values themselves, cover internationalization/localization, handle animations and so on.

User interaction is done through events which in turn means code-behind. This may be less convenient for .NET desktop developers as
the main pattern is Model-View-ViewModel in which code-behind is generally avoided in favour of commands and converters. On the other
hand this is not really an issue as we can encapsulate action dispatches inside commands (adapter pattern) and bind to those instead.

__Actions__ describe events that happen in the application such as searching for a term, viewing a list of items, loading the details
of an item and so on. Having services in our views can introduce complexity and unwarranted responsibility on our visual components
especially if we need to map the result of a service to a different [data transfer object](https://en.wikipedia.org/wiki/Data_transfer_object),
not to mention mapping inputs to what a service expects.

To mitigate this we can encapsulate the service calls and dispatches to separate objects and have the UI use them. The responsibility
of mapping view data transfer objects to service ones and dispatching actions falls unto these UI services. The flow is a little bit
different than what we usually have, a UI service method takes inputs as usual parameters, but does not have a result. The result is
actually dispatched to stores and eventually the result is visible on the UI.

__Asynchronous__ operations are more than expected in today's applications. The entire cycle starting with calling an action,
dispatching an action, handling it and updating the view is synchronous and must happen on the UI thread. Generally there is just one
dispatch in case of synchronous operations, for asynchronous operations there should be two. One that indicates the operation was
initiated (e.g. "prepare to receive data") and another when the operation completes and the result is provided.

The preparation dispatch can unload any unneeded data and set a loading state for the stores that are about to receive an update, in
turn some of the views can display a progress bar or a progress ring indicating that the operation is underway and that it may take
a bit of time until it completes. Once the result dispatch is made the stores will load the information and update their loading
state to done and the views will no longer show the progress bar and display the information.