﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OCEL.CSharp.Tests
{
#if !NETSTANDARD // Can not execute tests with .NET Standard

    public static class XmlTests
    {
        public class SchemaValidation
        {
            [Fact]
            public void SampleXmlIsValidAccordingToSchema()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
                Assert.True(Xml.Validate(xml));
            }
        }

        public class Deserialization
        {
            [Fact]
            public void CanParseSampleXml()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
                Assert.NotNull(Xml.Deserialize(xml));
            }

            [Fact]
            public void ParsedSampleXmlSatisfiedWellFormednessProperty()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                Assert.True(parsed.IsValid);
            }

            [Fact]
            public void CanParseGitHubPm4PyLog()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.xmlocel");
                Assert.NotNull(Xml.Deserialize(xml));
            }

            [Fact]
            public void ParsedGitHubPm4PyXmlSatisfiedWellFormednessProperty()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.xmlocel");
                var parsed = Xml.Deserialize(xml);
                Assert.True(parsed.IsValid);
            }
        }

        public class Serialization
        {
            [Fact]
            public void CanSerializeSampleLog()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                var serialized = Xml.Serialize(parsed, Types.Formatting.Indented);
                Assert.False(string.IsNullOrWhiteSpace(serialized));
            }

            [Fact]
            public void CanSerializeSampleLogAndDeserializeItAgain()
            {
                var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
                var parsed = Xml.Deserialize(xml);
                var serialized = Xml.Serialize(parsed, Types.Formatting.Indented);
                var reSerialized = Xml.Deserialize(serialized);
                Assert.True(reSerialized.IsValid);
            }
        }
    }

#endif
}