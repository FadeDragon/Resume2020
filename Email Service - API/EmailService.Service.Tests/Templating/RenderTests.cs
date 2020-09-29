using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EmailService.Core;
using EmailService.Service.Templating;
using NUnit.Framework;

namespace EmailService.Service.Tests.Templating
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class RenderTests
    {
        private const string Path = "./Templating/TestTemplates/Render/";
        private TemplateEngine engine;
        private readonly IDictionary<string, object> data = new Dictionary<string, object>
        {
            ["fruit"] = "Banana"
        };

        [SetUp]
        public void Setup()
        {
            engine = new TemplateEngine();
        }

        /*[TestCase("No_Key.txt", "No_Key.output.txt")]
        [TestCase("Single_Match.txt", "Single_Match.output.txt")]
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