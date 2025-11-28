# Motor Torque Curve Editor

A desktop application for creating and editing motor torque curves stored in JSON files.

## Features

### Phase 1 (Current - Foundation)
- ✅ Create new motor definition files
- ✅ Load and save motor curve data in JSON format
- ✅ Data models for motor definitions, curve series, and data points
- ✅ Curve generation from motor parameters (max RPM, torque, power)
- ✅ 1% increment data storage (101 points per curve)
- ✅ Basic UI shell with menu bar
- ✅ Structured logging with Serilog
- ✅ Comprehensive unit tests

### Future Phases
- Phase 2: Interactive chart with LiveCharts2
- Phase 3: Directory browser and save prompts
- Phase 4: EQ-style curve editing with Q slider
- Phase 5: User preferences and settings
- Phase 6: Units system (Nm ↔ lbf-in)
- Phase 7: Tabbed interface for multiple files
- Phase 8: Power curve overlay

## Getting Started

### Prerequisites
- .NET 8 SDK

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/CurveEditor
```

### Test

```bash
dotnet test
```

### Publish (Single Executable)

```bash
# Windows
dotnet publish src/CurveEditor -c Release -r win-x64 --self-contained

# Linux
dotnet publish src/CurveEditor -c Release -r linux-x64 --self-contained
```

## Project Structure

```
CurveEditor/
├── src/
│   └── CurveEditor/
│       ├── Models/           # Data models (MotorDefinition, CurveSeries, DataPoint)
│       ├── ViewModels/       # MVVM view models
│       ├── Views/            # Avalonia UI views
│       ├── Services/         # File and curve generation services
│       └── Assets/           # Application resources
├── tests/
│   └── CurveEditor.Tests/    # Unit tests
└── samples/
    └── example-motor.json    # Sample motor definition file
```

## Data Format

Motor definitions are stored as JSON files with the following structure:

```json
{
  "schemaVersion": "1.0",
  "motorName": "High Torque Servo Motor",
  "manufacturer": "Acme Motors",
  "maxRpm": 5000,
  "ratedPeakTorque": 55.0,
  "series": [
    {
      "name": "Peak",
      "data": [
        { "percent": 0, "rpm": 0, "torque": 55.0 },
        { "percent": 1, "rpm": 50, "torque": 55.0 },
        ...
        { "percent": 100, "rpm": 5000, "torque": 33.01 }
      ]
    }
  ]
}
```

Each curve series contains 101 data points at 1% increments from 0% to 100% of max speed.

## Technology Stack

- **Framework**: .NET 8
- **UI**: Avalonia UI 11.x
- **Charting**: LiveCharts2 (future phases)
- **MVVM**: CommunityToolkit.Mvvm
- **Logging**: Serilog

## License

MIT License - see [LICENSE](LICENSE) for details.
