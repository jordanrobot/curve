# Motor Torque Curve Editor - Requirements Summary

## Overview

A dedicated desktop application for editing motor torque curves stored in JSON files.

## Core Requirements

### Functional Requirements

1. **File Operations**
   - Load JSON files containing motor torque curve data
   - Save edited curves back to JSON files
   - Standard file editor behavior (Open, Save, Save As, New)

2. **Visualization**
   - Display torque curves as line graphs
   - Interactive graph manipulation
   - Real-time updates between graph and numeric values

3. **Data Editing**
   - View and edit curve properties
   - Edit numeric values directly
   - Manipulate graph points interactively

4. **EQ-Style Curve Editing**
   - Select points on the curve line and drag up/down to move them
   - Adjustable "Q" value (0.0-1.0) via slider to control curve sharpness
   - Q affects how gradual or sharp changes are to adjacent points
   - Similar interaction model to audio EQ applications

5. **Background Image Overlay**
   - Load an image behind the graph (for curve verification)
   - Scale image independently on X and Y axes
   - Purpose: overlay manufacturer motor curve images for verification

6. **Axis Scaling**
   - X and Y axes scalable via sliders
   - Smooth graph recalculation without jitter when scaling
   - Allows matching curve scale to background image

7. **Graph Grid & Labels**
   - Axis labels at rounded whole number increments
     - RPM: 500, 1000, 1500, 2000, etc.
     - Torque: 5, 10, 15, 20, etc.
   - Faded grid lines extending across graph at label positions
   - Auto-select label increments to avoid crowding

8. **Axis Configuration**
   - Default: RPM = X axis, Torque = Y axis
   - Future: Toggle to swap axes

9. **Hover Information**
   - Show RPM and torque values in popup when hovering near curve
   - Popup positioned to not obscure cursor
   - User preference to enable/disable hover popup

10. **Units System (Future)**
   - Toggle between Nm (Newton-meters) and lbf-in (pound-force inches)
   - Automatic value conversion when switching units
   - Use the `Tare` NuGet package (by jordanrobot) for unit handling

### Non-Functional Requirements

1. **Deployment**
   - Portable application (no installation required)
   - Run from user space
   - No server infrastructure or cloud resources
   - Self-contained executable

2. **Platform**
   - Primary target: Windows 11
   - Nice to have: Cross-platform support

3. **Technology Preferences**
   - C# and .NET 8 preferred
   - Open to other solutions if justified

## Data Model (Assumed)

Based on typical motor torque curve data:

```json
{
  "name": "Motor Model XYZ",
  "manufacturer": "Company Name",
  "unit": "Nm",
  "data": [
    { "rpm": 0, "torque": 50.0 },
    { "rpm": 1000, "torque": 48.5 },
    { "rpm": 2000, "torque": 45.0 },
    { "rpm": 3000, "torque": 40.0 },
    { "rpm": 4000, "torque": 33.0 },
    { "rpm": 5000, "torque": 25.0 }
  ],
  "metadata": {
    "created": "2024-01-15",
    "notes": "Test conditions: 25°C ambient"
  }
}
```

## User Workflow

1. Launch application (no installation)
2. Open existing JSON file or create new curve
3. Optionally load background image for reference
4. Scale background image to match graph dimensions
5. View torque curve visualization
6. Edit curve via:
   - Dragging points up/down (EQ-style)
   - Adjusting Q slider for curve sharpness
   - Direct numeric input in properties panel
7. Hover over curve to see precise values
8. Scale axes as needed to match reference image
9. Toggle units (future feature)
10. Save changes to file

## Success Criteria

- [ ] Can load and parse JSON torque curve files
- [ ] Displays interactive line graph
- [ ] Supports direct numeric editing
- [ ] Supports graph-based editing (EQ-style drag)
- [ ] Q slider affects curve sharpness (0.0-1.0)
- [ ] Can load background image for verification
- [ ] Background image scales independently on X/Y
- [ ] Axes scalable via sliders (smooth, no jitter)
- [ ] Grid lines at rounded value increments
- [ ] Hover popup shows RPM/torque values
- [ ] Hover popup can be disabled in settings
- [ ] RPM on X axis, Torque on Y axis by default
- [ ] Saves valid JSON output
- [ ] Runs without installation
- [ ] Works on Windows 11
