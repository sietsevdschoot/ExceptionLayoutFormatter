using System;
using System.Data;
using System.Diagnostics;
using System.ServiceModel;
using ExceptionLayoutFormatter;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace UnitTests.TestHelpers
{
    [DebuggerStepThrough]
    public class DefaultExceptionLayout : IExceptionLayout<Exception>
    {
        public string FormatException(IFormatter formatter, Exception ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class CustomerExceptionLayout : IExceptionLayout<CustomerException>
    {
        public string FormatException(IFormatter formatter, CustomerException ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class CustomerNotFoundExceptionLayout : IExceptionLayout<CustomerNotFoundException>
    {
        public string FormatException(IFormatter formatter, CustomerNotFoundException ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
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

    [DebuggerStepThrough]
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

    public class ExceptionThrowingLayout : IExceptionLayout<OrderException>
    {
        public string FormatException(IFormatter formatter, OrderException ex)
        {
            throw new InvalidOperationException("An error occured in ExceptionLayout");
        }
    }

    public class CalculationError
    {
        public string Reason { get; set; }
    }
}