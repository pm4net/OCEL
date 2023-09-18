namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit
open Xunit.Abstractions
open LiteDB

type CombinedTests(output: ITestOutputHelper) =

    [<Fact>]
    member _.``Can convert sample OCEL-JSON to OCEL-XML`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = OcelJson.deserialize true json
        let xml = OcelXml.serialize Formatting.Indented true log
        output.WriteLine $"Serialized XML:{Environment.NewLine}{xml}"
        OcelXml.validate xml |> Assert.True

    [<Fact>]
    member _.``Can convert sample nested OCEL-JSON to OCEL-XML`` () =
        let json = File.ReadAllText("minimal_nested.jsonocel")
        let log = OcelJson.deserialize true json
        let xml = OcelXml.serialize Formatting.Indented true log
        output.WriteLine $"Serialized XML:{Environment.NewLine}{xml}"
        OcelXml.validate xml |> Assert.True

    [<Fact>]
    member _.``Can convert sample OCEL-XML to OCEL-JSON`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = OcelXml.deserialize true xml
        let json = OcelJson.serialize Formatting.Indented true log
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{json}"
        OcelJson.validate json |> Assert.True

    [<Fact>]
    member _.``Can convert sample nested OCEL-XML to OCEL-JSON`` () =
        let xml = File.ReadAllText("minimal_nested.xmlocel")
        let log = OcelXml.deserialize true xml
        let json = OcelJson.serialize Formatting.Indented true log
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{json}"
        OcelJson.validate json |> Assert.True

    [<Fact>]
    member _.``Can convert sample OCEL-JSON to LiteDB`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = OcelJson.deserialize true json
        let liteDb = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize true liteDb log

    [<Fact>]
    member _.``Can convert sample OCEL-XML to LiteDB`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = OcelXml.deserialize true xml
        let liteDb = new LiteDatabase(":memory:")
        OCEL.OcelLiteDB.serialize true liteDb log