namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit
open Xunit.Abstractions

type XmlTests(output: ITestOutputHelper) =

    [<Fact>]
    member _.``Sample XML is valid according to schema`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        xml |> OcelXml.validate |> Assert.True

    [<Fact>]
    member _.``"GitHub pm4py" XML is valid according to schema`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        xml |> OcelXml.validate |> Assert.True

    [<Fact>]
    member _.``Can parse sample XML`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        xml |> OcelXml.deserialize |> Assert.NotNull

    [<Fact>]
    member _.``Parsed sample XML satisfies well-formedness property`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let parsed = OcelXml.deserialize xml
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can parse "GitHub pm4py" log`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        xml |> OcelXml.deserialize |> Assert.NotNull

    [<Fact>]
    member _.``Parsed "GitHub pm4py" XML satisfies well-formedness property`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        let parsed = OcelXml.deserialize xml
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can parse nested XML`` () =
        let xml = File.ReadAllText("minimal_nested.xmlocel")
        let parsed = OcelXml.deserialize xml
        parsed.IsValid |> Assert.True

    [<Fact>]
    member _.``Can serialize nested XML`` () =
        let xml = File.ReadAllText("minimal_nested.xmlocel")
        let parsed = OcelXml.deserialize xml
        let serialized = OcelXml.serialize Formatting.Indented parsed
        output.WriteLine $"Serialized XML:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize sample log`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let parsed = xml |> OcelXml.deserialize
        let serialized = parsed |> OcelXml.serialize Formatting.Indented
        output.WriteLine $"Serialized XML:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize "GitHub pm4pmy" log`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        let parsed = xml |> OcelXml.deserialize
        let serialized = parsed |> OcelXml.serialize Formatting.Indented
        output.WriteLine $"Serialized XML:{Environment.NewLine}{serialized}"
        String.IsNullOrWhiteSpace serialized |> Assert.False

    [<Fact>]
    member _.``Can serialize sample log and deserialize it again`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log = xml |> OcelXml.deserialize
        let logReserialized = log |> OcelXml.serialize Formatting.Indented |> OcelXml.deserialize
        log = logReserialized |> Assert.True
