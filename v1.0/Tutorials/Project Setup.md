Project Setup
-------------

Getting started with FluxBase is rather easy. To showcase how this library can be used we will be implementing a Contacts application (Windows Universal Application) in this tutorial series. We will name it the _MyContacts_ app.

The features we will be covering are listed below.
* Managing a list of contacts (add, update, remove and text based search)
* Contacts have first and last names, e-mail address and a phone number
* Storage is done in JSON files for simplicity

Let's get started, create a new Universal Windows Platform Application, blank template will do. We will be using dependency injection to take care of our service instantiation and related types lifecycle, for this we will use [Unity Container](https://github.com/unitycontainer/unity). Let's install the NuGet packages that we will be using: [FluxBase](https://www.nuget.org/packages/FluxBase) and [Unity](https://www.nuget.org/packages/Unity).

Both the [Dispatcher](../Dispatcher Class) and the [Store](../Store Class)s that we will be defining will be singletons. Each [Store](../Store Class) will be declared as resources in `App.xaml` to make them globally available and benefit from [IntelliSense](https://docs.microsoft.com/visualstudio/ide/using-intellisense) when defining our views. There is no need to configure them in the dependency container as singletons right now, we only need to configure the [Dispatcher](../Dispatcher Class) since we will be needing one for each actions object.

In `App.xaml.cs` we will be defining our dependency container and registering all of our yet to be defined [Store](../Store Class)s to handle dispatched messages.

```c#
sealed partial class App : Application
{
    private static readonly IUnityContainer _dependencyContainer = new UnityContainer()
        .RegisterSingleton<Dispatcher>();

    public App()
    {
        InitializeComponent();
        Suspending += OnSuspending;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        var dispatcher = _dependencyContainer.Resolve<Dispatcher>();
        foreach (var store in Resources.Values.OfType<Store>())
            dispatcher.Register(store);

        var rootFrame = Window.Current.Content as Frame;
        if (Window.Current.Content as Frame == null)
        {
            rootFrame = new Frame();
            rootFrame.Navigated += _OnNavigated;
            rootFrame.NavigationFailed += _OnNavigationFailed;
            Window.Current.Content = rootFrame;
        }

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            Window.Current.Activate();
        }
    }

    private void _OnNavigated(object sender, NavigationEventArgs e)
    {
        _dependencyContainer.BuildUp(e.Content.GetType(), e.Content);
    }

    private void _OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        deferral.Complete();
    }
}
```

Most of this is code coming from the blank app template, the important parts are the dependency injection configuration at the beginning, the store registration and the navigated event handler.

```c#
private static readonly IUnityContainer _dependencyContainer = new UnityContainer()
    .RegisterSingleton<Dispatcher>();
```

We will be updating the configuration as we build the application, right now we have just the [Dispatcher](../Dispatcher Class) configured as presented before.

```c#
var dispatcher = _dependencyContainer.Resolve<Dispatcher>();
foreach (var store in Resources.Values.OfType<Store>())
    dispatcher.Register(store);
```

Any [Store](../Store Class) that we will be declaring in the application resources will be registered with our [Dispatcher](../Dispatcher Class) to receive all messages, each will handle them respectively.

```c#
private void _OnNavigated(object sender, NavigationEventArgs e)
{
    _dependencyContainer.BuildUp(e.Content.GetType(), e.Content);
}
```

Any navigation that occurs will ensure the target page has all of its dependencies injected, this may not be enough for more complex applications as there would be more UI components that call one or more different actions and they too will need to have their dependencies injected.

That's all for the project setup, we have configured our main project and we are ready to implement the features.

Next part is [Data and Main View](Data and Main View) where we will be defining our data models and main view.