using System.IO;
using Xunit;

namespace OCEL.CSharp.Tests
{
    public static class XmlTests
    {
        public class SchemaValidation
        {
            [Fact]
            public void SampleXmlIsValidAccordingToSchema()
            {
                var xml = File.ReadAllText("minimal.xmlocel");
                Assert.True(Xml.Validate(xml));
            }
        }

        public class Deserialization
        {
            [Fact]
            public void CanParseSampleXml()
            {
                var xml = File.ReadAllText("minimal.xmlocel");
                Assert.NotNull(Xml.Deserialize(xml));
            }

            [Fact]
            public void ParsedSampleXmlSatisfiedWellFormednessProperty()
            {
                var xml = File.ReadAllText("minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                Assert.True(parsed.IsValid);
            }

            [Fact]
            public void CanParseGitHubPm4PyLog()
            {
                var xml = File.ReadAllText("github_pm4py.xmlocel");
                Assert.NotNull(Xml.Deserialize(xml));
            }

            [Fact]
            public void ParsedGitHubPm4PyXmlSatisfiedWellFormednessProperty()
            {
                var xml = File.ReadAllText("github_pm4py.xmlocel");
                var parsed = Xml.Deserialize(xml);
                Assert.True(parsed.IsValid);
            }
        }

        public class Serialization
        {
            [Fact]
            public void CanSerializeSampleLog()
            {
                var xml = File.ReadAllText("minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                var serialized = Xml.Serialize(parsed, Types.Formatting.Indented);
                var valid = Xml.Validate(serialized);
                Assert.True(valid);
            }

            [Fact]
            public void CanSerializeGitHubPm4PyLog()
            {
                var xml = File.ReadAllText("github_pm4py.xmlocel");
                var parsed = Xml.Deserialize(xml);
                var serialized = Xml.Serialize(parsed, Types.Formatting.Indented);
                var valid = Xml.Validate(serialized);
                Assert.True(valid);
            }

            [Fact]
            public void CanSerializeSampleLogAndDeserializeItAgain()
            {
                var xml = File.ReadAllText("minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                var serialized = Xml.Serialize(parsed, Types.Formatting.Indented);
                var reSerialized = Xml.Deserialize(serialized);
                Assert.True(reSerialized.IsValid);
            }
        }
    }
}