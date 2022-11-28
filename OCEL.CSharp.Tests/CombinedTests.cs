using System.IO;
using OCEL.Types;
using Xunit;

namespace OCEL.CSharp.Tests
{
    public static class CombinedTests
    {
        [Fact]
        public static void CanConvertSampleOcelJsonToOcelXml()
        {
            var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
            var log = Json.Deserialize(json);
            var xml = Xml.Serialize(log, Formatting.Indented);
            Assert.True(Xml.Validate(xml));
        }

        [Fact]
        public static void CanConvertSampleXmlJsonToOcelJson()
        {
            var xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel");
            var log = Xml.Deserialize(xml);
            var json = Json.Serialize(log, Formatting.Indented);
            Assert.True(Json.Validate(json));
        }
    }
}
