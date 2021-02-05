# ExceptionFormatter
ExceptionFormatter renders exceptions in a readable way. Containing important additional information which specialized exceptions may provide.

This allows you to get detailed exception messages together with all valuable information nested exceptions may hold.

##### Features:
* Formats all nested- and child exception in order of relevance, Inner exceptions first, outer exception last.
* Adding important value by serializing specific metadata of specialized Exceptions. 
* Provides an easy extendable way to format exceptions according to preference.  
* Robust implementation with fallback if any error occurs.
* Supports layouts for generic exceptions like for example ```FaultException<T>```

## How to use

Use ExceptionLayoutFormatter by adding an extension class for Exceptions.

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
Custom ExceptionLayouts can be added by scanning one or more assemblies.

```csharp 
ExceptionFormatter.Create(typeof(OptimisticConcurrencyExceptionLayout).Assembly); 
```

ExceptionLayouts can also be added explicitely

```csharp 
ExceptionFormatter.Create()
    .AddExceptionLayout(new DbEntityValidationExceptionLayout()); 
```

### Adding custom  ExceptionLayouts

To implement a layout for a given exception you have to implement the ```IExceptionLayout<TException>``` interface.

```csharp
    public interface IExceptionLayout<in TException> : IExceptionLayout
        where TException : Exception
    {
        string FormatException(IFormatter formatter, TException ex);
    }
```
The responsibility of an exceptionLayout is to render a string given an exception. 



```csharp
    public interface IFormatter
    {
        JsonSerializerSettings SerializerSettings { get; }
        void SetLayout(string layout);
        string PrettyPrint<T>(T item);
        string GetFormattedException(Exception ex, IEnumerable<string> additionalInfo);
        string GetFormattedException(Exception ex, string additionalInfo = null);
    }
```

ExceptionLayout uses NLog style renders: The following line is used as default.  

```csharp
formatter.SetLayout("[${exceptionType}: ${message}]\n${dictionary}\n${additionalInfo}\n${stacktrace}");
```

Available text renderers:

|				 |   								    |
| -------------- | ------------------------------------ |
| exceptionType  |	exceptionTypeName					|
| message	     |  ex.Message							|
| stackTrace	 |  ex.StackTrace						|
| dictionary	 |  ex.Data								|
| additionalInfo |  specific information about the exception   |

### ExceptionLayout Samples

```csharp
    public class DbEntityValidationExceptionLayout : IExceptionLayout<DbEntityValidationException>
    {
        public string FormatException(IFormatter formatter, DbEntityValidationException ex)
        {
            var entityValidationMessages = ex.EntityValidationErrors
                .Where(x => !x.IsValid)
                .Select(x => formatter.PrettyPrint(new
                {
                   ValidationErrors = x.ValidationErrors.Select(e => new
                   {
                       Property = $"{x.Entry.Entity.GetType().Name}.{e.PropertyName}",
                       Error = e.ErrorMessage
                   }),
                   Entity = x.Entry.Entity
                }
                ));
 
            return formatter.GetFormattedException(ex, entityValidationMessages);
        }
    }
```

```csharp
    public class OptimisticConcurrencyExceptionLayout : IExceptionLayout<OptimisticConcurrencyException>
    {
        public string FormatException(IFormatter formatter, OptimisticConcurrencyException ex)
        {
            var entities = ex.StateEntries.Select(x => formatter.PrettyPrint(x.Entity));

            return formatter.GetFormattedException(ex, entities);
        }
    }
```

```csharp
    public class DbUpdateExceptionLayout : IExceptionLayout<DbUpdateException>
    {
        public string FormatException(IFormatter formatter, DbUpdateException ex)
        {
            var validationErrors = new List<string>();

            try
            {
                validationErrors.AddRange(ex.Entries.Select(x => x.State == EntityState.Added
                    ? formatter.PrettyPrint(new { Current = x.CurrentValues.ToObject() })
                    : formatter.PrettyPrint(new 
                    {
                        Current = x.CurrentValues.ToObject(), 
                        Original = x.OriginalValues.ToObject()
                    })));
            }
            catch (Exception e)
            {
                validationErrors.Add($"DbUpdateExceptionLayout: {e.Message}");
                validationErrors.AddRange(ex.Entries.Select(x => formatter.PrettyPrint(x.Entity)));
            }

            return formatter.GetFormattedException(ex, validationErrors);
        }
    }
```

```csharp
    public class ReflectionTypeLoadExceptionLayout : IExceptionLayout<ReflectionTypeLoadException>
    {
        public string FormatException(IFormatter formatter, ReflectionTypeLoadException ex)
        {
            var msg = $"Unable to load:\n{string.Join("\n", ex.Types.Select(x => x.Name))}\n";

            return formatter.GetFormattedException(ex, msg);
        }
    }
```

```csharp
    public class SqlExceptionLayout : IExceptionLayout<SqlException>
    {
        public string FormatException(IFormatter formatter, SqlException ex)
        {
            formatter.SetLayout("[${exceptionType}: ${message}]\n${additionalInfo}\n${stacktrace}");

            return formatter.GetFormattedException(ex);
        }
    }
```
```csharp
    public class FaultExceptionLayout : IExceptionLayout<FaultException>
    {
        public string FormatException(IFormatter formatter, FaultException ex)
        {
            var fault = formatter.PrettyPrint(new
            {
                Reason = ex.Reason.ToString(),
                Action = ex.Action,
                Code = ex.Code?.Name,
                SubCode = ex.Code?.SubCode?.ToString(),
            });

            return formatter.GetFormattedException(ex, fault);
        }
    }
```

```csharp
    public class GenericFaultExceptionLayout<T> : IExceptionLayout<FaultException<T>>
    {
        public string FormatException(IFormatter formatter, FaultException<T> ex)
        {
            var fault = formatter.PrettyPrint(new
            {
                Reason = ex.Reason.ToString(),
                Action = ex.Action,
                Code = ex.Code?.Name,
                SubCode = ex.Code?.SubCode?.ToString(),
                Detail = ex.Detail
            });

            return formatter.GetFormattedException(ex, fault);
        }
    }
```