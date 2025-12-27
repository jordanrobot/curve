## Curve Editor Phase 3.7 Plan

### Status

Draft

**Related ADRs**

- ADR-0006 Motor File Schema and Versioning Strategy (`docs/adr/adr-0006-motor-file-schema-and-versioning.md`)
- ADR-0003 Generalized Undo/Redo Command Pattern (`docs/adr/adr-0003-motor-property-undo-design.md`)

### Goal

Rename key domain types and related documentation to improve clarity and consistency in the codebase.

### Scope

- In scope:
  - [ ] Rename domain model types across the repo:
    - [ ] `MotorDefinition` -> `ServoMotor`
    - [ ] `CurveSeries` -> `Curve`
    - [ ] `DriveConfiguration` -> `Drive`
    - [ ] `VoltageConfiguration` -> `Voltage`
  - [ ] Rename collection accessors/properties accordingly:
    - [ ] `MotorDefinition.DriveConfigurations` -> `ServoMotor.Drives`
    - [ ] `DriveConfiguration.VoltageConfigurations` -> `Drive.Voltages`
    - [ ] `VoltageConfiguration.CurveSeries` -> `Voltage.Curves`
  - [ ] Update all usages in the Avalonia app project and library project.
  - [ ] Update unit tests to match renamed types/members.
  - [ ] Update documentation to match terminology changes:
    - [ ] `docs/`
    - [ ] `README.md`
    - [ ] XML documentation comments (classes/members) updated to match renamed identifiers

- Out of scope:
  - [ ] Changing the JSON file format or schema (property names, structure, versioning)
  - [ ] Functional UI changes unrelated to the rename
  - [ ] Behavioral refactors beyond what is required to keep build/tests passing
  - [ ] Manual edits to `docs/api/*` (treated as generated output)

### Assumptions and Constraints

- This phase is a rename/refactor for clarity; the on-disk JSON schema remains stable (ADR-0006).
- The primary consumers are this repo's app and tests; breaking changes to public library APIs are acceptable unless we later decide otherwise.
- Keep refactor surgical: change names and references, avoid opportunistic design changes.
- Decision: use `ServoMotor` as the top-level type name. Future support for additional motor types is out of scope for this phase.

### Current Baseline (What exists today)

- Core domain types live in:
  - `src/MotorDefinition/Models/MotorDefinition.cs`
  - `src/MotorDefinition/Models/CurveSeries.cs`
  - `src/MotorDefinition/Models/DriveConfiguration.cs`
  - `src/MotorDefinition/Models/VoltageConfiguration.cs`
- The domain model already has some bridge property names:
  - `MotorDefinition.DriveConfigurations => Drives`
  - `DriveConfiguration.VoltageConfigurations => Voltages`
  (These will need to be revisited during renaming.)
- The Avalonia editor project uses these types broadly:
  - View models: `src/MotorEditor.Avalonia/ViewModels/*.cs`
  - Views/code-behind: `src/MotorEditor.Avalonia/Views/*.axaml.cs`
  - Services/commands: `src/MotorEditor.Avalonia/Services/*.cs`
- API docs appear to be checked in under `docs/api/` with type-named files such as:
  - `docs/api/JordanRobot.MotorDefinition.Model.MotorDefinition.md`
  - `docs/api/JordanRobot.MotorDefinition.Model.CurveSeries.md`
  (For Phase 3.7, treat `docs/api/*` as generated output; do not hand-edit. Keep XML docs correct so regenerated API docs stay consistent.)

### Proposed Design

#### 1) Rename Strategy (Types, Files, Namespaces)

- Use a single, consistent terminology set:
  - Motor (concept) -> `ServoMotor` (type)
  - Curves should read naturally at call sites: `Voltage.Curves`, `Curve.Data`, etc.
- Rename both:
  - C# type names (classes, records, enums if any)
  - File names (`MotorDefinition.cs`  `ServoMotor.cs`, etc.)
  - Member names (properties, methods, parameters)
  - XML docs / `<see cref="..."/>` references
- Namespaces:
  - Keep namespaces stable if possible to reduce churn, unless the repo already follows a pattern of mirroring type names in namespaces.
  - If namespaces must change, treat it as a separate, explicit step and ensure the docs/api generation is updated accordingly.

#### 2) Backward Compatibility Decision (Internal vs Public API)

- Default for this phase: no backwards compatibility shims.
- If we discover that external code consumes `JordanRobot.MotorDefinition` as a library, consider adding temporary compatibility types:
  - `MotorDefinition` as `[Obsolete]` wrapper/alias forwarding to `ServoMotor`
  - Same for `CurveSeries`, `DriveConfiguration`, `VoltageConfiguration`
  This would reduce downstream breakage, but increases maintenance and ambiguity.

#### 3) Serialization and Schema Safety

- Renaming CLR type names should not change JSON unless any of the following are in use:
  - Polymorphic serialization relying on type names
  - Custom converters that emit type names
  - Reflection-based mapping keyed on type names
- Validate the mapper layer (`src/MotorDefinition/MotorDefinitions/Mapping/*`) and DTOs (`src/MotorDefinition/Persistence/Dtos/*`) continue to map correctly.

### Implementation Steps (Incremental)

#### Step 1: Preparation (inventory + guardrails)

- [ ] Inventory all rename targets and high-risk references:
  - [ ] `nameof(MotorDefinition.*)` / `nameof(DriveConfiguration.*)` patterns (used by undo commands)
  - [ ] Reflection-based property access (e.g., edit commands that take a `propertyName` string)
  - [ ] Serialization attributes and DTO mapping points
  - [ ] Documentation generation pipeline (if any) for `docs/api/`
- [ ] Naming locked:
  - `MotorDefinition` -> `ServoMotor`
  - `ServoMotor.Drives` is the canonical drive collection

**Done when**

- A short list of high-risk call sites exists (for the implementation pass).
- Naming decision recorded in this plan (or a short ADR update if needed).

#### Step 2: Rename in the domain library (compile-first approach)

- [ ] Rename types in `src/MotorDefinition/Models/`:
  - [ ] `MotorDefinition` -> `ServoMotor`
  - [ ] `CurveSeries` -> `Curve`
  - [ ] `DriveConfiguration` -> `Drive`
  - [ ] `VoltageConfiguration` -> `Voltage`
- [ ] Rename the corresponding file names to match.
- [ ] Update member names and collections:
  - [ ] `ServoMotor.Drives`
  - [ ] `Drive.Voltages`
  - [ ] `Voltage.Curves`
- [ ] Remove or rename `bridge` properties (`DriveConfigurations`, `VoltageConfigurations`) based on the compatibility decision.

**Done when**

- `src/MotorDefinition/MotorDefinition.csproj` builds with no warnings related to missing types.

#### Step 3: Update persistence, mapping, probing, and validation

- [ ] Update mapping layer and DTO conversions to reference new type names.
- [ ] Update probe/validation APIs that mention old identifiers.
- [ ] Ensure any public surface areas and XML docs are consistent.

**Done when**

- The library builds and its unit tests (if any) pass.

#### Step 4: Update Avalonia app usage (view models, services, views)

- [ ] Update type usages throughout `src/MotorEditor.Avalonia`:
  - [ ] View models
  - [ ] Services
  - [ ] Views + code-behind
- [ ] Pay special attention to undo/redo commands using `nameof(...)`:
  - [ ] `EditMotorPropertyCommand`, `EditDrivePropertyCommand`, `EditVoltagePropertyCommand` patterns
  - [ ] Any reflection-based property setters that rely on `propertyName`

**Done when**

- `src/MotorEditor.Avalonia/MotorEditor.Avalonia.csproj` builds.

#### Step 5: Update tests

- [ ] Update tests under `tests/CurveEditor.Tests/`:
  - [ ] Rename test file/class names if they embed old type names.
  - [ ] Update assertions and helper builders that create model instances.

**Done when**

- `dotnet test` passes for the existing test projects.

#### Step 6: Update docs (roadmap, docs, README, API docs)

- [ ] Update terminology in:
  - [ ] `README.md`
  - [ ] `docs/QuickStart.md`, `docs/UserGuide.md`, and any other docs referencing old names
  - [ ] Relevant ADRs if they name the old types as if they are current
- [ ] Ensure XML documentation comments (types + members) align with the new names so regenerated API docs are consistent.

**Done when**

- Docs consistently use the new terminology, and internal links (if any) remain valid.

### Acceptance Criteria

- [ ] AC 3.7.1: The codebase contains no remaining references to the old domain type names (`MotorDefinition`, `CurveSeries`, `DriveConfiguration`, `VoltageConfiguration`) except any explicitly chosen compatibility shims.
- [ ] AC 3.7.2: `src/MotorDefinition` builds successfully and continues to load/save motor JSON files without schema changes.
- [ ] AC 3.7.3: `src/MotorEditor.Avalonia` builds successfully and the app can open an existing file and show curves.
- [ ] AC 3.7.4: All unit tests pass.
- [ ] AC 3.7.5: `docs/` and `README.md` reflect the new terminology.

### Testing Strategy

- Unit tests:
  - [ ] Run existing unit tests; update them only as needed for rename-induced compilation errors.
  - [ ] Add a focused test only if we discover a subtle rename-induced behavioral regression (e.g., reflection setter property name mismatch).

- Manual validation script:
  - [ ] Launch the app.
  - [ ] Open an existing JSON motor file.
  - [ ] Verify drives/voltages/curves are populated.
  - [ ] Edit a motor property and verify undo/redo still works.
  - [ ] Save and re-open the file to confirm serialization compatibility.

### Risks, Edge Cases, and Mitigations

- Risk: Reflection/property-name based undo commands break after rename.
  - Mitigation: Search for all `nameof(OldType.*)` and string-based property accesses; update and add a small regression test if needed.
- Risk: JSON serialization changes unintentionally.
  - Mitigation: Verify JSON contract attributes and mapper logic; run a round-trip load/save smoke test on `schema/example-motor.json`.
- Risk: Documentation drift or broken links due to file renames.
  - Mitigation: Update docs in the same PR and verify links locally.
- Risk: Large mechanical rename causes noisy diffs and merge conflicts.
  - Mitigation: Prefer one PR that is purely rename (no behavior changes) and keep formatting untouched.

### Follow-on Work and TODOs

- [ ] If external consumers exist, add `[Obsolete]` compatibility shims in a follow-on phase and document the deprecation timeline.
- [ ] Consider aligning the file/schema terminology (if desired) in a future schema-versioned change (ADR-0006).