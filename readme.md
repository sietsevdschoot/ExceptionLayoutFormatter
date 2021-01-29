

# ExceptionFormatter
ExceptionFormatter renders exceptions in a readable way. Containing important addiditional information which specialized exceptions may provide.

This allows you to get detailed exception messages together with all information nested exceptions may hold.

##### Features:
* Formats all nested- and child exception in order of relevance, Inner exceptions first, outer exception last.
* Adding important value by serializing specific metadata of specialized Exceptions. 
* Provides an easy extendable way to format exceptions according to preference.  
* Robust implementation with fallback if any error occurs.

### How to use

To use ExceptionLayoutFormatter, add an extension class for Exceptions like the sample below.
```csharp
    using ExceptionLayoutFormatter;

    public static class ExceptionExtensions
    {
        private static readonly ExceptionFormatter ExceptionFormatter;

        static ExceptionExtensions()
        {
            ExceptionFormatter = ExceptionFormatter.Create();
        }

        public static string FormatException(this Exception ex)
        {
            return ExceptionFormatter.FormatException(ex);
        }
    }
```

### Exception layouts and Extensibility

Exceptions are formatted using Exception layouts.

The sole purpose for an ExceptionLayout is to render a string given an exception

```csharp
public class OptimisticConcurrencyExceptionLayout : IExceptionLayout<OptimisticConcurrencyException>
{
	public string FormatException(IFormatter formatter, OptimisticConcurrencyException ex)
	{
		var entities = ex.StateEntries.Select(x => formatter.Serialize(x.Entity));

		return formatter.GetFormattedException(ex, entities);
	}
}
```
ExceptionLayouts can be added by passing in one or more assemblies to scan for exception layouts

```csharp 
ExceptionFormatter.Create(typeof(OptimisticConcurrencyExceptionLayout).Assembly); 
```
ExceptionLayouts allow for NLog style text renderers.
The sample below shows the default formatting:

```csharp
formatter.SetLayout("[${exceptionType}: ${message}]\n${dictionary}\n${additionalInfo}\n${stacktrace}");
```

#### Text Renderers

Text renderers should be written as ```${renderer}```

# ExceptionFormatter
ExceptionFormatter renders exceptions in a readable way. Containing important addiditional information which specialized exceptions may provide.

This allows you to get detailed exception messages together with all information nested exceptions may hold.

##### Features:
* Formats all nested- and child exception in order of relevance, Inner exceptions first, outer exception last.
* Adding important value by serializing specific metadata of specialized Exceptions. 
* Provides an easy extendable way to format exceptions according to preference.  
* Robust implementation with fallback if any error occurs.

### How to use

To create a new exception layout for an exception type you have to Implement the ```IExceptionLayout<TException>```  interface.


```csharp
public class OptimisticConcurrencyExceptionLayout : IExceptionLayout<OptimisticConcurrencyException>
{
   public string FormatException(IFormatter formatter, OptimisticConcurrencyException ex)
   {
	var entities = ex.StateEntries.Select(x => formatter.Serialize(x.Entity));

	return formatter.GetFormattedException(ex, entities);
   }
}
```


 


To use ExceptionLayoutFormatter, add an extension class for Exceptions like the sample below.
```csharp
    using ExceptionLayoutFormatter;

    public static class ExceptionExtensions
    {
        private static readonly ExceptionFormatter ExceptionFormatter;

        static ExceptionExtensions()
        {
            ExceptionFormatter = ExceptionFormatter.Create();
        }

        public static string FormatException(this Exception ex)
        {
            return ExceptionFormatter.FormatException(ex);
        }
    }
```

### Exception layouts and Extensibility

Exceptions are formatted using Exception layouts.

The sole purpose for an ExceptionLayout is to render a string given an exception

```csharp
public class OptimisticConcurrencyExceptionLayout : IExceptionLayout<OptimisticConcurrencyException>
{
	public string FormatException(IFormatter formatter, OptimisticConcurrencyException ex)
	{
		var entities = ex.StateEntries.Select(x => formatter.Serialize(x.Entity));

		return formatter.GetFormattedException(ex, entities);
	}
}
```
ExceptionLayouts can be added by passing in one or more assemblies to scan for exception layouts

```csharp 
ExceptionFormatter.Create(typeof(OptimisticConcurrencyExceptionLayout).Assembly); 
```
ExceptionLayouts allow for NLog style text renderers.
The sample below shows the default formatting:

```csharp
formatter.SetLayout("[${exceptionType}: ${message}]\n${dictionary}\n${additionalInfo}\n${stacktrace}");
```

#### Text Renderers

Text renderers should be written as ```${renderer}```

Available text renderers:

| 				 |   								    |
| -------------- | ------------------------------------ |
| exceptionType  |	exceptionTypeName					|
| message	     |  ex.Message							|
| stackTrace	 |  ex.StackTrace						|
| dictionary	 |  ex.Data								|
| additionalInfo |  specific information about the exception   |


Available text renderers:

| 				 |   								    |
| -------------- | ------------------------------------ |
| exceptionType  |	exceptionTypeName					|
| message	     |  ex.Message							|
| stackTrace	 |  ex.StackTrace						|
| dictionary	 |  ex.Data								|
| additionalInfo |  specific information about the exception   |
