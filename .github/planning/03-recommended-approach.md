# Motor Torque Curve Editor - Recommended Approach

## Primary Recommendation: Avalonia UI with LiveCharts2

After analyzing the requirements against the available options, **Avalonia UI** is the recommended framework for the motor torque curve editor.

### Why Avalonia?

1. **Perfect Fit for Requirements**
   - ✅ C# and .NET 8 native
   - ✅ Single-file portable executable
   - ✅ No installation required
   - ✅ Direct NuGet package support (Tare)
   - ✅ Cross-platform with Linux support
   - ✅ Excellent charting with LiveCharts2

2. **Lightweight and Fast**
   - Smaller bundle size than MAUI or Electron
   - Fast startup time
   - Lower memory footprint

3. **Future-Proof**
   - Can expand to macOS and Linux later
   - Active development and growing community
   - Modern MVVM architecture

4. **Developer Experience**
   - XAML-based (familiar to WPF/MAUI developers)
   - Hot reload support
   - Good IDE support (Visual Studio, Rider)

---

## Alternative Recommendation: WPF with LiveCharts2

If cross-platform is not important and you want the most stable, proven solution:

**WPF** is a solid choice for Windows-only development with:
- Mature and battle-tested
- Smallest learning curve for C# developers
- Excellent tooling support

---

## Recommended Architecture

### MVVM Pattern

```
┌─────────────────────────────────────────────────────────────────┐
│                           View Layer                             │
│  ┌─────────────┐  ┌─────────────────┐  ┌──────────────────────┐ │
│  │  MainWindow │  │   ChartView     │  │   PropertiesPanel    │ │
│  │  (Shell)    │  │   (LiveCharts2) │  │   (DataGrid/Form)    │ │
│  └─────────────┘  └─────────────────┘  └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        ViewModel Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ MainViewModel   │  │ CurveViewModel  │  │ PointViewModel  │  │
│  │ - File commands │  │ - Chart data    │  │ - RPM           │  │
│  │ - Navigation    │  │ - Edit commands │  │ - Torque        │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Model Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  TorqueCurve    │  │  DataPoint      │  │  CurveMetadata  │  │
│  │  - Name         │  │  - RPM          │  │  - Created      │  │
│  │  - Data[]       │  │  - Torque       │  │  - Notes        │  │
│  │  - Unit         │  │                 │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Services Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  FileService    │  │  UnitService    │  │  ValidationSvc  │  │
│  │  - Load JSON    │  │  - Convert Nm   │  │  - Validate     │  │
│  │  - Save JSON    │  │  - Convert lbf  │  │  - Check limits │  │
│  │  - File dialogs │  │  - (uses Tare)  │  │                 │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Project Structure

```
CurveEditor/
├── CurveEditor.sln
├── src/
│   └── CurveEditor/
│       ├── CurveEditor.csproj
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── Models/
│       │   ├── TorqueCurve.cs
│       │   ├── DataPoint.cs
│       │   └── CurveMetadata.cs
│       ├── ViewModels/
│       │   ├── ViewModelBase.cs
│       │   ├── MainWindowViewModel.cs
│       │   ├── CurveViewModel.cs
│       │   └── PointViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.axaml
│       │   ├── MainWindow.axaml.cs
│       │   ├── ChartView.axaml
│       │   └── PropertiesPanel.axaml
│       ├── Services/
│       │   ├── IFileService.cs
│       │   ├── FileService.cs
│       │   ├── IUnitService.cs
│       │   └── UnitService.cs
│       └── Assets/
│           └── app-icon.ico
├── tests/
│   └── CurveEditor.Tests/
│       ├── CurveEditor.Tests.csproj
│       ├── Models/
│       │   └── TorqueCurveTests.cs
│       └── Services/
│           └── FileServiceTests.cs
└── samples/
    └── example-curve.json
```

---

## Technology Stack

### Core Framework
- **.NET 8** - Long-term support, latest features
- **Avalonia UI 11.x** - Cross-platform UI framework
- **CommunityToolkit.Mvvm** - MVVM helpers and source generators

### Charting
- **LiveCharts2** - Best interactive charting for Avalonia
  - Supports drag-to-edit points
  - Smooth animations
  - Good documentation

### Additional UI Components
- **SkiaSharp** - For background image rendering and scaling
- Custom slider controls for Q value and axis scaling

### Dependencies (NuGet Packages)
```xml
<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.1.*" />
  <PackageReference Include="Avalonia.Desktop" Version="11.1.*" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.1.*" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.*" />
  <PackageReference Include="LiveChartsCore.SkiaSharpView.Avalonia" Version="2.0.*" />
  <PackageReference Include="System.Text.Json" Version="8.0.*" />
  <!-- Future: Add Tare for units -->
  <!-- <PackageReference Include="Tare" Version="x.x.x" /> -->
</ItemGroup>
```

---

## Key Features Implementation

### 1. EQ-Style Interactive Chart Editing

LiveCharts2 supports interactive point manipulation with drag behavior:

```csharp
// Example: Draggable points on chart with Q-based smoothing
public partial class CurveViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _qValue = 0.5; // Range 0.0 to 1.0
    
    public ISeries[] Series => new ISeries[]
    {
        new LineSeries<ObservablePoint>
        {
            Values = CurvePoints,
            Fill = null,
            GeometrySize = 12,
            LineSmoothness = QValue, // Q affects smoothness
            DataPointerDown = OnPointClicked,
        }
    };
    
    // Apply Q-based influence to adjacent points when dragging
    private void ApplyQInfluence(int pointIndex, double deltaY)
    {
        // Lower Q = sharper changes (affects fewer neighbors)
        // Higher Q = gradual changes (affects more neighbors)
        int influence = (int)(QValue * 5); // 0-5 adjacent points
        for (int i = 1; i <= influence; i++)
        {
            double factor = 1.0 - (i / (influence + 1.0));
            // Apply scaled delta to neighboring points
        }
    }
}
```

### 2. Background Image Overlay

Load and scale reference images behind the chart:

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _backgroundImagePath;
    
    [ObservableProperty]
    private double _imageScaleX = 1.0;
    
    [ObservableProperty]
    private double _imageScaleY = 1.0;
    
    public async Task LoadBackgroundImageAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load Background Image",
            Filters = new List<FileDialogFilter>
            {
                new() { Name = "Images", Extensions = { "png", "jpg", "jpeg", "bmp" } }
            }
        };
        
        var result = await dialog.ShowAsync(_window);
        if (result?.Length > 0)
        {
            BackgroundImagePath = result[0];
        }
    }
}
```

### 3. Axis Scaling with Sliders

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private double _xAxisMin = 0;
    
    [ObservableProperty]
    private double _xAxisMax = 5000;
    
    [ObservableProperty]
    private double _yAxisMin = 0;
    
    [ObservableProperty]
    private double _yAxisMax = 100;
    
    public Axis[] XAxes => new Axis[]
    {
        new Axis
        {
            Name = "RPM",
            MinLimit = XAxisMin,
            MaxLimit = XAxisMax,
            // Auto-calculate nice step values
            MinStep = CalculateNiceStep(XAxisMax - XAxisMin),
            LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
            SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
        }
    };
    
    private double CalculateNiceStep(double range)
    {
        // Return nice round numbers: 100, 250, 500, 1000, etc.
        double rough = range / 10;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        double normalized = rough / magnitude;
        
        if (normalized < 2) return magnitude;
        if (normalized < 5) return 2 * magnitude;
        return 5 * magnitude;
    }
}
```

### 4. Grid Lines and Labels

LiveCharts2 automatically creates grid lines with the separator paint:

```csharp
public Axis[] YAxes => new Axis[]
{
    new Axis
    {
        Name = "Torque (Nm)",
        MinLimit = YAxisMin,
        MaxLimit = YAxisMax,
        MinStep = CalculateNiceStep(YAxisMax - YAxisMin),
        // Faded grid lines
        SeparatorsPaint = new SolidColorPaint(
            new SKColor(200, 200, 200, 80) // Light gray with transparency
        ),
        // Axis labels
        LabelsPaint = new SolidColorPaint(SKColors.DarkGray),
        LabelsRotation = 0
    }
};
```

### 5. Hover Tooltip

```csharp
public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _showHoverTooltip = true; // User preference
    
    public SolidColorPaint? TooltipBackgroundPaint => ShowHoverTooltip 
        ? new SolidColorPaint(new SKColor(240, 240, 240, 220))
        : null;
    
    public Func<ChartPoint, string> TooltipFormatter => point =>
        $"RPM: {point.SecondaryValue:N0}\nTorque: {point.PrimaryValue:N1} Nm";
}
```

XAML for tooltip positioning:
```xml
<lvc:CartesianChart 
    Series="{Binding Series}"
    TooltipPosition="Top"
    TooltipBackgroundPaint="{Binding TooltipBackgroundPaint}"
    TooltipTextPaint="{Binding TooltipTextPaint}">
</lvc:CartesianChart>
```

### 6. File Operations

Standard file dialog integration:

```csharp
public async Task OpenFileAsync()
{
    var dialog = new OpenFileDialog
    {
        Title = "Open Torque Curve",
        Filters = new List<FileDialogFilter>
        {
            new() { Name = "JSON Files", Extensions = { "json" } },
            new() { Name = "All Files", Extensions = { "*" } }
        }
    };
    
    var result = await dialog.ShowAsync(_window);
    if (result?.Length > 0)
    {
        await LoadCurveAsync(result[0]);
    }
}
```

### 7. Unit Conversion (Future)

Prepared for Tare integration:

```csharp
public interface IUnitService
{
    double ConvertTorque(double value, TorqueUnit from, TorqueUnit to);
    TorqueUnit CurrentUnit { get; set; }
}

// Future implementation with Tare:
public class UnitService : IUnitService
{
    public double ConvertTorque(double value, TorqueUnit from, TorqueUnit to)
    {
        // Using Tare package for conversions
        // return Tare.Torque.Convert(value, from, to);
        throw new NotImplementedException("Implement with Tare package");
    }
}
```

---

## Deployment

### Publish as Single File

```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true

# Optional: Trim unused code for smaller size
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true
```

### Expected Output Size
- Self-contained single file: ~50-70 MB
- Trimmed version: ~30-50 MB

---

## Getting Started

See [04-mvp-roadmap.md](./04-mvp-roadmap.md) for the implementation roadmap.
