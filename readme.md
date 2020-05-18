![Publish](https://github.com/Andrei15193/FluxBase/workflows/Publish/badge.svg)

FluxBase is an implementation of Facebook's Flux architecture for .NET. For more information about the architecture model check out their page on this topic: https://facebook.github.io/flux/docs/overview.html.

The library is lightweight and contains a basic implementation of a dispatcher and a store. The dispatcher allows for handlers and stores to be registered for receiving messages and allows for handlers to wait for the completion of other handlers as well. Some dependency between stores is expected for large applications or in some edge cases.

The store is intended as a base class and provides helper methods for notifying observers when properties change and expose a method for setting property values and notify observers as well. This helps reduce boilerplate code for concrete stores.
