namespace OCEL.Tests

open OCEL.Types

open LiteDB
open System
open Xunit
open Xunit.Abstractions

type LiteDbTests(output: ITestOutputHelper) =

    [<Fact>]
    member _.``Can serialize and deserialize basic log`` () =
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

        let db = new LiteDatabase(":memory:")
        OCEL.LiteDB.serialize db log
        let deserializedLog = OCEL.LiteDB.deserialize db
        deserializedLog.IsValid |> Assert.True
        log.Equals deserializedLog |> Assert.True