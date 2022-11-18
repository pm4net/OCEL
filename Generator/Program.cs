using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

var schema = await JsonSchema.FromFileAsync(@"..\..\..\..\Schemas\schema.json");
var generator = new CSharpGenerator(schema);
var file = generator.GenerateFile();
await File.WriteAllTextAsync("OcelLog.cs", file);