using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class XmlTests
    {
        private readonly ITestOutputHelper _output;

        public XmlTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SampleXmlIsValidAccordingToSchema()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            Assert.True(OcelXml.Validate(xml));
        }

        [Fact]
        public void GitHubPm4PyXmlIsValidAccordingToSchema()
        {
            var xml = File.ReadAllText("github_pm4py.xmlocel");
            Assert.True(OcelXml.Validate(xml));
        }

        [Fact]
        public void CanParseSampleXml()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedSampleXmlSatisfiedWellFormednessProperty()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseGitHubPm4PyLog()
        {
            var xml = File.ReadAllText("github_pm4py.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedGitHubPm4PyXmlSatisfiedWellFormednessProperty()
        {
            var xml = File.ReadAllText("github_pm4py.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanSerializeSampleLog()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeGitHubPm4PyLog()
        {
            var xml = File.ReadAllText("github_pm4py.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented);
            _output.WriteLine($"Serialized XML:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLogAndDeserializeItAgain()
        {
            var xml = File.ReadAllText("minimal.xmlocel");
            var parsed = OcelXml.Deserialize(xml);
            var serialized = OcelXml.Serialize(parsed, Types.Formatting.Indented);
            var reSerialized = OcelXml.Deserialize(serialized);
            Assert.True(reSerialized.IsValid);
        }
    }
}