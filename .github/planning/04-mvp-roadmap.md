# Motor Torque Curve Editor - MVP Roadmap

## Phase Overview

```
Phase 1: Foundation (Week 1-2)
    └── Project setup, basic UI shell, file operations

Phase 2: Core Features (Week 3-4)
    └── Chart visualization, data binding, grid lines, hover tooltip

Phase 3: Advanced Editing (Week 5-7)
    └── EQ-style editing, Q slider, background images, axis scaling

Phase 4: Polish & User Preferences (Week 8)
    └── User settings, keyboard shortcuts, undo/redo

Phase 5: Units System (Future)
    └── Tare integration, unit conversion, preferences
```

---

## Phase 1: Foundation

### 1.1 Project Setup
- [ ] Create solution with Avalonia template
- [ ] Configure project for .NET 8
- [ ] Add NuGet packages (Avalonia, CommunityToolkit.Mvvm)
- [ ] Set up project structure (Models, Views, ViewModels, Services)
- [ ] Configure single-file publishing

### 1.2 Basic Window Shell
- [ ] Create MainWindow with menu bar
- [ ] Implement basic layout (chart area + properties panel)
- [ ] Add File menu (New, Open, Save, Save As, Exit)
- [ ] Implement window title showing current file

### 1.3 Data Models
- [ ] Create TorqueCurve model class
- [ ] Create DataPoint model class
- [ ] Create CurveMetadata model class
- [ ] Add JSON serialization attributes
- [ ] Write model unit tests

### 1.4 File Service
- [ ] Implement JSON loading
- [ ] Implement JSON saving
- [ ] Handle file dialogs
- [ ] Error handling for invalid files
- [ ] Write service unit tests

**Deliverable:** Application that can open, display JSON content, and save files.

---

## Phase 2: Core Features

### 2.1 Chart Integration
- [ ] Add LiveCharts2 NuGet package
- [ ] Create ChartView component
- [ ] Bind chart to TorqueCurve data
- [ ] Configure default axes (RPM = X, Torque = Y)
- [ ] Style chart appearance

### 2.2 Grid Lines and Axis Labels
- [ ] Configure axis labels at rounded increments
  - RPM: 500, 1000, 1500, 2000, etc.
  - Torque: 5, 10, 15, 20, etc.
- [ ] Add faded grid lines extending across graph
- [ ] Implement auto-calculation for label spacing (avoid crowding)
- [ ] Labels update smoothly when axis range changes

### 2.3 Hover Tooltip
- [ ] Show RPM and torque values on hover near curve
- [ ] Position tooltip to not obscure cursor
- [ ] Style tooltip for readability

### 2.4 Properties Panel
- [ ] Create PropertiesPanel component
- [ ] Display curve metadata (name, manufacturer)
- [ ] Display data grid of points
- [ ] Bind grid to curve data
- [ ] Enable editing in grid cells

### 2.5 Two-Way Binding
- [ ] Chart updates when grid values change
- [ ] Grid updates when chart is modified (future)
- [ ] Implement INotifyPropertyChanged throughout
- [ ] Handle dirty state tracking (unsaved changes)

### 2.6 Basic Validation
- [ ] Validate RPM values (positive, ascending)
- [ ] Validate torque values (non-negative)
- [ ] Show validation errors in UI
- [ ] Prevent saving invalid data

**Deliverable:** Application with working chart and editable data grid.

---

## Phase 3: Advanced Editing

### 3.1 EQ-Style Curve Editing
- [ ] Enable point selection on chart (click to select)
- [ ] Implement point dragging (drag up/down to adjust torque)
- [ ] Visual feedback when point is selected
- [ ] Sync dragged values back to data model in real-time

### 3.2 Q Value Control
- [ ] Add Q slider (range 0.0 to 1.0)
- [ ] Q affects curve sharpness when editing
  - Low Q = sharp/abrupt changes (affects fewer neighbors)
  - High Q = gradual changes (affects more adjacent points)
- [ ] Visual indication of affected zone when dragging
- [ ] Q value persists during editing session

### 3.3 Background Image Overlay
- [ ] Add "Load Background Image" menu/button
- [ ] Support PNG, JPG, BMP image formats
- [ ] Render image behind chart (z-order below curve)
- [ ] X-axis scale slider for image
- [ ] Y-axis scale slider for image (independent)
- [ ] Position offset controls (optional)
- [ ] Toggle image visibility on/off

### 3.4 Axis Scaling
- [ ] X-axis range slider (min/max RPM)
- [ ] Y-axis range slider (min/max Torque)
- [ ] Smooth graph recalculation (no jitter)
- [ ] Grid lines and labels update with scaling
- [ ] Maintain nice rounded label increments

### 3.5 Add/Remove Data Points
- [ ] Add "Insert Point" button
- [ ] Add "Delete Point" button
- [ ] Handle insertion between existing points
- [ ] Update chart and grid automatically

### 3.6 Undo/Redo (Optional MVP)
- [ ] Implement command pattern for edits
- [ ] Track edit history
- [ ] Enable Ctrl+Z / Ctrl+Y shortcuts

**Deliverable:** Fully functional MVP with EQ-style editing and image overlay.

---

## Phase 4: Polish & User Preferences

### 4.1 User Settings
- [ ] Toggle hover tooltip on/off
- [ ] Persist settings between sessions
- [ ] Settings accessible via menu

### 4.2 UI Polish
- [ ] Add toolbar with common actions
- [ ] Implement keyboard shortcuts
- [ ] Add status bar
- [ ] Add application icon

### 4.3 Unsaved Changes Handling
- [ ] Prompt to save on close
- [ ] Prompt to save on open new file
- [ ] Show asterisk (*) in title for unsaved changes

**Deliverable:** Polished application with user preferences.

---

## Phase 5: Units System (Future)

### 5.1 Tare Integration
- [ ] Add Tare NuGet package
- [ ] Create UnitService using Tare
- [ ] Define supported units (Nm, lbf-in)

### 5.2 Unit Toggle UI
- [ ] Add unit selector in UI
- [ ] Store current unit preference
- [ ] Convert displayed values on toggle

### 5.3 Conversion Logic
- [ ] Convert on display (not stored data)
- [ ] Or: Convert stored data (with user confirmation)
- [ ] Handle precision/rounding

### 5.4 Persistence
- [ ] Save unit preference
- [ ] Remember last used unit
- [ ] Store unit in JSON file (optional)

**Deliverable:** Full unit system support.

---

## Quick Start Commands

### Create New Project

```bash
# Install Avalonia templates
dotnet new install Avalonia.Templates

# Create new Avalonia MVVM project
dotnet new avalonia.mvvm -n CurveEditor -o src/CurveEditor

# Add packages
cd src/CurveEditor
dotnet add package LiveChartsCore.SkiaSharpView.Avalonia
dotnet add package CommunityToolkit.Mvvm
```

### Build and Run

```bash
# Development
dotnet run --project src/CurveEditor

# Publish portable executable
dotnet publish src/CurveEditor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Sample JSON File

Create `samples/example-curve.json` for testing:

```json
{
  "name": "Example Motor",
  "manufacturer": "Acme Motors",
  "unit": "Nm",
  "data": [
    { "rpm": 0, "torque": 50.0 },
    { "rpm": 500, "torque": 52.0 },
    { "rpm": 1000, "torque": 51.0 },
    { "rpm": 1500, "torque": 49.0 },
    { "rpm": 2000, "torque": 46.0 },
    { "rpm": 2500, "torque": 42.0 },
    { "rpm": 3000, "torque": 37.0 },
    { "rpm": 3500, "torque": 31.0 },
    { "rpm": 4000, "torque": 25.0 },
    { "rpm": 4500, "torque": 19.0 },
    { "rpm": 5000, "torque": 14.0 }
  ],
  "metadata": {
    "created": "2024-01-15T10:30:00Z",
    "modified": "2024-01-20T14:45:00Z",
    "notes": "Measured at 25°C ambient temperature, 12V supply"
  }
}
```

---

## Success Metrics

### MVP Complete When:
- [ ] Can create new curve file
- [ ] Can open existing JSON files
- [ ] Displays torque curve as line graph
- [ ] RPM on X axis, Torque on Y axis (default)
- [ ] Grid lines at rounded value increments
- [ ] Hover tooltip shows RPM/torque values
- [ ] Can edit values in data grid
- [ ] Chart updates in real-time
- [ ] EQ-style point dragging works
- [ ] Q slider affects curve sharpness
- [ ] Can load background image
- [ ] Background image scales independently (X/Y)
- [ ] Axis scaling via sliders (smooth, no jitter)
- [ ] Can save to JSON file
- [ ] Runs as portable executable
- [ ] Works on Windows 11

### Nice to Have for MVP:
- [ ] Undo/redo support
- [ ] Keyboard shortcuts
- [ ] Recent files list
- [ ] Axis swap toggle (RPM ↔ Torque)

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| LiveCharts2 learning curve | Start with static charts, add interactivity later |
| File format changes | Design flexible JSON schema |
| Cross-platform issues | Focus on Windows first, test others later |
| Bundle size concerns | Use trimming options, evaluate necessity |

---

## Next Steps

1. Review this roadmap and architecture documents
2. Decide on Avalonia vs WPF (or other option)
3. Set up development environment
4. Create initial project scaffold
5. Begin Phase 1 implementation

---

## Questions to Resolve

1. **JSON Schema**: Is the proposed schema suitable, or do you have an existing format?
2. **Unit Default**: Should Nm or lbf-in be the default unit?
3. ~~**Charting Style**: Preference for line style, colors, grid appearance?~~ ✅ Faded grid lines, rounded label increments
4. **Additional Metadata**: Any other properties needed in the curve model?
5. **Validation Rules**: Specific limits for RPM or torque values?
6. **Q Algorithm**: Preferred algorithm for Q-influenced point editing? (Gaussian, linear falloff, etc.)
7. **Image Formats**: Any specific image formats beyond PNG/JPG/BMP needed?
