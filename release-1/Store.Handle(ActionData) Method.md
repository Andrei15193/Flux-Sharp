[FluxBase](index) / [Store](Store Class) / Handle(ActionData) Method
--------------------------------------------------------------------

Handles the provided _actionData_.

This is a [protected](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/protected) [virtual](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/virtual) method.

```c#
protected virtual void Handle(ActionData actionData)
```

### Parameters
* __actionData__ [ActionData](ActionData Class)  
The [ActionData](ActionData Class) that was dispatched.

### Remarks
The default implementation maps all public methods that return [void](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/void)
and have one parameter that is an [ActionData](ActionData Class) or a subtype of [ActionData](ActionData Class)
and calls the method whose parameter is closest to the actual type of the provided _actionData_.

If a method where the actual type of the _actionData_ matches exactly then that method is called,
otherwise the method with the most sepcific base class (i.e.: the closest base type in the inheritance chain)
is called if one can be found.

### See Also
* [FluxBase](index)
* [Store](Store Class)