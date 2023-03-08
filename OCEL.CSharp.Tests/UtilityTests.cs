using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OCEL.CSharp.Tests
{
    public class UtilityTests
    {
        private readonly ITestOutputHelper _output;

        public UtilityTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanCorrectlyMergeTwoLogs()
        {
            var log1 = new OcelLog(
                new Dictionary<string, OcelValue>
                {
                    { "ordering", new OcelString("timestamp") },
                },
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", DateTimeOffset.Now, new List<string> { "o1" }, new Dictionary<string, OcelValue>()) },
                    { "e2", new OcelEvent("Activity 2", DateTimeOffset.Now.AddDays(1), new List<string> { "o1" }, new Dictionary<string, OcelValue>()) }
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Object", new Dictionary<string, OcelValue>())}
                }
            );

            var log2 = new OcelLog(
                new Dictionary<string, OcelValue>
                {
                    { "ordering", new OcelString("timestamp") },
                    { "version", new OcelString("1.0") }
                },
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", DateTimeOffset.Now, new List<string> { "o1" }, new Dictionary<string, OcelValue>()) },
                    { "e3", new OcelEvent("Activity 2", DateTimeOffset.Now.AddDays(1), new List<string> { "o1" }, new Dictionary<string, OcelValue>()) }
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Object", new Dictionary<string, OcelValue>())}
                }
            );

            var merged = log1.MergeWith(log2);

            Assert.True(merged.GlobalAttributes.Count == 2);
            Assert.True(merged.GlobalAttributes.Keys.SequenceEqual(new List<string> { "ordering", "version" }));

            Assert.True(merged.Events.Count == 3);
            Assert.True(merged.Events.Keys.SequenceEqual(new List<string> { "e1", "e2", "e3" }));

            Assert.True(merged.Objects.Count == 1);
            Assert.True(merged.Objects.Keys.SequenceEqual(new List<string> { "o1" }));
        }

        [Fact]
        public void CanCorrectlyRemoveDuplicateObjects()
        {
            var timestamp = DateTimeOffset.Now;
            var log = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", timestamp, new List<string> { "o1" }, new Dictionary<string, OcelValue>()) },
                    { "e2", new OcelEvent("Activity 2", timestamp, new List<string> { "o1-duplicate" }, new Dictionary<string, OcelValue>()) }
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Object", new Dictionary<string, OcelValue> { { "test", new OcelString("some string") }, { "test2", new OcelInteger(123) } })},
                    { "o1-duplicate", new OcelObject("Object", new Dictionary<string, OcelValue> { { "test", new OcelString("some string") }, { "test2", new OcelInteger(123) } })},
                }
            );

            var correctLog = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", timestamp, new List<string> { "o1" }, new Dictionary<string, OcelValue>()) },
                    { "e2", new OcelEvent("Activity 2", timestamp, new List<string> { "o1" }, new Dictionary<string, OcelValue>()) }
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Object", new Dictionary<string, OcelValue> { { "test", new OcelString("some string") }, { "test2", new OcelInteger(123) } })}
                }
            );

            Assert.True(log.MergeDuplicateObjects() == correctLog);
        }

        [Fact]
        public void CanCorrectlyConvertObjectsToAttributes()
        {
            var timestamp = DateTimeOffset.Now;
            var log = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", timestamp, new List<string> { "o1", "o2" }, new Dictionary<string, OcelValue>()) },
                    { "e2", new OcelEvent("Activity 2", timestamp, new List<string> { "o2" }, new Dictionary<string, OcelValue>()) }
                },
                new Dictionary<string, OcelObject>
                {
                    { "o1", new OcelObject("Type 1", new Dictionary<string, OcelValue> { { "prop1", new OcelString("some string") }, { "prop2", new OcelInteger(123) } })},
                    { "o2", new OcelObject("Type 2", new Dictionary<string, OcelValue> { { "prop1", new OcelBoolean(false) } })},
                }
            );

            var correctLog = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", timestamp, new List<string>(), 
                        new Dictionary<string, OcelValue>
                        {
                            { "Type 1", new OcelMap(new Dictionary<string, OcelValue> {
                                { "prop1", new OcelString("some string") },
                                { "prop2", new OcelInteger(123) }
                            })},
                            { "Type 2", new OcelBoolean(false) }
                        })
                    },
                    { "e2", new OcelEvent("Activity 2", timestamp, new List<string>(), new Dictionary<string, OcelValue>
                    {
                        { "Type 2", new OcelBoolean(false) }
                    })}
                },
                new Dictionary<string, OcelObject>()
            );

            Assert.True(log.ConvertObjectsToAttributes(new List<string> { "Type 1", "Type 2" }) == correctLog);
        }

        [Fact]
        public void CanCorrectlyConvertAttributesToObjects()
        {
            var timestamp = DateTimeOffset.Now;
            var log = new OcelLog(
                new Dictionary<string, OcelValue>(),
                new Dictionary<string, OcelEvent>
                {
                    { "e1", new OcelEvent("Activity 1", timestamp, new List<string>(),
                        new Dictionary<string, OcelValue>
                        {
                            { "Type 1", new OcelMap(new Dictionary<string, OcelValue> {
                                { "prop1", new OcelString("some string") },
                                { "prop2", new OcelInteger(123) }
                            })},
                            { "Type 2", new OcelBoolean(false) }
                        })
                    },
                    { "e2", new OcelEvent("Activity 2", timestamp, new List<string>(), new Dictionary<string, OcelValue>
                    {
                        { "Type 2", new OcelBoolean(false) }
                    })}
                },
                new Dictionary<string, OcelObject>()
            );

            var result = log.ConvertAttributesToObjects(new List<string> { "Type 1", "Type 2" });
            Assert.All(result.Events, e =>
            {
                switch (e.Key)
                {
                    case "e1":
                        Assert.True(e.Value.OMap.Count() == 2 && e.Value.VMap.Count == 0);
                        return;
                    case "e2":
                        Assert.True(e.Value.OMap.Count() == 1 && e.Value.VMap.Count == 0);
                        return;
                    default:
                        throw new Exception("Unknown event");
                }
            });
            Assert.True(result.Objects.Count == 2);
        }
    }
}
