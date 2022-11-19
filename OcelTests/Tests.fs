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
        Json.Deserialize json |> Assert.NotNull