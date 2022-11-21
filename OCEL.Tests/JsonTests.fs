namespace OCEL.Tests

open OCEL

open System
open System.IO
open Xunit

module Json =

    module ``Schema Validation`` =

        [<Fact>]
        let ``Sample JSON is valid according to schema`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.Validate json |> Assert.True

    module Deserialization =

        [<Fact>]
        let ``Can parse sample JSON`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.Deserialize json |> Assert.NotNull

        [<Fact>]
        let ``Parsed sample JSON satisfies well-formedness property`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            let parsed = Json.Deserialize json
            parsed.IsValid |> Assert.True

        [<Fact>]
        let ``Can parse "GitHub pm4py" log`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel")
            Json.Deserialize json |> Assert.NotNull

        [<Fact>]
        let ``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel")
            let parsed = Json.Deserialize json
            parsed.IsValid |> Assert.True

    module Serialization =

        [<Fact>]
        let ``Can serialize sample log`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.Deserialize json |> Json.Serialize Newtonsoft.Json.Formatting.Indented |> Assert.NotNull

        [<Fact>]
        let ``Can serialize sample log and deserialize it again`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            let log = 
                Json.Deserialize json 
                |> Json.Serialize Newtonsoft.Json.Formatting.Indented
                |> Json.Deserialize
            log.IsValid |> Assert.True
