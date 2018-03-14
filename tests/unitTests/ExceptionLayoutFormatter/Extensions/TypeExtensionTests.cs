using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExceptionLayoutFormatter.Extensions;
using FluentAssertions;
using Xunit;

namespace UnitTests.ExceptionLayoutFormatter.Extensions
{
    public class TypeExtensionTests
    {
        [Fact]
        public void GetTypeName_Renders_typeName_as_string()
        {
            typeof(Task<IEnumerable<DateTime>>).GetTypeName().Should().Be("Task<IEnumerable<DateTime>>");
        }

        [Fact]
        public void GetTypeName_Renders_typeName_with_nullable_as_string()
        {
            typeof(Task<IEnumerable<DateTime?>>).GetTypeName().Should().Be("Task<IEnumerable<DateTime?>>");
        }
    }
}