Deleting Contacts
-----------------

With all our testing from our prevrious parts we probably ended up with a bunch of contacts that don't really mean much, let's delete them. For this we will add one more [ActionData](../ActionData Class) that will represent the completion of our delete operation, when we intiate it we will be using `PrepareProcessContactDetailsActionData`. The very same one when we add and update a contact, I think we are starting to see a pattern here.

```c#
public class ContactDeletedActionData : ActionData
{
    public Guid ContactId { get; set; }
}
```

Same as before, add a new method in our service for deleting a contact. We will simply delete the file having the contact ID as its name.

```c#
public async Task DeleteAsync(Guid id)
{
    var contactFile = await ApplicationData
            .Current
            .LocalFolder
            .GetFileAsync($"{id}.json")
            .AsTask()
            .ConfigureAwait(false);
    await contactFile
        .DeleteAsync()
        .AsTask()
        .ConfigureAwait(false);
}
```

Define a delete action to call from our UI.

```c#
public async Task DeleteAsync(Guid contactId)
{
    _dispatcher.Dispatch(new PrepareProcessContactDetailsActionData());
    await _contactService.DeleteAsync(contactId);
    _dispatcher.Dispatch(
        new ContactDeletedActionData
        {
            ContactId = contactId
        }
    );
}
```

Handle the `ContactDeletedActionData` in our `ContactDetailsStore`, it is the same as when unloading a contact.

```c#
private void _Handle(ContactDeletedActionData actionData)
{
    SetProperty(() => ContactDetails, null);
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, false);
}
```

Finally, remove the contact from our list in the `ContactsStore`.

```c#
private void _Handle(ContactDeletedActionData actionData)
{
    SetProperty(() => SelectedContact, null);
    SetProperty(
        () => Contacts,
        Contacts
            .Where(contact => contact.Id != actionData.ContactId)
            .ToList()
    );
}
```

Here comes the more interesting part. Up until now in order to enable the new contact we only had to check whether the `ContactDetailsStore` was waiting on a contact operation to complete, in other words it's `IsLoading` property had to be `false`. For saving a contact we only had to check the `IsLoaded` property as we use the same button for both adding and updating a contact.

In case of the delete button we need to check two things, the `IsLoaded` property and whether a contact is loaded, the `SelectedContact` property from the `ContactsStore` has to be different than `null`, a contact has to selected. We cannot delete a contact that we haven't added yet.

We cannot use binding experessions directly and combine two fields, for this we need conditionals that allow us to bind two [bool](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/bool) properties and have a third one with the result. Whenever the value of one of the two operand properties changes, we update the result property as well. We can bind the result property of a conditional to the [IsEnabled](https://docs.microsoft.com/dotnet/api/system.windows.uielement.isenabled#System_Windows_UIElement_IsEnabled) of our delete button.

Most of our code goes into our `Conditional` base class. Both operand properties are [DependencyProperties](https://docs.microsoft.com/dotnet/api/system.windows.dependencyproperty). The result is either `true` or `false`, we expose two result properties to make it easier to bind two. In some cases we care if a conditional is true or whether it is not. The `Evaluate` method will tell us the result of the actual conditional (it can be a logical and, a logical or, maybe some other operator that we can add at any point in time).

Whenever one of the operands change we evaluate the expression again and update the `IsTrue` and `IsFalse` properties accordingly. This will trigger updates on any property bound to result ones.

```c#
public abstract class Conditional : DependencyObject, INotifyPropertyChanged
{
    public static readonly DependencyProperty LeftOperandProperty = DependencyProperty.Register(
        nameof(LeftOperand),
        typeof(bool),
        typeof(Conditional),
        new PropertyMetadata(false, _OperandPropertyChanged)
    );

    public bool LeftOperand
    {
        get => (bool)GetValue(LeftOperandProperty);
        set => SetValue(LeftOperandProperty, value);
    }

    public static readonly DependencyProperty RightOperandProperty = DependencyProperty.Register(
        nameof(RightOperand),
        typeof(bool),
        typeof(Conditional),
        new PropertyMetadata(false, _OperandPropertyChanged)
    );

    public bool RightOperand
    {
        get => (bool)GetValue(RightOperandProperty);
        set => SetValue(RightOperandProperty, value);
    }

    private static void _OperandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var conditional = (Conditional)d;

        var result = conditional.Evaluate();
        if (conditional.IsTrue != result)
        {
            conditional.IsTrue = result;
            conditional.PropertyChanged?.Invoke(conditional, new PropertyChangedEventArgs(nameof(IsTrue)));
            conditional.IsFalse = !result;
            conditional.PropertyChanged?.Invoke(conditional, new PropertyChangedEventArgs(nameof(IsFalse)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsTrue { get; private set; }

    public bool IsFalse { get; private set; }

    protected abstract bool Evaluate();
}
```

Let's implement two conditionals for the most common logical operators to see how easy it is.

```c#
public class AndConditional : Conditional
{
    protected override bool Evaluate()
        => LeftOperand && RightOperand;
}

public class OrConditional : Conditional
{
    protected override bool Evaluate()
        => LeftOperand || RightOperand;
}
```

That's it, all the heavy lifting is done in the base class. We can implement any binary logical operator at this point.

Next we will create a conditional and use it with out delete button in our view.

```xml
<conditionals:AndConditional x:Key="SelectedContactDetailsConditional"
                             LeftOperand="{Binding SelectedContact, Source={StaticResource ContactsStore}, Converter={StaticResource NullToBoolConverter}, ConverterParameter=negate}"
                             RightOperand="{Binding IsLoading, Source={StaticResource ContactDetailsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}" />
```

In order to check for `null` we need to define a converter that does that for us. Same as before we will use the `"negate"` switch to enhance our converter. By default it checks whether the `value` is `null`, if it is then the result is `true`, if not then the result is `false`. The `"negage"` converter parameter value will check whether the `value` is not `null`.

```c#
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var compareValue = value == null;
        if (string.Equals(System.Convert.ToString(parameter), "negate", StringComparison.OrdinalIgnoreCase))
            compareValue = !compareValue;
        return compareValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

Finally, we get to the delete button.

```xml
<CommandBar IsEnabled="{Binding IsLoading, Source={StaticResource  ContactsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}">
    <AppBarToggleButton x:Name="AddContactToggleButton"
                        Icon="Add"
                        Label="contact"
                        IsEnabled="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactDetailsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}"
                        Checked="_ResetContactButtonClick"
                        Unchecked="_UnloadContactButtonClick" />
    <AppBarSeparator />
    <AppBarButton Icon="Accept"
                  Label="confirm"
                  Click="_AddOrUpdateContactButtonClick"
                  IsEnabled="{Binding IsLoaded, Source={StaticResource ContactDetailsStore}, Mode=OneWay, Converter={StaticResource BoolConverter}}" />
    <AppBarButton Icon="Delete"
                  Label="delete"
                  Click="_DeleteContact"
                  IsEnabled="{Binding IsTrue, Mode=OneWay, Source={StaticResource SelectedContactDetailsConditional}}" />
</CommandBar>
```

Thanks to our conditional the delete button will be enabled when a contact is selected and loaded. We only need to call the action method from the event handler.

```c#
private async void _DeleteContact(object sender, RoutedEventArgs e)
{
    var selectedContact = (Contact)ContactsListView.SelectedItem;
    await ContactsActions.DeleteAsync(selectedContact.Id);
}
```

We have covered all core features of any application, create, read, update and delete operations.

In our final part of the tutorial we will be adding a final feature, [Filtering Contacts](Filtering Contacts).