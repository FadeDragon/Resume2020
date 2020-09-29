using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EmailService.Core;
using EmailService.Service.Templating;
using NUnit.Framework;

namespace EmailService.Service.Tests.Templating
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class IfTests
    {
        private const string Path = "./Templating/TestTemplates/If/";
        private TemplateEngine engine;
        private readonly IDictionary<string, object> data = new Dictionary<string, object>
        {
            ["variable1"] = "Variable Value One",
            ["variable2"] = "Variable Value Two",
            ["variable3"] = "Variable Value Three",
            ["variable4"] = "Variable Value Four",
            ["variable5"] = "Variable Value Five",
            ["variable6"] = "Variable Value Six",
            ["variable7"] = "Variable Value Seven",
            ["variable8"] = "Variable Value Eight",
            ["variable9"] = "Variable Value Nine",
            ["variable0"] = "Variable Value Ten"
        };

        [SetUp]
        public void Setup()
        {
            engine = new TemplateEngine();
        }

        /*[TestCase("Single.txt", "Single.output.txt")]
        [TestCase("Single_No_Match.txt", "Single_No_Match.output.txt")]
        [TestCase("Multiple.txt", "Multiple.output.txt")]
        [TestCase("Nested.txt", "Nested.output.txt")]
        [TestCase("Nested_Multiple.txt", "Nested_Multiple.output.txt")]
        [Test, Category(TestCategories.Unit)]
        public void TemplateTests(string input, string output)
        {
            var template = File.ReadAllText($"{Path}{input}");
            var expected = File.ReadAllText($"{Path}{output}");

            var actual = engine.Render(template, data);

            Assert.AreEqual(expected, actual);
        }*/
    }
}