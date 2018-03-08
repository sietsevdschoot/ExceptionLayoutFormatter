


Features:
	> Formats all nested- and child exceptions in order of relevance, Inner exceptions first, outer exception last.
	> Adding import value by serializing specific metadata of specialized Exceptions. 
	> Provides an easy to extend way to format exceptions in user preferred ways. 
	> 



------------------------
How to use
------------------------

To use ExceptionLayoutFormatter, add an extension class for Exceptions like the sample below.


```
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


------------------------
Extensibility
------------------------


```
    public class EmptyExceptionLayout : IExceptionLayout<Exception>
    {
        public string FormatException(IFormatter formatter, Exception ex)
        {
            return this.GetType().Name;
        }
    }

```