[FluxBase](index) / [Store](Store Class) / Handle(object) Method
----------------------------------------------------------------

Handles the provided _action_.

This is a [public](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/public) [virtual](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/virtual) method.

```c#
public virtual void Handle(object action)
```

### Parameters
* __action__ [object](https://docs.microsoft.com/dotnet/api/system.object)  
The action that was dispatched.

### Remarks
The default implementation maps all public methods with one parameter that return [void](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/void) and picks the method whose parameter is closest to the actual type of the provided _action_.

If there is a method accepting the same actual type of the provided _action_ then that method is called, otherwise the method with the most sepcific base class (i.e.: the closest base type in the inheritance chain) is called, if one can be found.

### See Also
* [FluxBase](index)
* [Store](Store Class)