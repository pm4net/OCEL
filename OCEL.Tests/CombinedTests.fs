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
    member __.``Can convert sample OCEL-JSON to OCEL-XML`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = Json.deserialize json
        let xml = Xml.serialize Formatting.Indented log
        output.WriteLine $"Serialized XML:{Environment.NewLine}{xml}"
        Xml.validate xml |> Assert.True

    [<Fact>]
    member __.``Can convert sample OCEL-XML to OCEL-JSON`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = Xml.deserialize xml
        let json = Json.serialize Formatting.Indented log
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{json}"
        Json.validate json |> Assert.True

    [<Fact>]
    member __.``Can convert sample OCEL-JSON to LiteDB`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = Json.deserialize json
        let liteDb = new LiteDatabase(":memory:")
        OCEL.LiteDB.serialize liteDb log

    [<Fact>]
    member __.``Can convert sample OCEL-XML to LiteDB`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = Xml.deserialize xml
        let liteDb = new LiteDatabase(":memory:")
        OCEL.LiteDB.serialize liteDb log