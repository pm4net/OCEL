namespace OCEL.Tests

open OCEL

open System
open System.IO
open Xunit

module Json =

    [<Fact>]
    let ``Sample JSON is valid`` () =
        let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
        Json.Validate json |> Assert.True

    [<Fact>]
    let ``Can parse sample JSON`` () =
        let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
        Json.Deserialize json

    [<Fact>]
    let ``Parsed sample JSON is valid`` () =
        let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
        let parsed = Json.Deserialize json
        parsed.IsValid

    [<Fact>]
    let ``Can parse GitHub pm4py log`` () =
        let json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel")
        let parsed = Json.Deserialize json
        parsed.IsValid
