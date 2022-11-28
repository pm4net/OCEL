namespace OCEL.Tests

open OCEL
open OCEL.Types

open System
open System.IO
open Xunit

module Xml =

    module ``Schema Validation`` =

        [<Fact>]
        let ``Sample XML is valid according to schema`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel")
            xml |> Xml.validate |> Assert.True

    module Deserialization =

        [<Fact>]
        let ``Can parse sample XML`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel")
            xml |> Xml.deserialize |> Assert.NotNull

        [<Fact>]
        let ``Parsed sample XML satisfies well-formedness property`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel")
            let parsed = Xml.deserialize xml
            parsed.IsValid |> Assert.True

        [<Fact>]
        let ``Can parse "GitHub pm4py" log`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.xmlocel")
            xml |> Xml.deserialize |> Assert.NotNull

        [<Fact>]
        let ``Parsed "GitHub pm4py" JSON satisfies well-formedness property`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\github_pm4py.xmlocel")
            let parsed = Xml.deserialize xml
            parsed.IsValid |> Assert.True

    module Serialization =

        [<Fact>]
        let ``Can serialize sample log`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel")
            xml 
            |> Xml.deserialize 
            |> Xml.serialize Formatting.Indented 
            |> String.IsNullOrWhiteSpace 
            |> Assert.False

        [<Fact>]
        let ``Can serialize sample log and deserialize it again`` () =
            let xml = File.ReadAllText(@"..\..\..\..\Samples\minimal.xmlocel")
            let log =
                xml
                |> Xml.deserialize
                |> Xml.serialize Formatting.Indented
                |> Xml.deserialize
            log.IsValid |> Assert.True