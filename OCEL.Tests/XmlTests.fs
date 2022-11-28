namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit
open Xunit.Abstractions

type XmlTests(output: ITestOutputHelper) =

    [<Fact>]
    member __.``Sample XML is valid according to schema`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        xml |> Xml.validate |> Assert.True

    [<Fact>]
    member __.``Can parse sample XML`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        xml |> Xml.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed sample XML satisfies well-formedness property`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let parsed = Xml.deserialize xml
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can parse "GitHub pm4py" log`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        xml |> Xml.deserialize |> Assert.NotNull

    [<Fact>]
    member __.``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        let parsed = Xml.deserialize xml
        parsed.IsValid |> Assert.True

    [<Fact>]
    member __.``Can serialize sample log`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let parsed = xml |> Xml.deserialize
        let serialized = parsed |> Xml.serialize Formatting.Indented
        output.WriteLine $"Serialized XML:{Environment.NewLine}{xml}"
        serialized |> Xml.validate |> Assert.True

    [<Fact>]
    member __.``Can serialize "GitHub pm4pmy" log`` () =
        let xml = File.ReadAllText("github_pm4py.xmlocel")
        let parsed = xml |> Xml.deserialize
        let serialized = parsed |> Xml.serialize Formatting.Indented
        output.WriteLine $"Serialized XML:{Environment.NewLine}{xml}"
        serialized |> Xml.validate |> Assert.True

    [<Fact>]
    member __.``Can serialize sample log and deserialize it again`` () =
        let xml = File.ReadAllText("minimal.xmlocel")
        let log =
            xml
            |> Xml.deserialize
            |> Xml.serialize Formatting.Indented
            |> Xml.deserialize
        log.IsValid |> Assert.True
