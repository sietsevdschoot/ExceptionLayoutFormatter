using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExceptionLayoutFormatter.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace ExceptionLayoutFormatter
{
    public class ExceptionFormattingUtil : IFormatter
    {
        private string _layout;
        private readonly string[] _keywords;

        public ExceptionFormattingUtil()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new DefaultContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            };

            _keywords = new[] {"ExceptionType", "Message", "Stacktrace", "AdditionalInfo", "Dictionary"};

            SetLayout("[${exceptionType}: ${message}]\n${dictionary}\n${additionalInfo}\n${stacktrace}");
        }

        public JsonSerializerSettings SerializerSettings { get; }

        public string GetFormattedException(Exception ex, IEnumerable<string> additionalInfo)
        {
            return GetFormattedException(ex, string.Join("\n", additionalInfo));
        }

        public string GetFormattedException(Exception ex, string additionalInfo = null)
        {
            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"ExceptionType", ex.GetType().GetTypeName()},
                {"Message", ex.Message},
                {"Stacktrace", ex.StackTrace},
                {"AdditionalInfo", additionalInfo},
                {"Dictionary", PrettyPrint(ex.Data)},
            };

            var foundKeywords = Regex.Matches(_layout, @"\${(.*?)}").Cast<Match>().Select(x => x.Value).ToList();

            var formattedException = _layout;

            foreach (var foundKeyword in foundKeywords)
            {
                var value = dict[foundKeyword.Trim('$','{', '}')];

                if (!string.IsNullOrEmpty(value))
                {
                    formattedException = formattedException.Replace(foundKeyword, value);
                }
                else
                {
                    var parts = formattedException.Split(new[] { foundKeyword }, StringSplitOptions.RemoveEmptyEntries);
                    formattedException = string.Join("", new[] { parts[0].TrimEnd('\r', '\n') }.Concat(parts.Skip(1)));
                }
            }

            return formattedException;
        }

        public string PrettyPrint<T>(T item)
        {
            return !Equals(item, default(T))
                ? JsonConvert.SerializeObject(item, SerializerSettings)
                : null;
        }

        public void SetLayout(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                throw new ArgumentNullException(nameof(layout));

            var foundKeywords = Regex.Matches(layout, "{(.*?)}").Cast<Match>().Select(x => x.Value.Trim('{', '}')).ToList();

            if (foundKeywords.GroupBy(x => x).Any(x => x.Count() > 1))
                throw new ArgumentException($"Duplicate keywords found in: {string.Join(", ", foundKeywords)}");

            var unknownKeywords = foundKeywords.Where(x => !_keywords.Contains(x, StringComparer.InvariantCultureIgnoreCase)).ToList();

            if (unknownKeywords.Any())
                throw new ArgumentException($"Unknown keywords: {string.Join(", ", unknownKeywords)}");        

            _layout = layout;
        }
    }

    public interface IFormatter
    {
        JsonSerializerSettings SerializerSettings { get; }
        /// <summary>
        /// Default Layout: "[${exceptionType}: ${message}]\n${dictionary}\n${additionalInfo}\n${stacktrace}"
        /// </summary>
        /// <param name="layout"></param>
        void SetLayout(string layout);
        string PrettyPrint<T>(T item);
        string GetFormattedException(Exception ex, IEnumerable<string> additionalInfo);
        string GetFormattedException(Exception ex, string additionalInfo = null);
    }
}