using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EmailService.Core;
using EmailService.Service.Templating;
using NUnit.Framework;

namespace EmailService.Service.Tests.Templating
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ForTests
    {
        private const string Path = "./Templating/TestTemplates/For/";
        private TemplateEngine engine;

        private readonly IDictionary<string, object> data = new Dictionary<string, object>
        {
            ["empty_list"] = new List<IDictionary<string, object>>(),
            ["small_list"] = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "Banana"
                },
                new Dictionary<string, object>
                {
                    ["name"] = "Apple"
                },
                new Dictionary<string, object>
                {
                    ["name"] = "Grape"
                },
                new Dictionary<string, object>
                {
                    ["name"] = "Watermelon"
                },
                new Dictionary<string, object>
                {
                    ["name"] = "Kiwifruit"
                }
            },
            ["wrong_type"] = "not a list"
        };

        [SetUp]
        public void Setup()
        {
            engine = new TemplateEngine();
        }
        
        /*[TestCase("5_Items.txt", "5_Items.output.txt")]
        [TestCase("No_Key.txt", "No_Key.output.txt")]
        [TestCase("Wrong_Type.txt", "Wrong_Type.output.txt")]
        [TestCase("Empty.txt", "Empty.output.txt")]
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