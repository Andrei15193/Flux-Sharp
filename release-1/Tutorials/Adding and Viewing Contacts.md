Adding and Viewing Contacts
---------------------------

We have our view in place, our stores subscribed and our model ready. It's time to write our very first actions, one for loading contacts and one for adding them. We will start with the one that loads contacts so we know how to save them. For this we will declare two types subclassing [ActionData](../ActionData Class).

```c#
public class PrepareProcessContactsActionData : ActionData
{
}

public class ContactsLoadedActionData : ActionData
{
    public IReadOnlyCollection<Contact> Contacts { get; set; }
}
```

The first one signals that the operation for loading contacts was initiated, the second one concludes the operation with the list of contacts. During this time the user should not be able to make changes and a process ring should be shown to the user so they know the application is busy.

We know what the contacts store will be looking for, now we need to define our service which will do the actual loading. We will store each contact in a separate file, formatted as [JSON](https://www.json.org/). Each file name contains the ID of the contact (a [Guid](https://docs.microsoft.com/dotnet/api/system.guid)) and the `.json` extension.

```c#
public interface IContactService
{
    Task<IReadOnlyCollection<Contact>> GetAllAsync();
}

public class ContactService : IContactService
{
    public async Task<IReadOnlyCollection<Contact>> GetAllAsync()
    {
        var files = await ApplicationData
            .Current
            .LocalFolder
            .GetFilesAsync()
            .AsTask()
            .ConfigureAwait(false);

        var contacts = await Task
            .WhenAll(files.Select(_ReadContactAsync))
            .ConfigureAwait(false);
        return contacts;
    }

    private async Task<Contact> _ReadContactAsync(StorageFile contactFile)
    {
        using (var fileStream = await contactFile
            .OpenStreamForReadAsync()
            .ConfigureAwait(false))
        using (var streamReader = new StreamReader(fileStream))
        {
            var contactJson = await streamReader
                .ReadToEndAsync()
                .ConfigureAwait(false);
            return JsonConvert.DeserializeObject<Contact>(contactJson);
        }
    }
}
```

Next we will define our actions that will receive calls from our view, call the service we have just defined and dispatch messages to signal the beginning and completion of our load action.

```c#
public class ContactsActions
{
    private readonly Dispatcher _dispatcher;
    private readonly IContactService _contactService;

    public ContactsActions(Dispatcher dispatcher, IContactService contactService)
    {
        _dispatcher = dispatcher;
        _contactService = contactService;
    }

    public async Task LoadAll()
    {
        _dispatcher.Dispatch(new PrepareProcessContactsActionData());
        var contacts = await _contactService.GetAllAsync();
        _dispatcher.Dispatch(
            new ContactsLoadedActionData
            {
                Contacts = contacts
            }
        );
    }
}
```

The only thing left is to handle the [ActionData](../ActionData Class)s that are dispatched in our store. We will be using the [SetProperty\<TProperty\>(Expression\<Func\<TProperty\>\>, TProperty)](../Store.SetProperty{TProperty}(Expression{Func{TProperty}}, TProperty) Method) to notify observers about our changes and call the load action from our main view.

When the `PrepareProcessContactsActionData` is dispatched we are setting the `IsLoading` property to `true`, property to which be bind to to display our process ring. Once the `ContactsLoadedActionData` is received we set the `IsLoading` property back to `false` and update the `Contacts` property so we can display something.

```c#
public class ContactsStore : Store
{
    public bool IsLoading { get; private set; }

    public IReadOnlyCollection<Contact> Contacts { get; private set; }

    private void _Handle(PrepareProcessContactsActionData actionData)
    {
        SetProperty(() => IsLoading, true);
    }

    private void _Handle(ContactsLoadedActionData actionData)
    {
        SetProperty(() => IsLoading, false);
        SetProperty(
            () => Contacts,
            actionData
                .Contacts
                .OrderBy(contact => contact.FirstName)
                .ThenBy(contact => contact.LastName)
                .ToList()
        );
    }
}
```

Last thing to get things going, when the view is loaded we will call the `LoadAll` action so we will eventually get to see all of our contacts. To do this we need to inject the `ContactsActions`, subscribe to the [Loaded](https://docs.microsoft.com/dotnet/api/system.windows.frameworkelement.loaded) event and call the action.

```c#
public sealed partial class MainPage : Page
{
    [Dependency]
    public ContactsActions ContactsActions { get; set; }

    public MainPage()
    {
        InitializeComponent();
        Loaded += async delegate
        {
            await ContactsActions.LoadAll();
        };
    }
}
```

That's all we need to display contacts, now let's make it possible to add a few. We will add a command bar with an add button on our main view. The button will open the contact details form with empty fields. The add button will be a toggle one, it will be active when we are editing the details of a new contact and inactive when we want to dismiss the form.

```xml
<Page.BottomAppBar>
    <CommandBar IsEnabled="{Binding IsLoading, Source={StaticResource  ContactsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}">
        <AppBarToggleButton x:Name="AddContactToggleButton"
                            Icon="Add"
                            Label="contact"
                            IsEnabled="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactDetailsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}"
                            Checked="_ResetContactButtonClick"
                            Unchecked="_UnloadContactButtonClick" />
    </CommandBar>
</Page.BottomAppBar>
```

As we can see we are using a new converter, `BoolConverter` which doesn't do much. It converts a [bool](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/bool) to a [bool](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/bool), or does nothing, with the same option as `BoolToVisibilityConverter` by which we can negate the value. This is the actual utility of the converter, it allows us to negate the value of a [bool](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/bool).

```c#
public class BoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var compareValue = (bool)value;
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

Same as before, we will define the [ActionData](../ActionData Class)s for this flow. One for reseting the form, one for hiding the form, one for signaling that the save operation has started and one that the save operation completed.

```c#
public class ResetContactDetailsActionData : ActionData
{
}

public class UnloadContactDetailsActionData : ActionData
{
}

public class PrepareProcessContactDetailsActionData : ActionData
{
}

public class ContactDetailsUpdatedActionData : ActionData
{
    public ContactDetails ContactDetails { get; set; }
}
```

First part is to get the form to show and hide as we toggle the add button. For this we will define two new methods on our actions class that will dispatch the corresponding [ActionData](../ActionData Class).

```c#
public void Reset()
{
    _dispatcher.Dispatch(new ResetContactDetailsActionData());
}

public void UnloadDetails()
{
    _dispatcher.Dispatch(new UnloadContactDetailsActionData());
}
```

Next we will update our `ContactDetailsStore` to handle these actions.

```c#
private void _Handle(ResetContactDetailsActionData actionData)
{
    SetProperty(() => ContactDetails, new ContactDetails());
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, true);
}

private void _Handle(UnloadContactDetailsActionData actionData)
{
    SetProperty(() => ContactDetails, null);
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, false);
}
```

Finally, we will call these actions from our event handles.

```c#
private void _ResetContactButtonClick(object sender, RoutedEventArgs e)
{
    ContactsActions.Reset();
}

private void _UnloadContactButtonClick(object sender, RoutedEventArgs e)
{
    ContactsActions.UnloadDetails();
}
```

This will allow us to show and dismiss the details form when we toggle the add button, when we handle the `ResetContactDetailsActionData` we can set a `ContactDetails` that contains default values when adding a new one. In our case this is not applicable, but for more complex applications there may be a few defaults that are globally configured and should be taken into consideration, this is one way to do it.

Next stop is saving the contact detail and have it show up in our list. We will add a save button on our command bar so we can actually submit the form. We will be using the same button to update existing contacts when we get there.

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
</CommandBar>
```

Next we need to extend our `ContactsService` with an `AddAsync` method to do the actual saving.

```c#
public async Task AddAsync(ContactDetails contact)
{
    contact.Id = Guid.NewGuid();

    var contactFile = await ApplicationData
        .Current
        .LocalFolder
        .CreateFileAsync($"{contact.Id}.json", CreationCollisionOption.FailIfExists)
        .AsTask()
        .ConfigureAwait(false);

    await _WriteContactAsync(contact, contactFile)
        .ConfigureAwait(false);
}

private async Task _WriteContactAsync(ContactDetails newContact, StorageFile contactFile)
{
    using (var fileStream = await contactFile
        .OpenStreamForWriteAsync()
        .ConfigureAwait(false))
    using (var streamWriter = new StreamWriter(fileStream))
    {
        var contactJson = JsonConvert.SerializeObject(newContact);
        await streamWriter
            .WriteAsync(contactJson)
            .ConfigureAwait(false);
    }
}
```

Same as before, we are using the ID of our contact as the file name and save the entire entity as [JSON](https://www.json.org/) in the file and the load method will pick them up.

Next we will add an `AddAsync` action which will dispatch the related [ActionData](../ActionData Class)s and call the service.

```c#
public async Task AddAsync(ContactDetails contact)
{
    _dispatcher.Dispatch(new PrepareProcessContactDetailsActionData());
    await _contactService.AddAsync(contact);
    _dispatcher.Dispatch(
        new ContactDetailsUpdatedActionData
        {
            ContactDetails = contact
        }
    );
}
```

Now we will update our stores to handle these message types. For the `ContactDetails` we need to handle both. While the operation is in progress we need to be able to hide the form which means that there are no contact details loaded and that the store is awaiting a response which will be visible through the `IsLoading` property.

```c#
private void _Handle(PrepareProcessContactDetailsActionData actionData)
{
    SetProperty(() => ContactDetails, null);
    SetProperty(() => IsLoading, true);
    SetProperty(() => IsLoaded, false);
}

private void _Handle(ContactDetailsUpdatedActionData actionData)
{
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, false);
}
```

Next we will update the event handler to call the `AddAsync` action so we actually add the contact.

```c#
private async void _AddOrUpdateContactButtonClick(object sender, RoutedEventArgs e)
{
    var contact = new ContactDetails
    {
        FirstName = FirstName.Text,
        LastName = LastName.Text,
        EMail = EMail.Text,
        TelephoneNumber = TelephoneNumber.Text
    };
    AddContactToggleButton.IsChecked = false;
    await ContactsActions.AddAsync(contact);
}
```

If we run the application right now, we will be able to add contacts, but we will not be able to see them unless we reload the application. The contacts list does not update at the moment. The form will be hidden and a progress ring will be displayed until the operation completes. Let's update the `ContactsStore` as well to handle the `ContactDetailsUpdatedActionData` so we update the list when a new contact is added.

```c#
private void _Handle(ContactDetailsUpdatedActionData actionData)
{
    var newContact = new Contact
    {
        Id = actionData.ContactDetails.Id,
        FirstName = actionData.ContactDetails.FirstName,
        LastName = actionData.ContactDetails.LastName
    };
    SetProperty(
        () => Contacts,
        Contacts
            .Where(contact => contact.Id != actionData.ContactDetails.Id)
            .Concat(new[] { newContact })
            .OrderBy(contact => contact.FirstName)
            .ThenBy(contact => contact.LastName)
            .ToList()
    );
}
```

The `ContactDetailsUpdatedActionData` provides us with a `ContactDetail` instance containing all the details of a contact, but we need a `Contact` instance. The first thing we do is map from one to another, then we go through the contact list and filter out the existing contact (if there is one, it was updated) and add the new contact. Finally we sort the list so we can display our contacts alphabetically.

If we run the application now, whenever we add a contact the list on the left side will be updated as well.

In most applications, when we have a master-detail template we see two things that happen when we add a new item. The form is dismissed and the item shows up in the list (exactly where we are at this moment) or we have the form show up again and newly added item is in edit mode this time and it is selected in the list as well. Let's do that.

To enable contact selection we need to expose a `SelectedContact` property in our `ContactsStore` so we know which one is currently selected. Whenever a selection happens we need to update this property. In our case we will just add a new property and update the corresponding handle method, we only need to add `SetProperty(() => SelectedContact, newContact)` at the end.

```c#
public Contact SelectedContact { get; private set; }

private void _Handle(ContactDetailsUpdatedActionData actionData)
{
    var newContact = new Contact
    {
        Id = actionData.ContactDetails.Id,
        FirstName = actionData.ContactDetails.FirstName,
        LastName = actionData.ContactDetails.LastName
    };
    SetProperty(
        () => Contacts,
        Contacts
            .Where(contact => contact.Id != actionData.ContactDetails.Id)
            .Concat(new[] { newContact })
            .OrderBy(contact => contact.FirstName)
            .ThenBy(contact => contact.LastName)
            .ToList()
    );
    SetProperty(() => SelectedContact, newContact);
}
```

Next we need to update the binding on our view so that the list knows we have selected a contact.

```xml
<ListView x:Name="ContactsListView"
          Grid.Row="1"
          Grid.Column="0"
          SelectionMode="Single"
          DataContext="{StaticResource ContactsStore}"
          ItemsSource="{Binding Contacts, Mode=OneWay}"
          SelectedItem="{Binding SelectedContact, Mode=TwoWay}"
          IsEnabled="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactDetailsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}">
```

Two things were added, the `SelectedItem` binding which is a `TwoWay` one. This is mostly due to a bug of the `ListView`. If the binding is `OneWay` it does not detect changes when a selected item is set on the source. Since the property is read only in our store the `TwoWay` binding does not really work both ways, if we select an item from the list by clicking on it, it will not update the source meaning that our `TwoWay` binding actually works like a `OneWay` binding with the exception that it listens to updates on the source.

The second change is the binding on [IsEnabled](https://docs.microsoft.com/dotnet/api/system.windows.uielement.isenabled#System_Windows_UIElement_IsEnabled), whenever the `ContactDetailsStore` is waiting for the processing on a contact we should not be able to select contacts or do anything with the list. This is for simplicity, we need to implement proper cancellation to handle scenarios like these. For instance, what should happen if we click on a contact to load it, the progress ring is displayed to let us know that the contact is loading and while it is doing that we click on a different contact? The first request should be cancelled and a new request to load the second contact should be issued. We don't have cancellation in our application therefore having multiple asynchronous operations running at the same time can lead to inconsistent states. We can end up having one contact selected in the list and display a different one because the first request took longer.

If we run the application right now we will see the item being selected in the list, but the form does not show up. To handle this we only need to update our handler for the `ContactDetailsUpdatedActionData` to load the details of our updated contact.

```c#
private void _Handle(ContactDetailsUpdatedActionData actionData)
{
    SetProperty(() => ContactDetails, actionData.ContactDetails);
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, true);
}
```

That's all we need for adding and viewing contacts in our application. We finally get to see the Flux architecture in action, we have actions that are called through user interaction that dispatch different messages, some messages are interpreted only by one store while in other cases we have the same message handled differently by different stores. All our flows are unidirectional, user triggres action which dispatches messages which update stores which update the view.

In the next part we will be [Editing Contacts](Editing Contacts).