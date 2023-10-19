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
        private readonly string _dataPath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "..", "..", "data", "OCEL"));

        public JsonTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SampleJsonIsValidAccordingToSchema()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            Assert.True(OcelJson.Validate(json));
        }

        [Fact]
        public void GitHubPm4PyJsonIsValidAccordingToSchema()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "github_pm4py.jsonocel"));
            Assert.True(OcelJson.Validate(json));
        }

        [Fact]
        public void CanParseSampleJson()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedSampleJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseGitHubPm4PyLog()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "github_pm4py.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            Assert.NotNull(parsed);
        }

        [Fact]
        public void ParsedGitHubPm4PyJsonSatisfiedWellFormednessProperty()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "github_pm4py.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanParseNestedJson()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal_nested.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            Assert.True(parsed.IsValid);
        }

        [Fact]
        public void CanSerializeNestedJson()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal_nested.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            var serialized = OcelJson.Serialize(parsed, Formatting.Indented, true);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLog()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented, true);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeGitHubPm4PyLog()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "github_pm4py.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented, true);
            _output.WriteLine($"Serialized JSON:{Environment.NewLine}{serialized}");
            Assert.False(string.IsNullOrWhiteSpace(serialized));
        }

        [Fact]
        public void CanSerializeSampleLogAndDeserializeItAgain()
        {
            var json = File.ReadAllText(Path.Combine(_dataPath, "minimal.jsonocel"));
            var parsed = OcelJson.Deserialize(json, true);
            var serialized = OcelJson.Serialize(parsed, Types.Formatting.Indented, true);
            var reSerialized = OcelJson.Deserialize(serialized, true);
            Assert.True(reSerialized.IsValid);
        }
    }
}