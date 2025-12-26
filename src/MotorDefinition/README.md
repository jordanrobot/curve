## JordanRobot.MotorDefinitions

.NET library for loading and saving motor definition JSON files.

The data model shape is:

- Motor -> Drive(s) -> Voltage(s) -> Performance Curve(s) -> Data Point(s)

### Install

This package is currently intended to be consumed from a local `.nupkg` produced via `dotnet pack`.

Once published to NuGet.org, you can install it via:

- the NuGet Package Manager in Visual Studio,
- by visiting the [NuGet.org page](https://www.nuget.org/packages/JordanRobot.MotorDefinitions).
- or by command line:
```bash
dotnet add package JordanRobot.MotorDefinitions
```


### Load

```csharp
using JordanRobot.MotorDefinitions;
using JordanRobot.MotorDefinitions.Model;

MotorDefinition motor = MotorFile.Load(@"C:\path\to\motor.json");
Console.WriteLine(motor.MotorName);
```

### Save

```csharp
using JordanRobot.MotorDefinitions;
using JordanRobot.MotorDefinitions.Model;

var motor = new MotorDefinition
{
	MotorName = "My Motor",
	Manufacturer = "Acme",
	PartNumber = "M-123"
};

MotorFile.Save(motor, @"C:\path\to\motor.json");
```

### Lightweight shape probe (quick filter)

If you need a fast pre-check (for example, filtering `*.json` files before doing a full load), you can probe a JSON document:

```csharp
using JordanRobot.MotorDefinitions;
using System.Text.Json;

using var document = JsonDocument.Parse(File.ReadAllText(@"C:\path\to\candidate.json"));
bool looksLikeMotorFile = MotorFile.IsLikelyMotorDefinition(document);
```

This is intentionally a lightweight shape check, not a full schema+semantic validator.

### Schema version

The library writes the schema version from `MotorDefinition.CurrentSchemaVersion` when saving.

### Further resources

- Documentation home: https://github.com/jordanrobot/curve/tree/main/docs
- Quick Start: https://github.com/jordanrobot/curve/blob/main/docs/QuickStart.md
- User Guide: https://github.com/jordanrobot/curve/blob/main/docs/UserGuide.md
- API documentation: https://github.com/jordanrobot/curve/blob/main/docs/api/index.md