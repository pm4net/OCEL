using System;
using System.IO;
using LiteDB;
using OCEL.Types;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class CombinedTests
    {
        private readonly ITestOutputHelper _output;
        private readonly string _dataPath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "..", "..", "data", "OCEL"));

        public CombinedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanConvertSampleOcelJsonToOcelXml()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var log = OcelJson.Deserialize(json, true);
            var xml = OcelXml.Serialize(log, Formatting.Indented, true);
            var valid = OcelXml.Validate(xml);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{xml}");
            Assert.True(valid);
        }

        [Fact]
        public void CanConvertSampleXmlJsonToOcelJson()
        {
            var xml = File.ReadAllText(Path.Combine(_dataPath, "minimal.xmlocel"));
            var log = OcelXml.Deserialize(xml, true);
            var json = OcelJson.Serialize(log, Formatting.Indented, true);
            var valid = OcelJson.Validate(json);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{json}");
            Assert.True(valid);
        }

        [Fact]
        public void CanConvertNestedSampleOcelJsonToOcelXml()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal_nested.jsonocel"));
            var log = OcelJson.Deserialize(json, true);
            var xml = OcelXml.Serialize(log, Formatting.Indented, true);
            var valid = OcelXml.Validate(xml);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{xml}");
            Assert.True(valid);
        }

        [Fact]
        public void CanConvertNestedSampleXmlJsonToOcelJson()
        {
            var xml = File.ReadAllText(Path.Combine(_dataPath, "minimal_nested.xmlocel"));
            var log = OcelXml.Deserialize(xml, true);
            var json = OcelJson.Serialize(log, Formatting.Indented, true);
            var valid = OcelJson.Validate(json);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{json}");
            Assert.True(valid);
        }

        [Fact]
        public void CanConvertSampleOcelJsonToLiteDb()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var log = OcelJson.Deserialize(json, true);
            var liteDb = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(liteDb, log, true);
        }

        [Fact]
        public void CanConvertSampleOcelXmlToLiteDb()
        {
            var xml = File.ReadAllText(Path.Combine(_dataPath, "minimal.xmlocel"));
            var log = OcelXml.Deserialize(xml, true);
            var liteDb = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(liteDb, log, true);
        }
    }
}
