using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class ConverterTests
    {
        private readonly ITestOutputHelper _output;

        public ConverterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanFilterOutNullValues()
        {
            var log = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", DateTimeOffset.Now, new List<string> { "o1" }, new Dictionary<string, OcelValue> 
                        {
                            { "prop1", null },
                            { "prop2", null }
                        })
                    },
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Object", new Dictionary<string, OcelValue> 
                        {
                            { "prop1", null },
                            { "prop2", null }
                        })
                    }
                }
            );

            var fsLog = log.ToFSharpOcelLog();
            Assert.True(fsLog.Events.All(e => e.Value.VMap.IsEmpty));
            Assert.True(fsLog.Objects.All(o => o.Value.OvMap.IsEmpty));
        }
    }
}
