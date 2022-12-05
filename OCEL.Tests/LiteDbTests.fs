﻿namespace OCEL.Tests

open OCEL.Types

open LiteDB
open System
open Xunit
open Xunit.Abstractions

type LiteDbTests(output: ITestOutputHelper) =

    [<Fact>]
    member __.``Can serialize basic log`` () =
        let log = {
            GlobalAttributes = ["version", OcelString "1.0"; "ordering", OcelString "timestamp"] |> Map.ofSeq
            Events = 
                ["e1", {
                    Activity = "Add to cart"
                    Timestamp = DateTimeOffset.Now
                    OMap = ["item1"]
                    VMap = ["price", OcelFloat 13.37] |> Map.ofList
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
                    OvMap = Map.empty
                }]
                |> Map.ofList
        }
        let db = OCEL.LiteDB.serialize (new LiteDatabase(@"C:\Temp\MyData.db")) log
        true