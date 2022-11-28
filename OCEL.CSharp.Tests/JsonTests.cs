using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class JsonTests
    {
        private readonly ITestOutputHelper _output;

        public JsonTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SampleJsonIsValidAccordingToSchema()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            Assert.True(Json.Validate(json));
        }

        [Fact]
        public void CanParseSampleJson()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = Json.Deserialize(json);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedSampleJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = Json.Deserialize(json);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseGitHubPm4PyLog()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = Json.Deserialize(json);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedGitHubPm4PyJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = Json.Deserialize(json);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanSerializeSampleLog()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = Json.Deserialize(json);
            var serialized = Json.Serialize(parsed, Types.Formatting.Indented);
            var valid = Json.Validate(serialized);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.True(valid);
        }

        [Fact]
        public void CanSerializeGitHubPm4PyLog()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = Json.Deserialize(json);
            var serialized = Json.Serialize(parsed, Types.Formatting.Indented);
            var valid = Json.Validate(serialized);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.True(valid);
        }

        [Fact]
        public void CanSerializeSampleLogAndDeserializeItAgain()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = Json.Deserialize(json);
            var serialized = Json.Serialize(parsed, Types.Formatting.Indented);
            var reSerialized = Json.Deserialize(serialized);
            Assert.True(reSerialized.IsValid);
        }
    }
}