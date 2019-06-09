---
title: Home
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

Previous Releases
-----------------
* [Release 1 (FluxBegin)](release-1/)
    * [Version 1.0.0](https://www.nuget.org/packages/FluxBase/1.0.0)
    * [Version 1.0.1](https://www.nuget.org/packages/FluxBase/1.0.1)

Introduction
------------
Flux Base is an implementation of Facebook's Flux architecture (see
[https://facebook.github.io/flux/docs/overview.html](https://facebook.github.io/flux/docs/overview.html) for more details)
for .NET targeting .NET Framework, .NET Core and .NET Standard allowing for the library to be used for any kind of desktop
application.

The architecture model restricts the flow of information to be unidirectional. A store may not dispatch messages, a view may
not update stores directly. A view calls an action which represents the user action which in turn dispatches a messages which
in turn is handled by stores whose state updates and is reflected on the view. Once the view is updated an action can be called
again which triggers the entire cycle again. Ultimately, information follows in one direction, to minimize overhead for store
updates they implement the observer pattern together with the views. When a store updates one of it properties it notifies the
view about it so the view itself can update without having to periodically query the store.

The __Dispatcher__ acts as a central messaging component, all messages flow through here. It is a singleton in order to ensure this.
While a message is being dispatched a new messages cannot be dispatched in the same time, this is to limit cascading updates.
Calling an action initiates the cycle which ends with one or more stores being updated and eventually the view itself, having an
action called as a result of a store update can lead to a very complex behaviour of the application and prone to errors.

__Stores__ contain the view state, or the information that is being shown to the user. It is not a database, it does not act as an
intermediary between the view and the service layer, it does not have any storage logic. All information is kept in memory.
Stores represent logical units of information that may be seen across views and may contain multiple entities, they do not map
1 to 1 with entities. They are responsible with managing the state of a portion of what is viewable and notifying upon change.

Stores are singletons themselves, this makes it easier to keep track of data that is loaded as the respective data is not
limited by the lifetime of a view. A view may be unloaded, however the data may still be available and displayed from the same
view when it is reconstructed or from a different view. A caveat here is potential memory leaks, in order to notify views about
changes a reference to the view is kept (this happens internally with .NET when subscribing to events). Since the store is a
singleton the garbace collector will maintain the reference to the view even if it is no longer used, ensure that when manually
subscribing to events requires to manually unsubscribe from them as well when the view is being unloaded to avoid this issue.
Generally, binding expressions should be used all the time (binding mode should be either one time or one way to enforce
unidirectional flow of information) which makes life easier as the entire subscribing and unsubscribing to and from events is
handled by the framework.

An example would be the master-detail pattern, a store would hold the information about the list of items or a page of items and
contain paging information in that case as well. A second store would be defined to represent the details of an item, keep in mind
that each store is a singleton. When clicking on an item from the list a load action would be called that in turn will dispatch
a message with the loaded item. The second store (detail store) will unload all previous information and load the new information
about the currently selected item. Even though there are multiple items, only one is shown at a time, there is no nead to load
the details of all items all the time. The detail store is build in this way as there is only need of at most one at any given moment.

__Views__ are  visual elements that are provided by the framework or custom built using framework components. Their sole responsibility
is to display information and call actions upon user interactions or UI timers. They display information provided by the stores,
eventually applying conversions on the values themselves and translations, handle animations and so on.

User interaction is done through events which in turn means code-behind. This may be less convenient for .NET desktop developers as
the main pattern is Model-View-ViewModel in which code-behind is generally avoided in favour of commands and converters. On the other
hand this is not really an issue. A ViewModel that loads information from stores and forwards command calls to actions can be
implemented. The only caveat is ensuring ViewModel lifetime is not affected by the store's lifetime, i.e. the ViewModel would
subscribe to the property changed event of the store which in turn will maintain a reference to the ViewModel itself. If the
ViewModel lives as long as the view lives then when the view is discarded the ViewModel will continue to live on due to the fact
that stores are singletons and live as long as the application lives. When the view is recreated a new ViewModel is created as well,
the first ViewModel cannot be reused because the view that created it was discarded and thus a memory leak is born.

This can be mitigated by having custom accessors for the ViewModel's property changed event. As long as there are any listeners the
ViewModel will be subscribed to the store's property changed event, when this no longer happens the ViewModel unsubscribes from the
store and no memory leak happens. Ensure that when the view is discarded all bindings unsubscribe from the ViewModel's events and
it is not assumed that the view model will die along with the view and the only references are the circular ones between the view
and the ViewModel. Something similar to the sample below may do the trick.

```c#
public class ViewModel : INotifyPropertyChanged
{
    public Store Store { get; set; }

    private PropertyChangedEventHandler _propertyChangedEventHandler;

    public event PropertyChangedEventHandler PropertyChanged
    {
        add
        {
            if (_propertyChangedEventHandler == null && Store != null)
                Store.PropertyChanged += _StorePropertyChanged;
            _propertyChangedEventHandler += value;
        }
        remove
        {
            _propertyChangedEventHandler -= value;
            if (_propertyChangedEventHandler == null && Store != null)
                Store.PropertyChanged -= _StorePropertyChanged;
        }
    }

    private void _StorePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // update properties
    }
}
```

This ultimately leads to a lot of code duplication just for the sake of having a ViewModel and little to no code-behind.

__Actions__ encapsulate behaviour and map to user actions such as searching for a term, viewing a list of items, loading
the details of an item and so on. They act as a bridge between the user and the service implementations that actually do
what the user asks. The only way of communicating "back" is to dispatch messages, there shouldn't be any return values
except for [Task](https://docs.microsoft.com/dotnet/api/system.threading.tasks.task)s in case of asynchronous operations.

Some operations are simple and can work with types provided by the framework alone (e.g.: strings for searching terms,
GUIDs for deleting entities and so on). Sometimes there are operations that require more complex data, such as adding
an entity (contains name, description possibly other information) in which case the appropriate way of doing it is to
pass an instance of a class. This is a place where it can get a bit messy as the age old question will unavoidably pop up:
"Do we use the same types across all our layers because they all look the same right now or we define a set at each layer
through copy paste?".

In case of the former this should not be a problem, the view will pass the exact same type of object to the actions which
in turn will pass it to the service layer and so on. When actions dispatch messages they will contain objects of types
that are globally available, nice and easy until discrepancies start to show up between how layers communicate in which
case there will either be muddy models (exposing properties that are contextually used, may be properly loaded or not and
so on), multiple types that represent the same thing (more or less, some have more information than others) and at the same
time have types that go through all layers since they all look the same. With proper planing of a naming and refactoring
strategy this can work and has its benefits (no recreating of objects when it is not necessary), no time wasted on
copy/pasting type definitions across layers and possibly others.

In the latter case there is a global pattern that enforces each layer to define along its interface the data transfer
objects as well. Arguably the inputs and outputs of an interface are part of the contract and thus should go together.
This approach maintains the interaction between layers more clear and more clean, there is no question whether the
returned object is fully initialized, all of its properties must be properly initialized because if they are not used
they can be removed, the impact is local and there is no fear of damaging other areas of the application.

One of the benefits of this approach is that at any given point layers can be split into separate projects and even
made available as NuGet packages allowing them to be reused across applications. The downside is that for every layer
the application has to create a new instance of an object that contains more or less the exact same information which
in turn may have a very small impact on performance. On the other hand too many objects has rarely been the case of
performance issues unless the entire database was being downloaded, loops in memory are extremely fast, a few thousand
iterations are hardly visible.

Since actions act as the bridge between the UI layer and the business layer all mappings between data transfer objects
should be done in the actions. Data transfer objects at the UI layer can be referred to as "view models" (to not be
confused with view models from MVVM) while the business layer data transfer objects can be referred as "business models"
or just "models" since they are the closest they will get to how the business is modeled in the application.

Stores and views work with view models while actions receive view models as parameters, they map them to business models
and call business services, upon receiving a result they map the result to view models and dispatch them. This helps keep
the layer clean as everyone can speak the same language.

In either case, keep in mind "Premature optimization is the root of all evil" - Donald Knuth.

__Asynchronous__ operations are more than expected in todays applications. The entire cycle starting with calling an action,
dispatching a message, handling the message and updating the view is synchronous and must happen on the UI thread. At the
same time there is no restriction to how many dispatches an action can make. Generally there is just one dispatch in
case of synchronous operations, for asynchronous operations there should be two. One that indicates the operation was
initiated (e.g. "prepare to receive data") and another when the operation completes and the result is provided.

The preparation dispatch can unload any unneeded data and set a loading state for the stores that are about to receive an
update, in turn, some of the views can display a progress bar or a progress ring indicating that the operation is underway and that
it may take a bit of time until it completes. Once the result dispatch is made the stores will load the information
and update their loading state to done and the views will no longer show the progress bar and display the
information.

__Cancellation__ is an important part of asynchronous operations. This can be handled in multiple ways, either in the views
or in the actions. The question is which will maintain the [CancellationTokenSource](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtokensource),
having it in the view enables easier management, there should only be one operation that can be cancelled in this way meaning
that there should only be one button that does the actual cancellation. Having multiple such operations can be confusing
for the user. If such a case is encontered then multiple [CancellationTokenSource](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtokensource)s
can be maintained by the view.

A second approach is to have the action maintain a [CancellationTokenSource](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtokensource) and
expose a cancel method to represent the cancellation action. One of the issues here is exposing multiple methods for actions
that may be run in parallel and be able to actually only run one since the cancel method is generic and having a cancel
method for each action method may actually lead to boilerplate code and have the actions class actually contain a large
number of [CancellationTokenSource](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtokensource) fields.

The cases in which an action or a number of actions can be run in parallel are best described by the UI, the easiest place
to maintain [CancellationTokenSource](https://docs.microsoft.com/dotnet/api/system.threading.cancellationtokensource)s is
in the view, but it is a question of whether it should be doing that as well.