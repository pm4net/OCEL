namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit
open Xunit.Abstractions

type JsonTests(output: ITestOutputHelper) =

    [<Fact>]
    member _.``Sample JSON is valid according to schema`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        json |> OcelJson.validate |> Assert.True

    [<Fact>]
    member _.``"GitHub pm4py" JSON is valid according to schema`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        json |> OcelJson.validate |> Assert.True

    [<Fact>]
    member _.``Can parse sample JSON`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        json |> OcelJson.deserialize true |> Assert.NotNull

    [<Fact>]
    member _.``Parsed sample JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = OcelJson.deserialize true json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can parse "GitHub pm4py" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        json |> OcelJson.deserialize true |> Assert.NotNull

    [<Fact>]
    member _.``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = OcelJson.deserialize true json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can parse nested JSON`` () =
        let json = File.ReadAllText("minimal_nested.jsonocel")
        let parsed = OcelJson.deserialize true json
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can serialize nested JSON`` () =
        let json = File.ReadAllText("minimal_nested.jsonocel")
        let parsed = OcelJson.deserialize true json
        let serialized = OcelJson.serialize Formatting.Indented parsed
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize sample log`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let parsed = json |> OcelJson.deserialize true
        let serialized = parsed |> OcelJson.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize "GitHub pm4pmy" log`` () =
        let json = File.ReadAllText("github_pm4py.jsonocel")
        let parsed = json |> OcelJson.deserialize true
        let serialized = parsed |> OcelJson.serialize Formatting.Indented
        output.WriteLine $"Serialized JSON:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize sample log and deserialize it again`` () =
        let json = File.ReadAllText("minimal.jsonocel")
        let log = json |> OcelJson.deserialize true
        let logReserialized = log |> OcelJson.serialize Formatting.Indented |> OcelJson.deserialize true
        log = logReserialized |> Assert.True
