namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit

module CombinedTests =

    [<Fact>]
    let ``Can convert sample OCEL-JSON to OCEL-XML`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = Json.deserialize json
        let xml = Xml.serialize Formatting.Indented log
        Xml.validate xml |> Assert.True

    [<Fact>]
    let ``Can convert sample OCEL-XML to OCEL-JSON`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = Xml.deserialize xml
        let json = Json.serialize Formatting.Indented log
        Json.validate json |> Assert.True