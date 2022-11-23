namespace OCEL.Tests

open OCEL

open System
open System.IO
open Xunit

#if !NETSTANDARD // Can not execute tests with .NET Standard

module Json =

    module ``Schema Validation`` =

        [<Fact>]
        let ``Sample JSON is valid according to schema`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.validate json |> Assert.True

    module Deserialization =

        [<Fact>]
        let ``Can parse sample JSON`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.deserialize json |> Assert.NotNull

        [<Fact>]
        let ``Parsed sample JSON satisfies well-formedness property`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            let parsed = Json.deserialize json
            parsed.IsValid |> Assert.True

        [<Fact>]
        let ``Can parse "GitHub pm4py" log`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel")
            Json.deserialize json |> Assert.NotNull

        [<Fact>]
        let ``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.jsonocel")
            let parsed = Json.deserialize json
            parsed.IsValid |> Assert.True

    module Serialization =

        [<Fact>]
        let ``Can serialize sample log`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            Json.deserialize json |> Json.serialize Newtonsoft.Json.Formatting.Indented |> Assert.NotNull

        [<Fact>]
        let ``Can serialize sample log and deserialize it again`` () =
            let json = File.ReadAllText(@"..\..\..\..\Samples\minimal.jsonocel")
            let log = 
                Json.deserialize json 
                |> Json.serialize Newtonsoft.Json.Formatting.Indented
                |> Json.deserialize
            log.IsValid |> Assert.True

#endif