# OCEL

[![GitHub](https://img.shields.io/github/license/pm4net/OCEL?style=flat-square)](https://github.com/pm4net/OCEL/blob/master/LICENSE)
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/pm4net/OCEL/Build%20and%20Run%20Tests?label=tests&style=flat-square)](https://github.com/pm4net/OCEL/actions/workflows/tests.yml)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/OCEL?label=OCEL&style=flat-square)](https://www.nuget.org/packages/OCEL/)
[![Nuget](https://img.shields.io/nuget/dt/OCEL?label=NuGet%20Downloads&style=flat-square)](https://www.nuget.org/packages/OCEL/#versions-body-tab)

**Object-Centric Event Log (OCEL)** is a standard interchange format for object-centric event data with multiple case notions [1]. This library aims to implement this standard in .NET with a high degree of type safety.

# Supported formats

The OCEL standard is defined for both JSON and XML. Both include a validation schema that is used by the library to validate input.

An additional useful format is to store OCEL data in document databases such as MongoDB [2]. A very good alternative for .NET is [LiteDB](https://www.litedb.org/), which is an embedded NoSQL database that is similar to MongoDB. It allows writing to files directly and does not require a database server to use. Support for MongoDB will be evaluated in the future.

| Format        | Status        |
| ------------- |:-------------:|
| JSON          | Implemented   |
| XML           | Implemented   |
| LiteDB        | Implemented   |
| MongoDB       | TBD           |

# Libraries

The library is written in F# and can be used directly by all .NET languages. For idiomatic usage in C#, a wrapper library is provided that mainly converts between appropriate types for the parsed logs.

Both libraries are available on NuGet:

| Library       | NuGet Link |
| ------------- | ------------- |
| F#            | https://www.nuget.org/packages/OCEL |
| C#            | https://www.nuget.org/packages/OCEL.CSharp |

# Examples

## Validating an OCEL file

```fsharp
let json = File.ReadAllText("minimal.jsonocel")
let valid = OCEL.OcelJson.validate json
let valid, errors = OCEL.OcelJson.validateWithErrorMessages json
```

## Parsing a JSON-OCEL and XML-OCEL string

```fsharp
let json = File.ReadAllText("minimal.jsonocel")
let xml = File.ReadAllText("minimal.xmlocel")
let fromJson = OCEL.OcelJson.deserialize json
let fromXml = OCEL.OcelXml.deserialize xml
```

## Writing and reading from a LiteDB database

```fsharp
let log = File.ReadAllText("minimal.jsonocel") |> OCEL.OcelJson.deserialize
let db = new LiteDatabase(":memory:")
OCEL.OcelLiteDB.serialize db log
let serializedLog = OCEL.OcelLiteDB.deserialize db
```

## Converting between formats

```fsharp
let json = File.ReadAllText("minimal.jsonocel")
let parsed = OCEL.OcelJson.deserialize json
let xml = OCEL.OcelXml.serialize parsed OCEL.Types.Formatting.Indented
```

# References

[1] Farhang, A., Park, G. G., Berti, A., & Aalst, W. Van Der. (2020). OCEL Standard. http://ocel-standard.org/

[2] Berti, A., Ghahfarokhi, A. F., Park, G., & van der Aalst, W. M. P. (2021). A Scalable Database for the Storage of Object-Centric Event Logs. CEUR Workshop Proceedings, 3098, 19–20. https://arxiv.org/abs/2202.05639.