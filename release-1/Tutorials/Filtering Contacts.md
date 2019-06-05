Filtering Contacts
------------------

We have our contacts app, it's great, we saved a ton of contacts but it's starting to get a bit difficult to search through them. It is great that the contacts are sorted, but we would still like to be able to go through them more easily. Let's add a text box that will help us filter the contacts and show the ones that have anything matching.

We'll add a textbox, whenever the we hit the enter key we will trigger the filter. The filter will go through all the contact details, it will unload any selected contact. This is very similar to the initial load that we are performing, the only difference is we apply a filter, that's all.

The text box and contacts list will need to be disabled whenever one of the stores in awaitng a result, in other words. If one of the `IsLoading` properties is `true` then we need to disable them. We will define a new conditional for this.

```xml
<conditionals:OrConditional x:Key="IsLoading"
                            LeftOperand="{Binding IsLoading, Source={StaticResource ContactsStore}}"
                            RightOperand="{Binding IsLoading, Source={StaticResource ContactDetailsStore}}" />
```

Now we can update our list binding and define our filter text box.

```xml
<Grid Grid.Row="1"
      Grid.Column="0"
      Margin="0,10,0,0">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition />
    </Grid.RowDefinitions>
    <TextBox Grid.Row="0"
             InputScope="Search"
             KeyUp="_FilterKeyUp"
             IsEnabled="{Binding IsFalse, Mode=OneWay, Source={StaticResource IsLoading}}" />

    <!-- The TwoWay binding is only there as a workaround for a silly bug, it has no effect on the source.
         Having the binding set to OneWay it will not update the list selection when the property is updated, it magically works using a TwoWay binding -->
    <ListView x:Name="ContactsListView"
              Grid.Row="1"
              SelectionMode="Single"
              DataContext="{StaticResource ContactsStore}"
              ItemsSource="{Binding Contacts, Mode=OneWay}"
              SelectedItem="{Binding SelectedContact, Mode=TwoWay}"
              IsEnabled="{Binding IsFalse, Mode=OneWay, Source={StaticResource IsLoading}}"
              IsItemClickEnabled="True"
              ItemClick="_SelectContact">
        <ListView.ItemTemplate>
            <DataTemplate>
                <TextBlock>
                    <Run Text="{Binding FirstName, Mode=OneWay}" /><Run Text=" " /><Run Text="{Binding LastName, Mode=OneWay}" />
                </TextBlock>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>
</Grid>
```

No new [ActionData](../ActionData Class)s needed, we will use the existing ones. We only need one more handler in our `ContactsStore` for when we unload the contact details. Up until now it was not possible to deselect a contact.

```c#
private void _Handle(UnloadContactDetailsActionData actionData)
{
    SetProperty(() => SelectedContact, null);
}
```

Now to add a method in our service to retrieve filtered contacts.

```c#
public async Task<IReadOnlyCollection<Contact>> GetFilteredAsync(string filterString)
{
    var filters = filterString.Split(' ').Where(filter => !string.IsNullOrWhiteSpace(filter));

    if (!filters.Any())
        return await GetAllAsync().ConfigureAwait(false);

    var files = await ApplicationData
        .Current
        .LocalFolder
        .GetFilesAsync()
        .AsTask()
        .ConfigureAwait(false);

    var contacts = await Task
        .WhenAll(files.Select(_ReadContactDetailsAsync))
        .ConfigureAwait(false);

    var result = (
        from contact in contacts
        where filters.Any(filter => _PassesFilter(contact, filter))
        select new Contact
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName
        }).ToList();

    return result;

}

private bool _PassesFilter(ContactDetails contact, string filter)
    => contact.FirstName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
    || contact.LastName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
    || contact.EMail.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
    || contact.TelephoneNumber.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
```

The implementation is rather simplistic, we load all the details, see if any of the porperties contain any of the values (separated by white space) provided in the filter string. The contacts that do are returned.

Next we will make an action method available so we can call it from the event handler.

```c#
public async Task FilterContactsAsync(string filterString)
{
    _dispatcher.Dispatch(new PrepareProcessContactsActionData());
    var contacts = await _contactService.GetFilteredAsync(filterString);
    _dispatcher.Dispatch(
        new ContactsLoadedActionData
        {
            Contacts = contacts
        }
    );
}
```

The final step is implementing the event handler to trigger the filter and thus help us with searching through our contacts.

```c#
private async void _FilterKeyUp(object sender, KeyRoutedEventArgs e)
{
    if (e.Key == VirtualKey.Enter)
    {
        AddContactToggleButton.IsChecked = false;
        ContactsActions.UnloadDetails();

        var searchTextBox = (TextBox)sender;
        await ContactsActions.FilterContactsAsync(searchTextBox.Text);
    }
}
```

These are the basics of using Flux, there are more improvements that can be done to our simple application like form validation and cancellation. We could use observable collections and update them accordingly instead of resetting them. For the purpose of getting started with FluxBase and covering the most common flows this tutorial series covers enough ground to start building applications with this library. Thanks for reading!