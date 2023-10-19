using System;
using System.Collections.Generic;
using System.IO;
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

        private static readonly OcelLog Log = new(
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

        private static (
            ILiteCollection<Tuple<string, OcelEvent>> e, 
            ILiteCollection<Tuple<string, OcelObject>> o, 
            ILiteCollection<Tuple<string, OcelValue>> a) 
            GetCollections(ILiteDatabase db)
        {
            var events = db.GetCollection<Tuple<string, OcelEvent>>("events");
            var objects = db.GetCollection<Tuple<string, OcelObject>>("objects");
            var attrs = db.GetCollection<Tuple<string, OcelValue>>("global_attributes");
            return (events, objects, attrs);
        }

        private static void TestCorrectNumberOfElements(ILiteDatabase db, OcelLog log)
        {
            var (e, o, a) = GetCollections(db);
            Assert.True(e.Count() == log.Events.Count);
            Assert.True(o.Count() == log.Objects.Count);
            Assert.True(a.Count() == log.GlobalAttributes.Count);
        }

        [Fact]
        public void CanDeserializeBasicLog()
        {
            var path = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "..", "..", "data", "OCEL", "minimal.db"));
            var db = new LiteDatabase($"Filename={path};ReadOnly=true");
            var log = OcelLiteDB.Deserialize(db);
            Assert.True(log.IsValid);
        }

        [Fact]
        public void CanSerializeBasicLog()
        {
            var db = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(db, Log, true);
            TestCorrectNumberOfElements(db, Log);
        }

        [Fact]
        public void CanSerializeAndDeserializeBasicLog()
        {
            var db = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(db, Log, true);
            var deserializedLog = OcelLiteDB.Deserialize(db);
            TestCorrectNumberOfElements(db, Log);
            Assert.True(deserializedLog.IsValid);
        }

        [Fact]
        public void CanDeserializeBasicLogMultipleTimesWithoutError()
        {
            var db = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(db, Log, true);
            OcelLiteDB.Serialize(db, Log, true);
            TestCorrectNumberOfElements(db, Log);
        }

        [Fact]
        public void CanSerializeBasicLogToDisk()
        {
            var db = new LiteDatabase(":temp:");
            OcelLiteDB.Serialize(db, Log, true);
            TestCorrectNumberOfElements(db, Log);
        }

        [Fact]
        public void SerializedBasicLogIsValid()
        {
            var db = new LiteDatabase(":memory:");
            OcelLiteDB.Serialize(db, Log, true);
            Assert.True(OcelLiteDB.Validate(db));
        }
    }
}
