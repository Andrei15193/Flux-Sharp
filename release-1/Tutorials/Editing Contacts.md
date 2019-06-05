Editing Contacts
----------------

We are able to view and add contacts, our next stop is editing them. Once the list of contacts is loaded we want to be able to select one by clicking an item in the list. The contact details will be loaded and displayed in our form, once we change some of the fields we will save the contact and just like adding a contact we want the list to be updated and still have it selected.

To select an item we will listen to the [ItemClick](https://docs.microsoft.com/uwp/api/windows.ui.xaml.controls.listviewbase.itemclick) event, when this happens we will call an action that selects the clicked contact.

```xml
<ListView x:Name="ContactsListView"
          Grid.Row="1"
          Grid.Column="0"
          SelectionMode="Single"
          DataContext="{StaticResource ContactsStore}"
          ItemsSource="{Binding Contacts, Mode=OneWay}"
          SelectedItem="{Binding SelectedContact, Mode=TwoWay}"
          IsEnabled="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactDetailsStore}, Converter={StaticResource BoolConverter}, ConverterParameter=negate}"
          IsItemClickEnabled="True"
          ItemClick="_SelectContact">
```

We will use two [ActionData](../ActionData Class)s, one for signaling that the load operation started and one when it completes. We already have `PrepareProcessContactDetailsActionData` which signals the beginning of a contact operation so we will reuse that, we only need to define an [ActionData](../ActionData Class) for when the contact was loaded.

```c#
public class ContactDetailsLoadedActionData : ActionData
{
    public ContactDetails ContactDetails { get; set; }
}
```

Some of these [ActionData](../ActionData Class)s have no distinction between what data, if any, they provide. [ActionData](../ActionData Class)s are not all about what they contain, but about their type as well. We can distinguish between what each [ActionData](../ActionData Class) represents from the concrete type, an alternative to this is to provide the _action type_ as a property and filter out what we want to do in each handler, in case we handle that type of notification. It boils down to personal preferrence on how developers want to write their code, there are pros and cons for all approaches. Pick one that you think is best for your application and stick with it. If you later on find out that it doesn't work as good, you can always refactor.

Next we will extend our service to allow us to load the details of a contact.

```c#
public async Task<ContactDetails> GetAsync(Guid id)
{
    var contactFile = await ApplicationData
        .Current
        .LocalFolder
        .GetFileAsync($"{id}.json")
        .AsTask()
        .ConfigureAwait(false);

    return await _ReadContactDetailsAsync(contactFile)
        .ConfigureAwait(false);
}

private async Task<ContactDetails> _ReadContactDetailsAsync(StorageFile contactFile)
{
    using (var fileStream = await contactFile
        .OpenStreamForReadAsync()
        .ConfigureAwait(false))
    using (var streamReader = new StreamReader(fileStream))
    {
        var contactJson = await streamReader
            .ReadToEndAsync()
            .ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ContactDetails>(contactJson);
    }
}
```

Expose a new action for loading the contact details.

```c#
public async Task LoadDetailsAsync(Guid contactId)
{
    _dispatcher.Dispatch(new PrepareProcessContactDetailsActionData());
    var contactDetails = await _contactService.GetAsync(contactId);
    _dispatcher.Dispatch(
        new ContactDetailsLoadedActionData
        {
            ContactDetails = contactDetails
        }
    );
}
```

Handle `ContactDetailsLoadedActionData` in our `ContactDetailsStore`.

```c#
private void _Handle(ContactDetailsLoadedActionData actionData)
{
    SetProperty(() => ContactDetails, actionData.ContactDetails);
    SetProperty(() => IsLoading, false);
    SetProperty(() => IsLoaded, true);
}
```

Whenever a contact a selected we should update the `SelectedContact` property in `ContactsStore`.

```c#
private void _Handle(ContactDetailsLoadedActionData actionData)
{
    var selectedContact = Contacts
        .FirstOrDefault(contact => contact.Id == actionData.ContactDetails.Id);
    SetProperty(() => SelectedContact, selectedContact);
}
```

Now to call our load contact action from the event handler from our view.

```c#
private async void _SelectContact(object sender, ItemClickEventArgs e)
{
    AddContactToggleButton.IsChecked = false;
    var selectedContact = (Contact)e.ClickedItem;
    await ContactsActions.LoadDetailsAsync(selectedContact.Id);
}
```

That's it, when we select a contact we will get the edit form filled. If we press the save button it will only add a new contact.

For updating an existing contact we will be using `PrepareProcessContactDetailsActionData` and `ContactDetailsUpdatedActionData` just like in the case of `AddAsync`, we only need to define the appropriate methods. The first one is in our service.

```c#
public async Task UpdateAsync(ContactDetails contact)
{
    var contactFile = await ApplicationData
        .Current
        .LocalFolder
        .CreateFileAsync($"{contact.Id}.json", CreationCollisionOption.ReplaceExisting)
        .AsTask()
        .ConfigureAwait(false);

    await _WriteContactAsync(contact, contactFile)
        .ConfigureAwait(false);
}
```

The second update method is in our actions class.

```c#
public async Task UpdateAsync(ContactDetails contact)
{
    _dispatcher.Dispatch(new PrepareProcessContactDetailsActionData());
    await _contactService.UpdateAsync(contact);
    _dispatcher.Dispatch(
        new ContactDetailsUpdatedActionData
        {
            ContactDetails = contact
        }
    );
}
```

We already handle the [ActionData](../ActionData Class)s and there's nothing new to interpret about them, there are no changes in our stores. We only need to update the event handler for saving contacts, if it is a new one then we call `AddAsync` otherwise it is an existing one and we call `UpdateAsync`.

```c#
var contact = new ContactDetails
{
    FirstName = FirstName.Text,
    LastName = LastName.Text,
    EMail = EMail.Text,
    TelephoneNumber = TelephoneNumber.Text
};
if (AddContactToggleButton.IsChecked ?? false)
{
    AddContactToggleButton.IsChecked = false;
    await ContactsActions.AddAsync(contact);
}
else
{
    var selectedContact = (Contact)ContactsListView.SelectedItem;
    contact.Id = selectedContact.Id;
    await ContactsActions.UpdateAsync(contact);
}
```

Now, when we edit contacts they will be updated upon save. The list of contacts will be updated accordingly and the edited contact selected, just like when we add one.

In the next part we will cover [Deleting Contacts](Deleting Contacts) and introduce conditionals for more complex binding scenarios.