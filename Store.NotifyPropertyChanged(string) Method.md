[FluxBase](index) / [Store](Store Class) / NotifyPropertyChanged(string) Method
-------------------------------------------------------------------------------

Notifies that a property was changed.

This is a [protected](https://docs.microsoft.com/dotnet/csharp/language-reference/keywords/protected) method.

```c#
protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
```

### Parameters
* __propertyName__ [string](https://docs.microsoft.com/dotnet/api/system.string)  
The name of the property that was changed.  
__Attributes__: [CallerMemberNameAttribute](https://docs.microsoft.com/dotnet/api/system.runtime.compilerservices.callermembernameattribute)  
__Default value__: `null`.

### Remarks
This method is not available for .NET Framework 2.0, .NET Framework 3.0, .NET Framework 3.5 and .NET Framework 4.0 builds.

### See Also
* [FluxBase](index)
* [Store](Store Class)