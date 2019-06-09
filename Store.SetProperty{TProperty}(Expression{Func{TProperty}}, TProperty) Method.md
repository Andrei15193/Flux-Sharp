[FluxBase](index) / [Store](Store Class) / SetProperty\<TProperty\>(Expression\<Func\<TProperty\>\>, TProperty) Method
----------------------------------------------------------------------------------------------------------------------

Dynamically updates a property and notifies observers about the change.

This is a [protected](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/protected) method.

```c#
protected void SetProperty<TProperty>(Expression<Func<TProperty>> property, TProperty value)
```

### Generic Parameters
* __TProperty__ The type of the property that was changed.

### Parameters
* __property__ [Expression](https://docs.microsoft.com/dotnet/api/system.linq.expressions.expression-1)\<[Func\<TProperty\>](https://docs.microsoft.com/dotnet/api/system.func-1)\>  
The property to update.
* __value__ TProperty  
The new value to set to the property.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _property_ is `null`.
* __[ArgumentException](https://docs.microsoft.com/dotnet/api/system.argumentexception)__ - Thrown when _property_ does not resolve to a property of the current instance.

### Remarks
This method simplifies stores by removing the boilerplate code for writing properties that notify observers upon change. Using this method the store class can have less clutter. Without using this method the store would look something like this:

```c#
public class MyStore : Store
{
    private int _value1;

    public int Property1
    {
        get => _value1;
        set
        {
            _value1 = value;
            NotifyPropertyChanged(nameof(Property1)); // or just NotifyPropertyChanged()
        }
    }

    protected override void Handle(Action action)
    {
        Property1++;
    }
}
```

Using `SetProperty` will simplify this to the following:

```c#
public class MyStore : Store
{
    public int Property1 { get; private set; }

    protected override void Handle(Action action)
    {
        SetProperty(() => Property1, Property1 + 1);
    }
}
```

There is no need to explicitly back properties with a field and manually raise the [PropertyChanged](Store.PropertyChanged Event) event. This is all done by the `SetProperty` method.

This method is not available for .NET Framework 2.0 and .NET Framework 3.0 builds.

### See Also
* [FluxBase](index)
* [Store](Store Class)