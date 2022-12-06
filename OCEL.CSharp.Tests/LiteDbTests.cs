using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class LiteDbTests
    {
        private readonly ITestOutputHelper _output;

        public LiteDbTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanSerializeAndDeserializeBasicLogInMemory()
        {
            var log = new OcelLog(
                new Dictionary<string, OcelValue>
                {
                    { "version", new OcelString("1.0") },
                    { "ordering", new OcelString("timestamp") }
                },
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Add to cart", DateTimeOffset.Now, new List<string> { "item1" }, new Dictionary<string, OcelValue>()) }
                }, 
                new Dictionary<string, OcelObject>
                {
                    { "item1", new OcelObject("Item", new Dictionary<string, OcelValue> 
                        {
                            { "Cheese", new OcelInteger(12) }

                        })
                    }
                });

            var db = new LiteDatabase(":memory:");
            LiteDB.Serialize(db, log);
            var deserializedLog = LiteDB.Deserialize(db);
            Assert.True(deserializedLog.IsValid);
        }
    }
}
