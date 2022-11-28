using System;
using System.IO;
using OCEL.Types;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class CombinedTests
    {
        private readonly ITestOutputHelper _output;

        public CombinedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanConvertSampleOcelJsonToOcelXml()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var log = Json.Deserialize(json);
            var xml = Xml.Serialize(log, Formatting.Indented);
            var valid = Xml.Validate(xml);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{xml}");
            Assert.True(valid);
        }

        [Fact]
        public void CanConvertSampleXmlJsonToOcelJson()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            var log = Xml.Deserialize(xml);
            var json = Json.Serialize(log, Formatting.Indented);
            var valid = Json.Validate(json);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{json}");
            Assert.True(valid);
        }
    }
}
