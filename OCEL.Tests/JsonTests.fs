namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit
open Xunit.Abstractions

type JsonTests(output: ITestOutputHelper) =

    [<Fact>]
    member __.``Sample JSON is valid according to schema`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        json |> Json.validate |> Assert.True

    [<Fact>]
    member __.``Can parse sample JSON`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        json |> Json.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed sample JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = Json.deserialize json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can parse "GitHub pm4py" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        json |> Json.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = Json.deserialize json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can serialize sample log`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = json |> Json.deserialize
        let serialized = parsed |> Json.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        serialized |> Json.validate |> Assert.True

    [<Fact>]
    member __.``Can serialize "GitHub pm4pmy" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = json |> Json.deserialize
        let serialized = parsed |> Json.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        serialized |> Json.validate |> Assert.True

    [<Fact>]
    member __.``Can serialize sample log and deserialize it again`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = 
            json
            |> Json.deserialize 
            |> Json.serialize Formatting.Indented
            |> Json.deserialize
        log.IsValid |> Assert.True
