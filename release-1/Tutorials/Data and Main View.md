Data and Main View
------------------

Now that we have our project configured it's time to think about the data we will be handling and what behaviour the application will have. Based on this we can determine what information and how it is presented to the user as well as the states the UI goes through.

For simplicity we will share our models throughout the application, we do not intend to have different representations between the layers since our app is really simple.

We will be having only one window where we can see a list of all our contacts sorted alphabetically on the left side, once we click on one of the contacts we will see their details on the right side of the window. The list will take up 33% of the space while the details view will take the remainder.

This tells us what data transfer objects we will be having. One is a _Contact_ which contains the ID, first and last names for a contact, we will be using them to display them in our contacts list. A second data transfer object type is _Contact Details_ which contains all the information we are storing for a contact, we will be using them for displaying contact details. All very obvious this far. We will be defining our data transfer object in the `Models` namespace to keep things clean.

```c#
public class Contact
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}

public class ContactDetails
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string EMail { get; set; }

    public string TelephoneNumber { get; set; }
}
```

Once the window is loaded and ready for interaction we will call a load action so we get a list of all our contacts, while this happens a progress ring is displayed so the user knowns something is happening behind the scenes. Initially, our list will be empty, we haven't added any contacts yet since we can't really do that just yet. Our first feature will be adding contacts and seeing them in our list. In order to do this we need to define our [Store](../Store Class)s to which our views bind to.

One [Store](../Store Class) is the contacts store where we maintain our list of contacts and update a flag indicating whether the store is awaiting to receive information (i.e.: while contacts are beeing loaded). We will use the flag to determine whether a progress ring should be displayed or not.

```c#
public class ContactsStore : Store
{
    public bool IsLoading { get; private set; }

    public IReadOnlyCollection<Contact> Contacts { get; private set; }
}
```

Our second [Store](../Store Class) is the contact detials store where we are maintaining a `ContactDetails` instance and two flags. Same as before a flag indicating whether the [Store](../Store Class) is waiting on a message (i.e.: loading a contact from the file system) and a second flag that indicates if a `ContactDetails` instance is loaded. This flag will indicate whether we need to show the input fields for updating or adding a contact, this flag will be `true` when we are adding a new contact, in this case we haven't selected one from the list.

```c#
public class ContactDetailsStore : Store
{
    public bool IsLoading { get; private set; }

    public bool IsLoaded { get; private set; }

    public ContactDetails ContactDetails { get; private set; }
}
```

We will be updating each store as we move along, mostly with handle methods for interpreting different messages coming from the [Dispatcher](../Dispatcher Class). All our [Store](../Store Class) will be defined in a separate namespace, same as we did with the models.

Now, to define our UI. First we need to update `App.xaml` so we can reference our newly defined [Store](../Store Class)s.

```xml
<Application x:Class="MyContacts.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:stores="using:MyContacts.Stores">
    <Application.Resources>
        <stores:ContactsStore x:Key="ContactsStore" />
        <stores:ContactDetailsStore x:Key="ContactDetailsStore" />
    </Application.Resources>
</Application>
```

Our main window will use a grid as a layout panel, two rows and two columns. The first row is for the application header while the second will contain our contacts list and contact details form.

```xml
<Page x:Class="MyContacts.MainPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:converters="using:MyContacts.Converters"
      mc:Ignorable="d"
      Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Page.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter" />
    </Page.Resources>

    <Grid Margin="25">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="1*" />
            <ColumnDefinition Width="2*" />
        </Grid.ColumnDefinitions>

        <!-- Application header -->
        <TextBlock Text="MyContacts"
                   Style="{ThemeResource HeaderTextBlockStyle}" />

        <!-- A progress ring for when we load the contacts list -->
        <ProgressRing Grid.Row="1"
                      Grid.ColumnSpan="2"
                      IsActive="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactsStore}}"
                      Width="200"
                      Height="200" />

        <ListView x:Name="ContactsListView"
                  Grid.Row="1"
                  Grid.Column="0"
                  SelectionMode="Single"
                  DataContext="{StaticResource ContactsStore}"
                  ItemsSource="{Binding Contacts, Mode=OneWay}">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <TextBlock>
                        <Run Text="{Binding FirstName, Mode=OneWay}" /><Run Text=" " /><Run Text="{Binding LastName, Mode=OneWay}" />
                    </TextBlock>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- A progress ring for when we load the contact details -->
        <ProgressRing Grid.Row="1"
                      Grid.Column="1"
                      IsActive="{Binding IsLoading, Mode=OneWay, Source={StaticResource ContactDetailsStore}}"
                      Width="150"
                      Height="150" />
        <!-- The contact details form, only visible when they are loaded -->
        <Grid Grid.Row="1"
              Grid.Column="1"
              VerticalAlignment="Top"
              DataContext="{StaticResource ContactDetailsStore}"
              Visibility="{Binding IsLoaded, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="auto" />
                <ColumnDefinition />
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition />
                <RowDefinition />
                <RowDefinition />
                <RowDefinition />
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0"
                       Grid.Column="0"
                       Margin="10"
                       Text="First Name"
                       Style="{ThemeResource TitleTextBlockStyle}" />
            <TextBox x:Name="FirstName"
                     Grid.Row="0"
                     Grid.Column="1"
                     Margin="10"
                     InputScope="PersonalFullName"
                     Text="{Binding ContactDetails.FirstName, Mode=OneWay}" />

            <TextBlock Grid.Row="1"
                       Grid.Column="0"
                       Margin="10"
                       Text="Last Name"
                       Style="{ThemeResource TitleTextBlockStyle}" />
            <TextBox x:Name="LastName"
                     Grid.Row="1"
                     Grid.Column="1"
                     Margin="10"
                     InputScope="PersonalFullName"
                     Text="{Binding ContactDetails.LastName, Mode=OneWay}" />

            <TextBlock Grid.Row="2"
                       Grid.Column="0"
                       Margin="10"
                       Text="E-Mail"
                       Style="{ThemeResource TitleTextBlockStyle}" />
            <TextBox x:Name="EMail"
                     Grid.Row="2"
                     Grid.Column="1"
                     Margin="10"
                     InputScope="EmailSmtpAddress"
                     Text="{Binding ContactDetails.EMail, Mode=OneWay}" />

            <TextBlock Grid.Row="3"
                       Grid.Column="0"
                       Margin="10"
                       Text="Telephone Number"
                       Style="{ThemeResource TitleTextBlockStyle}" />
            <TextBox x:Name="TelephoneNumber"
                     Grid.Row="3"
                     Grid.Column="1"
                     Margin="10"
                     InputScope="TelephoneNumber"
                     Text="{Binding ContactDetails.TelephoneNumber, Mode=OneWay}" />
        </Grid>
    </Grid>
</Page>
```

First of, we need to define the `BoolToVisibilityConverter` type. As the name suggest we are converting a `bool` value to `visiblity`, but we want to have the option to reverse the result. I.e. by default `true` maps to `visible` while `false` to `collapsed`, we want to be able to reverse, or negate, the source value  so that `true` maps to `collapsed` and `false` maps to `visible`. We may not need it now, but it may come in handly and it is a small effort.

```c#
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var compareValue = (bool)value;
        if (string.Equals(System.Convert.ToString(parameter), "negate", StringComparison.OrdinalIgnoreCase))
            compareValue = !compareValue;

        if (compareValue)
            return Visibility.Visible;
        else
            return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

This is one of the benefits of Flux, with the unidirectional data flow we do not have to worry about converting the value back ever. It is safe to leave the `ConvertBack` as it is as we should never use two way bindings unless we really have to (see the mentioned _selected item bug_). Even in these cases the two way binding should not go all the way to the store. This can be handled using an intermediate object that accepts two way bindings but does not propagate all the way to the store, but it propagates changes from the store towards the view.

Next part is [Adding and Viewing Contacts](Adding and Viewing Contacts) where we will be adding some functionality to our application and see how all the pieces fit together.