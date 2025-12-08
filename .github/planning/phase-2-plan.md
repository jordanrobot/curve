## Curve Editor Phase 2 Plan

**Related ADRs**

- ADR-0003 Motor Property Undo Design (`.github/adr/adr-0003-motor-property-undo-design.md`)

### 2. Undo/Redo Infrastructure (Phase 1.8 follow-through)
- **Goal**: Provide robust undo/redo for common editing actions and ensure it integrates with dirty-state tracking.
- **Context**: Roadmap section `1.8 Undo/Redo Infrastructure` outlines the building blocks. This plan refines them.

#### 2.1 Core Undoable Command Model
- [x] Define an `IUndoableCommand` interface with at least:
  - `void Execute()`
  - `void Undo()`
  - Optional: a `string Description` or similar for debugging/UI.
- [x] Decide on whether commands are:
  - Per-motor/document, or
  - Global for the entire app.
  (Recommended: one undo stack per open motor/file.)

#### 2.2 Undo Stack Service
- [x] Create an `UndoStack` (or `UndoService`) responsible for:
  - Holding a stack of executed commands and a separate stack for redo.
  - `PushAndExecute(IUndoableCommand command)` that runs `Execute()` and records the command.
  - `Undo()` that pops the last command, calls `Undo()`, and pushes it onto the redo stack.
  - `Redo()` that re-executes the last undone command and moves it back to the undo stack.
- [x] Expose simple properties/events:
  - `CanUndo`, `CanRedo` for button/shortcut enabling.
  - `UndoStackChanged` (optional) for debug/insights.
- [x] Associate the undo stack with the current document/motor definition so it resets appropriately when a new file is opened.

#### 2.3 Command Types for Common Operations
- [x] Implement concrete `IUndoableCommand` types for the main editing actions:
  - `EditPointCommand`: change torque (and/or RPM) for a single data point at a given index/series.
  - `EditSeriesCommand`: rename, visibility change, color change, or lock state for a `CurveSeries`.
  - `EditMotorPropertyCommand`: change a scalar property on `MotorDefinition` or its nested metadata (e.g., max RPM, max torque).
- [x] Ensure each command captures enough prior state to undo reliably (old/new value, series id, index, etc.).
- [x] Centralize domain mutations so that:
  - Direct property edits from the UI go through helper methods that create and push commands rather than mutating models directly.

See ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`) for the specific command-driven pattern adopted for motor-level text properties.

**Future refinement for motor text properties:**

Integration experiments with attached `TextBox` behaviors showed that trying to retrofit undoable motor text edits on top of fully active two-way bindings is brittle: by the time a command runs, the underlying `MotorDefinition` has often already been updated, so the command cannot reliably capture the true "old" value. The agreed direction is to introduce a dedicated, command-driven editing path for motor-level text properties (e.g., Motor Name, Manufacturer, Part Number) instead of relying on behaviors. See ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`) for details.

#### 2.4 Wiring Undo/Redo to UI and Dirty State
#### 2.4 Wiring Undo/Redo to UI and Dirty State
- [x] Wire Ctrl+Z / Ctrl+Y (or platform-appropriate equivalents) to call `Undo()` and `Redo()` on the active document's undo stack.
- [x] Add toolbar/menu items for Undo/Redo, bound to the same commands, with enable/disable driven by `CanUndo` / `CanRedo`.
- [x] Integrate undo/redo with dirty tracking:
  - Mark the document dirty whenever a command is executed.
  - Track the "clean" checkpoint (state after last save) so undoing back to that point clears the dirty flag.

#### 2.5 Tests and Safeguards
- [x] Add unit tests for `UndoStack` behavior:
  - Push/execute, undo, redo sequences.
  - Edge cases when stacks are empty.
- [x] Add unit tests for each concrete command type to verify:
  - `Execute()` applies the expected change.
  - `Undo()` fully restores prior state.
- [x] Add a small integration-style test around a view model (e.g., editing a point in the data grid) to ensure UI-driven edits go through commands and can be undone/redone.

### 3. Phase 2 Completion Criteria
- [x] All items under `1.7 Logging and Exception Handling` in `04-mvp-roadmap.md` are satisfied by the implemented logging/exception handling.
- [x] All items under `1.8 Undo/Redo Infrastructure` are satisfied by the implemented undo/redo system.
- [x] New logging and undo/redo behavior does not break existing tests; new tests are added where appropriate to cover the new infrastructure.

### 4. Future Work: Motor Property Undo via Dedicated Command Path

Although the core undo/redo infrastructure is in place, motor-level text properties are still using direct two-way bindings to `MotorDefinition`. To bring these fields fully under the undo/redo system in a maintainable way, a future agent should implement the following plan (guided by ADR-0003):

1. **Introduce explicit edit methods on the view model**
   - On `MainWindowViewModel` (or a dedicated `MotorPropertiesViewModel`), add methods such as:
     - `EditMotorName(string newName)`
     - `EditMotorManufacturer(string newManufacturer)`
     - `EditMotorPartNumber(string newPartNumber)`
   - Each method should:
     - Read the current value from `CurrentMotor`.
     - Early-out if the value is unchanged.
     - Construct an `IUndoableCommand` containing both old and new values.
     - Push and execute the command through the existing `UndoStack`, ensuring dirty state is updated.

2. **Refactor bindings for key motor text fields**
   - In `MainWindow.axaml`, replace direct bindings like `Text="{Binding CurrentMotor.MotorName}"` with a pattern that routes edits through the view model, for example:
     - Bind `TextBox.Text` to a simple VM property (e.g., `MotorNameEditor`) and, on commit (Enter key or focus loss), invoke `EditMotorName(MotorNameEditor)`.
     - Or use a command-based input pattern where the new text is passed as a command parameter to `EditMotorName`.
   - Ensure the underlying `MotorDefinition` is mutated **only** via the command path, not by the binding itself.

3. **Update or extend the motor property command type**
   - Either refactor `EditMotorPropertyCommand` to accept both `oldValue` and `newValue` in its constructor, or add a new command type that:
     - Stores `oldValue` and `newValue` explicitly.
     - Sets the property to `newValue` in `Execute()` and back to `oldValue` in `Undo()`.
   - The command should not attempt to infer `oldValue` from the model at execution time.

4. **Keep keyboard shortcuts at the window level**
   - Leave the existing Ctrl+Z / Ctrl+Y key bindings on `MainWindow` bound to `UndoCommand` / `RedoCommand`.
   - Once motor property edits are command-driven, they will naturally participate in the global undo/redo history without additional per-control key handling.

5. **Add focused tests**
   - Add tests that:
     - Create a view model with a `MotorDefinition`.
     - Invoke `EditMotorName("New Name")` and assert the model changes.
     - Call `Undo()` and verify the original name is restored.
     - Call `Redo()` and verify the new name is applied again.

This plan avoids the fragility observed with attached behaviors and tightly couples motor property edits to the undo/redo infrastructure in a clear, testable way.

This future work is governed by ADR-0003 (`.github/adr/adr-0003-motor-property-undo-design.md`), which documents the rationale, decision, and migration steps.
