using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace OCEL.CSharp.Tests
{
    public static class JsonTests
    {
        public class SchemaValidation
        {
            [Fact]
            public void SampleJsonIsValidAccordingToSchema()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
                Assert.True(Json.Validate(json));
            }
        }

        public class Deserialization
        {
            [Fact]
            public void CanParseSampleJson()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
                Assert.NotNull(Json.Deserialize(json));
            }

            [Fact]
            public void ParsedSampleJsonSatisfiedWellFormednessProperty()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
                var parsed = Json.Deserialize(json);
                Assert.True(parsed.IsValid);
            }

            [Fact]
            public void CanParseGitHubPm4PyLog()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel");
                Assert.NotNull(Json.Deserialize(json));
            }

            [Fact]
            public void ParsedGitHubPm4PyJsonSatisfiedWellFormednessProperty()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel");
                var parsed = Json.Deserialize(json);
                Assert.True(parsed.IsValid);
            }
        }

        public class Serialization
        {
            [Fact]
            public void CanSerializeSampleLog()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
                var parsed = Json.Deserialize(json);
                var serialized = Json.Serialize(parsed, Formatting.Indented);
                Assert.False(string.IsNullOrWhiteSpace(serialized));
            }

            [Fact]
            public void CanSerializeSampleLogAndDeserializeItAgain()
            {
                var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
                var parsed = Json.Deserialize(json);
                var serialized = Json.Serialize(parsed, Formatting.Indented);
                var reSerialized = Json.Deserialize(serialized);
                Assert.True(reSerialized.IsValid);
            }
        }
    }
}