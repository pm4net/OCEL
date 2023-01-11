namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open Xunit
open Xunit.Abstractions

type UtilityTests(output: ITestOutputHelper) =

    [<Fact>]
    let ``Can correctly merge two logs`` () =
        let log1 = {
            GlobalAttributes = ["ordering", OcelString "timestamp"] |> Map.ofList
            Events = [
                "e1", {
                    Activity = "Activity 1"
                    Timestamp = DateTimeOffset.Now
                    OMap = ["o1"]
                    VMap = Map.empty 
                }
                "e2", {
                    Activity = "Activity 2"
                    Timestamp = DateTimeOffset.Now.AddDays 1
                    OMap = ["o1"]
                    VMap = Map.empty
                }
            ] |> Map.ofList
            Objects = [
                "o1", {
                    Type = "Object"
                    OvMap = Map.empty
                }
            ] |> Map.ofList
        }

        let log2 = {
            GlobalAttributes = ["ordering", OcelString "timestamp"; "version", OcelString "1.0"] |> Map.ofList
            Events = [
                "e1", {
                    Activity = "Activity 1"
                    Timestamp = DateTimeOffset.Now
                    OMap = ["o1"]
                    VMap = Map.empty 
                }
                "e3", {
                    Activity = "Activity 2"
                    Timestamp = DateTimeOffset.Now.AddDays 1
                    OMap = ["o1"]
                    VMap = Map.empty
                }
            ] |> Map.ofList
            Objects = [
                "o1", {
                    Type = "Object"
                    OvMap = Map.empty
                }
            ] |> Map.ofList
        }

        let merged = log1.MergeWith log2

        merged.GlobalAttributes.Count = 2 |> Assert.True
        merged.GlobalAttributes.Keys |> Seq.toList = ["ordering"; "version"] |> Assert.True

        merged.Events.Count = 3 |> Assert.True
        merged.Events.Keys |> Seq.toList = ["e1"; "e2"; "e3"] |> Assert.True

        merged.Objects.Count = 1 |> Assert.True
        merged.Objects.Keys |> Seq.toList = ["o1"] |> Assert.True

    [<Fact>]
    let ``Can correctly remove duplicate objects`` () =
        let timestamp = DateTimeOffset.Now

        let log = {
            GlobalAttributes = Map.empty
            Events = [
                "e1", {
                    Activity = "Activity 1"
                    Timestamp = timestamp
                    OMap = ["o1"]
                    VMap = Map.empty 
                }
                "e2", {
                    Activity = "Activity 2"
                    Timestamp = timestamp
                    OMap = ["o1-duplicate"]
                    VMap = Map.empty
                }
            ] |> Map.ofList
            Objects = [
                "o1", {
                    Type = "Object"
                    OvMap = ["test", OcelString "some string"; "test2", OcelInteger 123] |> Map.ofList
                }
                "o1-duplicate", {
                    Type = "Object"
                    OvMap = ["test", OcelString "some string"; "test2", OcelInteger 123] |> Map.ofList
                }
            ] |> Map.ofList
        }

        let correctLog = {
            GlobalAttributes = Map.empty
            Events = [
                "e1", {
                    Activity = "Activity 1"
                    Timestamp = timestamp
                    OMap = ["o1"]
                    VMap = Map.empty 
                }
                "e2", {
                    Activity = "Activity 2"
                    Timestamp = timestamp
                    OMap = ["o1"]
                    VMap = Map.empty
                }
            ] |> Map.ofList
            Objects = [
                "o1", {
                    Type = "Object"
                    OvMap = ["test", OcelString "some string"; "test2", OcelInteger 123] |> Map.ofList
                }
            ] |> Map.ofList
        }

        log.MergeDuplicateObjects() = correctLog |> Assert.True