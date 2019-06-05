Store Class
-----------

Represents a store, responsible with managing application view state.

Base type: [object](https://docs.microsoft.com/dotnet/api/system.object).\
Implemented interfaces: [INotifyPropertyChanged](https://docs.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged).

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) [abstract](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/abstract) class.


```c#
public abstract class Store : INotifyPropertyChanged
```

### Constructors
* __[Store()](Store Constructor)__ - Initializes a new instance of the [Store](Store Class) class.

### Events
* __[PropertyChanged](Store.PropertyChanged Event)__ - Occurs when a property value changes.

### Methods
* __[Handle(ActionData)](Store.Handle(ActionData) Method)__ - Handles the provided actionData.
* __[NotifyPropertyChanged(string)](Store.NotifyPropertyChanged(string) Method)__ - Notifies that a property was changed.
* __[SetProperty\<TProperty\>(Expression\<Func\<TProperty\>\>, TProperty)](Store.SetProperty{TProperty}(Expression{Func{TProperty}}, TProperty) Method)__ - Dynamically updates a property and notifies observers about the change.

### See Also
* [FluxBase](index)
