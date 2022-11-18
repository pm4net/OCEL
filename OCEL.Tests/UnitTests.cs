namespace OCEL.Tests
{
    public class UnitTests
    {
        [Fact]
        public void SampleJsonIsValid()
        {
            var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
            var valid = OcelJson.Validate(json);
            Assert.True(valid);
        }

        [Fact]
        public void CanReadSampleJson()
        {
            var json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel");
            var ocelLog = OcelJson.Deserialize(json);
            Assert.True(ocelLog is not null);
        }
    }
}