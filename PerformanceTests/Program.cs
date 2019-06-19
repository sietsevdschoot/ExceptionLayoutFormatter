using System;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<ExceptionExtractionPerformanceTests>();

            Console.WriteLine(JsonConvert.SerializeObject(summary, new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

        }
    }
}
