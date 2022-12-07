using System;
using System.IO;
using OCEL.Types;
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
            Assert.True(OcelJson.Validate(json));
        }

        [Fact]
        public void GitHubPm4PyJsonIsValidAccordingToSchema()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            Assert.True(OcelJson.Validate(json));
        }

        [Fact]
        public void CanParseSampleJson()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedSampleJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseGitHubPm4PyLog()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedGitHubPm4PyJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseNestedJson()
        {
            var json = File.ReadAllText("minimal_nested.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanSerializeNestedJson()
        {
            var json = File.ReadAllText("minimal_nested.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            var serialized = OcelJson.Serialize(parsed, Formatting.Indented);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLog()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeGitHubPm4PyLog()
        {
            var json = File.ReadAllText("github_pm4py.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLogAndDeserializeItAgain()
        {
            var json = File.ReadAllText("minimal.jsonocel");
            var parsed = OcelJson.Deserialize(json);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented);
            var reSerialized = OcelJson.Deserialize(serialized);
            Assert.True(reSerialized.IsValid);
        }
    }
}