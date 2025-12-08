## ADR 0003: Motor Property Undo Design

### Status

- Accepted

### Date

- 2025-12-07

### Context

The Curve Editor uses a command-based undo/redo infrastructure (`IUndoableCommand`, `UndoStack`) that already covers chart edits (`EditPointCommand`), series metadata (`EditSeriesCommand`), and some motor-level operations (`EditMotorPropertyCommand`).

During Phase 2, we attempted to extend undo/redo to text-based motor properties (e.g., `MotorName`, `Manufacturer`, `PartNumber`) so that:

- Ctrl+Z / Ctrl+Y act at the document level, not just within a single `TextBox`.
- Each edit to a motor property is a whole-field undo step (old value → new value), not per-character.
- Undo/redo across text boxes behaves consistently with chart/grid edits.

An attached behavior (`UndoableTextBox`) was introduced to sit on top of Avalonia's normal two-way bindings. The behavior tried to:

- Capture the original text on focus (`GotFocus`).
- On `LostFocus`, compare the original and new text and, if changed, create an `EditMotorPropertyCommand` and push it to the `UndoStack`.
- Intercept Ctrl+Z/Ctrl+Y at the `TextBox` level and route them to the global undo/redo commands.

Extensive Serilog logging showed that:

- The behavior *did* see focus, lost focus, and text changes correctly.
- The behavior *did* push a command when a motor property changed.
- However, by the time the command executed, the underlying `MotorDefinition` had already been updated by the **existing Avalonia two-way binding**.

As a result:

- `EditMotorPropertyCommand` observed `Old` and `New` values as the **same** (the already-updated value), even after attempts to capture the old value earlier.
- Undo operations simply re-applied the same value and had no visible effect.
- Ctrl+Z from another textbox (e.g., `Manufacturer`) often reached the window-level `UndoCommand` before the per-textbox behavior, so the behavior could not reliably own key routing either.

This aligns with the broader roadmap intent in `04-mvp-roadmap.md` and `phase-2-plan.md` that:

- All user-visible edits (including motor properties) should participate in the same undo/redo infrastructure (Phase 1.8).
- Motor properties are part of the “Core Features” (Phase 2) and must behave consistently with chart and grid edits.

Conclusion: trying to layer command-based undo on top of fully active two-way bindings for motor properties is fragile. The binding pipeline is already mutating the model before the command can capture the true "old" value, and focus/key routing order is subtle. This approach introduces hidden coupling to Avalonia internals and is difficult to reason about and maintain.

### Decision

We will **not** rely on an attached `TextBox` behavior to bolt undoable motor-property edits onto the existing two-way bindings.

Instead, for motor-level text properties we will introduce a **dedicated, command-driven editing path** that owns the mutation and participates in undo/redo by design.

Key aspects of the chosen direction:

1. **ViewModel-centric edit methods**
   - Add explicit edit methods on `MainWindowViewModel` (or a dedicated `MotorPropertiesViewModel`), e.g.:
     - `EditMotorName(string newName)`
     - `EditMotorManufacturer(string newManufacturer)`
     - `EditMotorPartNumber(string newPartNumber)`
   - Each method will:
     - Read the current value from the model (old value).
     - Construct and push an `IUndoableCommand` with both `old` and `new` values.
     - Apply the new value to the underlying `MotorDefinition` as part of command `Execute()`.

2. **UI bindings use the dedicated path instead of raw two-way binding**
   - For motor text fields, we will stop binding `TextBox.Text` directly to `MotorDefinition` properties with unconstrained two-way bindings.
   - Instead, we will either:
     - Bind to simple VM properties (e.g., `MotorNameEditor`), and on commit (Enter key, focus loss, or explicit "Apply"), call the corresponding edit method, or
     - Use command bindings (e.g., `TextBox` submit behavior) that pass the edited value into `EditMotorName`.
   - The critical property: **only the command path mutates `MotorDefinition`**. The binding no longer writes directly to the model behind the command's back.

3. **Command representation**
   - Either refactor `EditMotorPropertyCommand` or introduce a specialized command, e.g., `EditMotorScalarPropertyCommand`, that:
     - Stores both `oldValue` and `newValue` at construction time.
     - In `Execute()`, sets the property to `newValue`.
     - In `Undo()`, sets the property back to `oldValue`.
   - The command should not attempt to infer `oldValue` from the model at execution time.

4. **Global Ctrl+Z / Ctrl+Y remain window-level**
   - Keyboard shortcuts continue to route through `MainWindow` and `MainWindowViewModel.UndoCommand` / `RedoCommand`.
   - Motor property edits become just another `IUndoableCommand` on the existing per-document `UndoStack`, so global undo/redo behavior is consistent across charts, grids, and motor fields.

### Consequences

**Positive:**

- **Robustness:**
  - Undo/redo for motor properties is no longer sensitive to Avalonia's internal binding update order or focus event timing.
  - All model mutations for these fields go through a single, explicit command path.

- **Clarity:**
  - Future contributors (or agents) can reason about motor property edits by looking at view-model edit methods and commands, not at subtle behaviors and attached properties.
  - Unit tests can target view-model methods directly without requiring UI event orchestration.

- **Consistency:**
  - Motor property edits behave like other undoable operations in the system: a user-visible action → command on `UndoStack` → model mutation → undo/redo.

**Negative / Trade-offs:**

- **Refactor cost:**
  - Existing direct two-way bindings from `TextBox.Text` to `MotorDefinition` properties must be refactored to go through the dedicated edit path.
  - Short-term implementation effort is higher than the behavior-only approach.

- **Slightly more view-model surface area:**
  - Additional edit methods and/or small editor properties increase the size of `MainWindowViewModel` or introduce another VM layer.

### Implementation Notes / Migration Plan

This ADR is the authoritative guide for future work described in:

- `04-mvp-roadmap.md`, sections **1.8 Undo/Redo Infrastructure** and **2.6 Motor Properties Panel**.
- `phase-2-plan.md`, sections **2.3 Command Types for Common Operations** and **4. Future Work: Motor Property Undo via Dedicated Command Path**.

When an agent implements this design, they should:

1. **Refactor bindings for key motor fields**
   - Start with the most important text fields (e.g., `MotorName`, `Manufacturer`, `PartNumber`).
   - Replace direct `Text="{Binding CurrentMotor.MotorName}"` with either:
     - A VM editor property (e.g., `Text="{Binding MotorNameEditor}"`) plus an explicit commit trigger, or
     - A command-binding pattern where the edit is committed via `EditMotorName`.

2. **Add explicit edit methods**
   - On the relevant view model (likely `MainWindowViewModel`):
     - Implement `EditMotorName(string newName)` and similar methods.
     - Inside each method:
       - Get `var oldName = CurrentMotor.MotorName;`.
       - If `oldName == newName`, do nothing.
       - Construct an undoable command with `oldName` and `newName`.
       - Call the existing `_undoStack.PushAndExecute(command);` and mark dirty.

3. **Use commands that store old/new explicitly**
   - Update or add a command type so it has a constructor like:
     - `EditMotorPropertyCommand(MotorDefinition motor, string propertyName, object? oldValue, object? newValue)`.
   - Ensure `Execute()` and `Undo()` only ever read from these stored values.

4. **Integrate with existing undo infrastructure**
   - Reuse the existing `UndoStack` in `MainWindowViewModel`.
   - No changes should be needed to `UndoStack` itself; motor edits simply become normal commands on that stack.

5. **Testing**
   - Add unit or integration tests that:
     - Create a `MainWindowViewModel` with a test `MotorDefinition`.
     - Call `EditMotorName("New Name")` and assert that the model updates.
     - Call `Undo()` and assert that the model reverts to the original name.
     - Call `Redo()` and assert that the model returns to the new name.

### Alternatives Considered

1. **Keep attached behavior and try to capture values earlier**
  - Capture `oldValue` on `GotFocus` and store it in the behavior.
  - On `LostFocus`, construct a command using the stored `oldValue` and current `TextBox.Text`.
  - **Rejected** because Avalonia bindings may still update the model before or during these events, leading to subtle race/timing issues and hard-to-test behavior.

2. **Use two-way bindings but hook into property changed events on the view model**
  - Observe `INotifyPropertyChanged` on `MotorDefinition` and create commands when properties change.
  - **Rejected** because it inverts control (commands are an after-the-fact log) and makes it difficult to group related edits or avoid double-mutations. It also couples undo logic to model-level notifications rather than explicit user actions.

3. **Make motor properties immutable snapshots and rebind on every change**
  - Treat motor edits as replacing a whole `MotorDefinition` instance on each edit.
  - **Rejected** as unnecessarily heavy for the current application scope and inconsistent with existing mutable model semantics.

### References

- `src/CurveEditor/Services/UndoStack.cs` – core undo/redo infrastructure.
- `src/CurveEditor/Services/EditMotorPropertyCommand.cs` – existing motor property command implementation.
- `src/CurveEditor/ViewModels/MainWindowViewModel.cs` – owner of the per-document `UndoStack` and motor definition.
- `.github/planning/04-mvp-roadmap.md` – roadmap context for undo/redo and motor properties.
- `.github/planning/phase-2-plan.md` – phase 2 follow-through and future work items that depend on this ADR.
