namespace OCEL.Tests

open OCEL.Types

open LiteDB
open System
open Xunit
open Xunit.Abstractions

type LiteDbTests(output: ITestOutputHelper) =

    let getCollections (db: LiteDatabase) =
        let eventsColl = db.GetCollection<string * OcelEvent>("events")
        let objectsColl = db.GetCollection<string * OcelObject>("objects")
        let globalAttrsColl = db.GetCollection<string * OcelValue>("global_attributes")
        (eventsColl, objectsColl, globalAttrsColl)

    let testCorrectNumberOfElements (db: LiteDatabase) (log: OcelLog) =
        let (e, o, a) = getCollections db
        e.Count() = log.Events.Count |> Assert.True
        o.Count() = log.Objects.Count |> Assert.True
        a.Count() = log.GlobalAttributes.Count |> Assert.True

    let log = {
            GlobalAttributes = ["version", OcelString "1.0"; "ordering", OcelString "timestamp"] |> Map.ofSeq
            Events = 
                ["e1", {
                    Activity = "Add to cart"
                    Timestamp = DateTimeOffset.Now
                    OMap = ["item1"]
                    VMap = ["price", OcelFloat 13.37; "added on", OcelTimestamp DateTimeOffset.Now] |> Map.ofList
                }; "e2", {
                    Activity = "Submit order"
                    Timestamp = DateTimeOffset.Now.AddMinutes 1
                    OMap = []
                    VMap = Map.empty
                }]
                |> Map.ofList
            Objects =
                ["item1", {
                    Type = "Item"
                    OvMap = ["Cheese", OcelInteger 12] |> Map.ofList
                }]
                |> Map.ofList
        }

    [<Fact>]
    member _.``Can serialize basic log`` () =
        let db = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize db log
        testCorrectNumberOfElements db log

    [<Fact>]
    member _.``Can serialize and deserialize basic log`` () =
        let db = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize db log
        testCorrectNumberOfElements db log
        let deserializedLog = OCEL.OcelLiteDB.deserialize db
        deserializedLog.IsValid |> Assert.True
        log.Equals deserializedLog |> Assert.True

    [<Fact>]
    member _.``Can serialize basic log multiple times without error`` () =
        let db = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize db log
        OCEL.OcelLiteDB.serialize db log
        testCorrectNumberOfElements db log

    [<Fact>]
    member _.``Can serialize basic log to disk`` () =
        let db = new LiteDatabase(":temp:")
        OCEL.OcelLiteDB.serialize db log
        testCorrectNumberOfElements db log

    [<Fact>]
    member _.``Serialized basic log is valid`` () =
        let db = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize db log
        OCEL.OcelLiteDB.validate db |> Assert.True
