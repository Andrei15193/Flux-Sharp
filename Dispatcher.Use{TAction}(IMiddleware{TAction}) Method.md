[FluxBase](index) / [Dispatcher](Dispatcher Class) / Use\<TAction\>(IMiddleware\<TAction\>) Method
---------------------------------------------------------------------------------------------------

Configures the given _middleware_ as the last handler in the pipeline.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) method.

```c#
public object Use<TAction>(IMiddleware<TAction> middleware)
```

### Generic Parameters
* __TAction__ The type of actions for which the middleware applies.

### Parameters
* __middleware__ [IMiddleware\<TAction\>](IMiddleware{TAction} Interface)  
The [IMiddleware](IMiddleware{TAction} Interface)\<TAction\> to configure.

### Returns [object](https://docs.microsoft.com/dotnet/api/system.object)
Returns the ID of the configured _middleware_.

### Exceptions
* __[ArgumentNullException](https://docs.microsoft.com/dotnet/api/system.argumentnullexception)__ - Thrown when _middleware_ is `null`.

### Remarks
The middleware pipeline is called in the same order they are configured, configuring a middleware handler multiple times will not reorder it. The respective instance will be called multiple times.

### See Also
* [FluxBase](index)
* [Dispatcher](Dispatcher Class)