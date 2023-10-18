using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class XmlTests
    {
        private readonly ITestOutputHelper _output;
        private const string DataPath = @"..\..\..\..\..\..\data\OCEL\";

        public XmlTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SampleXmlIsValidAccordingToSchema()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal.xmlocel"));
            Assert.True(OcelXml.Validate(xml));
        }

        [Fact]
        public void GitHubPm4PyXmlIsValidAccordingToSchema()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "github_pm4py.xmlocel"));
            Assert.True(OcelXml.Validate(xml));
        }

        [Fact]
        public void CanParseSampleXml()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedSampleXmlSatisfiedWellFormednessProperty()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseGitHubPm4PyLog()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "github_pm4py.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void ParsedGitHubPm4PyXmlSatisfiedWellFormednessProperty()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "github_pm4py.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseNestedXml()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal_nested.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanSerializeNestedXml()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal_nested.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented, true);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLog()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented, true);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeGitHubPm4PyLog()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "github_pm4py.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented, true);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLogAndDeserializeItAgain()
        {
            var xml = File.ReadAllText(Path.Combine(DataPath, "minimal.xmlocel"));
            var parsed = OcelXml.Deserialize(xml, true);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented, true);
            var reSerialized = OcelXml.Deserialize(serialized, true);
            Assert.True(reSerialized.IsValid);
        }
    }
}