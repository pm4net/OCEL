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
        json |> OcelJson.validate |> Assert.True

    [<Fact>]
    member __.``"GitHub pm4py" JSON is valid according to schema`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        json |> OcelJson.validate |> Assert.True

    [<Fact>]
    member __.``Can parse sample JSON`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        json |> OcelJson.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed sample JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = OcelJson.deserialize json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can parse "GitHub pm4py" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        json |> OcelJson.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = OcelJson.deserialize json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can parse nested JSON`` () =
        let json = File.ReadAllText("minimal_nested.jsonocel")
        let parsed = OcelJson.deserialize json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can serialize nested JSON`` () =
        let json = File.ReadAllText("minimal_nested.jsonocel")
        let parsed = OcelJson.deserialize json
        let serialized = OcelJson.serialize Formatting.Indented parsed
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member __.``Can serialize sample log`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = json |> OcelJson.deserialize
        let serialized = parsed |> OcelJson.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member __.``Can serialize "GitHub pm4pmy" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = json |> OcelJson.deserialize
        let serialized = parsed |> OcelJson.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member __.``Can serialize sample log and deserialize it again`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = json |> OcelJson.deserialize 
        let logReserialized = log |> OcelJson.serialize Formatting.Indented |> OcelJson.deserialize
        log = logReserialized |> Assert.True
