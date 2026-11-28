# Validation report

Date: 27 November 2026. Source: `kreslingEngineering.fs`, pinned to FeatureScript 2144 and standard library 2144.0.

## What was actually checked

1. Reviewed the original implementation and checked the replacement's public API names and argument schemas against published Onshape documentation.
2. Scanned the source for balanced delimiters, complete helper definitions, and the expected parameter groups. Reviewed conditional parameters, body-query ownership, operation identifier nesting, tool consumption, and the distinction between a tree used for unfolding and additional compatible cell-boundary connections.
3. Implemented an independent Python numerical model of the same intended geometry, using NumPy for coordinates, Shapely for planar profile validity/overlap, and Manifold 3.5.5 for mesh solids and Boolean operations. **The Python model does not execute or compile the FeatureScript.** It can detect mistakes in the underlying design math while leaving FeatureScript-specific errors undiscovered.
4. Checked all pairwise distances within reference polygons and within inset panels before/after exact development, as well as the endpoint alignment of retained creases.
5. Checked inset half-planes, printable attachment lengths, panel-panel interference, every hinge's positive-volume connection to its two panels, unintended hinge-panel interference, hinge-hinge interference, final mesh status, and the count of connected solid components.
6. Checked end-plate clearance at vertices, not only shared edges. Tested holes with reinforcement, rounded panel outlines, slotted tabs, and inverted web placement. Circles and rounded corners in these independent checks use polygonal approximations; Onshape's analytic surfaces and fillets are not tested here.
7. Exercised invalid twist, offset, inset, compression, and net-overlap cases and confirmed rejection in the independent model.

These are selected-state checks. They are not a sweep over the entire UI domain or the mechanical motion path. Mesh intersection checks use a 1e-7 mm³ numerical threshold. The FeatureScript instead uses Onshape `evCollision`, including unintended touching contacts, and requires positive-volume hinge attachment; the two kernels can disagree near tolerances.

## Results

13 representative designs passed in both modes: **26 single-component mesh solids**. Five intentionally invalid designs were rejected. No unexpected verification failures occurred in the final run.

Defaults in this run: six sides, 60 mm circumdiameter, 30 mm per cell, **75° twist**, 1.2 mm panels, 0.4 mm web, 1.6 mm minimum hinge span, 0.6 mm minimum total gap, 0.2 mm extra clearance, 0.8 mm overlap, and 1 mm minimum end relief. The default example has six mountain creases and five valley creases after opening its seam.

| Case | Faces | Retained hinges | Folded / net solid components | Outcome |
|---|---:|---:|---:|---|
| basic | 12 | 11 | 1 / 1 | Pass |
| extended | 36 | 45 | 1 / 1 | Pass |
| alternating | 36 | 45 | 1 / 1 | Pass |
| tapered | 36 | 35 | 1 / 1 | Pass |
| progressive_offset | 36 | 35 | 1 / 1 | Pass |
| clockwise | 12 | 11 | 1 / 1 | Pass |
| caps_holes_ribs | 14 | 13 | 1 / 1 | Pass |
| compressed_redesign | 12 | 11 | 1 / 1 | Pass |
| custom_schedule | 36 | 35 | 1 / 1 | Pass |
| custom_overlap_rejection | — | — | — | Rejected: net overlaps 15,4 |
| rounded_panels | 12 | 11 | 1 / 1 | Pass |
| slotted_tabs | 12 | 11 | 1 / 1 | Pass |
| inverted_hinges | 12 | 11 | 1 / 1 | Pass |
| seam_changed | 24 | 28 | 1 / 1 | Pass |
| twist_too_large | — | — | — | Rejected: twist range |
| offset_too_large | — | — | — | Rejected: offset range |
| collapsed_inset | — | — | — | Rejected: collapsed inset |
| excess_compression | — | — | — | Rejected: compressed height |

Maximum reference-panel distance discrepancy: 3.91e-14 mm. Maximum inset-panel distance discrepancy: 5.33e-14 mm. These are floating-point residuals in a numerical reference, **not CAD manufacturing tolerances**. The assertion tolerance for these distance checks was 1e-8 mm.

`validation_results.json` records volumes and shortest retained hinge lengths. Folded and net volumes differ because finite-thickness hinge cross-sections are rebuilt rather than developed with a neutral-axis model. This is an acknowledged limitation, not a panel-unfolding error.

The `caps_holes_ribs` case includes both 2 mm end plates, 6 mm plate holes, 3 mm panel ventilation holes, 0.8 mm raised reinforcement and 3 mm reinforcement setbacks. The tab case uses the UI's 16 mm width, 10 mm extension, 2 mm slot opening and 2 mm ligaments. The rounded case uses a 0.3 mm radius. The checked custom schedule and rejected overlapping schedule are recorded explicitly in `verify_geometry.py`.

Additional exploratory checks before the final matrix covered the original 25° configuration and 60°, 65°, 90°, 100°, 120° and 150° one-cell configurations with the default material dimensions. These are exploratory results, not a guarantee that every high-twist combination is feasible.

## Reproduce the independent checks

Use Python 3.12 or a compatible Python with the listed packages:

```bash
python -m venv /tmp/kresling-validation
/tmp/kresling-validation/bin/pip install -r requirements-validation.txt
/tmp/kresling-validation/bin/python verify_geometry.py
MPLCONFIGDIR=/tmp/kresling-mpl XDG_CACHE_HOME=/tmp/kresling-cache /tmp/kresling-validation/bin/python make_preview.py
```

Run from the extracted package directory. The verification script recreates JSON results and the meshes needed by the preview script. It exits unsuccessfully if a case unexpectedly fails or an invalid design is unexpectedly accepted. The runtime used to prepare this package already supplied NumPy and Matplotlib; Shapely and Manifold were installed in a temporary dependency directory.

## Not verified here

- Compilation against Onshape's actual pinned 2144 library.
- Feature dialog rendering and standard-library behavior in that version.
- Parasolid B-rep regeneration, analytic fillets, transient query behavior, exact collision classification, or export from Onshape.
- Arbitrary side counts, dimensions, cell schedules, or all combinations of optional features.
- Strain, neutral-axis bend allowance, compliant motion, fatigue, creep, print bridging, slicer paths, load capacity, pressure distribution, locking, or medical use.

No claim is made that a valid mesh proves a valid Onshape B-rep, a printable hinge, or a functional splint.

## Onshape acceptance checklist

1. Paste the entire source into a new Feature Studio and commit. Resolve any compiler diagnostics before using the feature. Do not infer successful compilation from this report.
2. Create the default folded model and verify exactly one solid in the Parts list. Inspect the slit and every visible hinge, and use section views to inspect inner-side connections.
3. Change only Generate to Printable net. Confirm one printable solid; guide sketches may add non-solid reference entities. Verify both mountain and valley guide groups and inspect valley-side bridge spans.
4. Reproduce the basic, extended and tapered examples in `ENGINEERING_GUIDE.md`, in both modes. Confirm the same panel dimensions and appropriate intentional relief cuts.
5. Separately enable plates/holes, reinforcement, rounding, and tabs with the dimensions listed above. Verify body count, minimum ligaments and hinge attachments after each change. Combine options only after checking them separately.
6. Exercise the invalid settings. A regeneration error is the intended outcome; no disconnected partial result should be accepted. Increasing twist above a polygon sector is now allowed, so use an actually invalid range or an interfering geometry rather than relying on the original sector-angle rule.
7. Change side count and cell count, turn alternation on/off, change seam, switch profiles, and return to defaults. Reconfirm regeneration and identity of this feature's own result without modifying pre-existing parts.
8. Export only the intended solid and inspect it in the target slicer for manifold status, wall generation, layer count, bridging and support. Print and cycle a coupon before extending the structure.

Source SHA-256 (current source): `134601f98939212951c63fb5e2ff2a25cf829f92ca1925cde4e4a2256c192c94`.

## Follow-up from user-provided Onshape compiler diagnostics

The supplied compiler log identified a genuine FeatureScript syntax error in the source: dot access `hit.type` used the reserved keyword `type`. All three occurrences now use `hit["type"]`. This fixes the source-level cause of the reported errors at the collision helper.

The log also reports `FeatureScript` at line 4 and the collision-helper error three lines later than the source, consistent with pasting the file beneath the three-line Feature Studio starter header. Replace the entire editor contents with the corrected file so the version statement occurs exactly once, at line 1, and its import occurs once immediately below it.

The corrected file passed header, reserved-member-access, and delimiter checks. It has not yet been recompiled in Onshape. Numerical geometry results are unchanged by this syntax-only correction.

## Second compiler-feedback correction: unique array parameter IDs

Onshape reported `Duplicate feature parameter twist` at the custom-schedule angle declaration. Array-entry parameter IDs must be unique across the entire feature, including top-level parameters. The custom angle declaration is now `cell.cellTwist`, and its regeneration read is `row.cellTwist`. The global `definition.twist` remains unchanged. All scalar and typed UI declarations were scanned together and now have unique terminal parameter names. User-facing labels and geometry calculations are unchanged. This update addresses the reported precondition error; recompilation in Onshape remains pending.

## Manufacturing and connections change

The branch adds separate PLA/TPU construction, round/rectangular captive keys, continuous/segmented webs, fold-specific dimensions, filtered slicer volumes, default inset lacing eyelets, net-only projecting tabs, independent modules, and a common-coordinate export workflow. The original geometry path remains the default. Detailed controls and explicit limitations are in `MANUFACTURING.md`.

Validation for this change:

- `python check_featurescript_source.py`: passes, including 94 globally unique parameter IDs, balanced source, reserved-keyword guards, complete local helper references, the matching 2144 header/import, and role/material-reservation checks. This is not a compiler.
- `python verify_geometry.py`: original 13 designs still pass in both representations; five invalid designs are rejected. The reference model does not execute the FeatureScript.
- `python -m unittest verify_manufacturing -v`: 19 test methods, with folded/net and configuration subcases. Covers round and rectangular captive heads, positive engagement with both owners, finite modifier footprints, unrelated-component interference, separate material reservation, segmented strips, selected mountain folds, fold-specific thickness, capped cells, clockwise/inverted hinges, repeated/alternating cells, taper/custom schedules, independent modules, inset section/seam eyelets, net-only projecting tabs, and invalid manufacturing dimensions.
- Python scripts compile, and `git diff --check` passes.

The manufacturing reference uses 48-sided circle meshes and a positional simplification tolerance of **1e-5 mm**. Near-zero-volume numerical components below **1e-7 mm³** are excluded from connectivity counts. Re-intersecting a difference with its own tools can produce residue at coincident mesh surfaces; the partition-volume bound is the measured surface area multiplied by four times the positional tolerance. This bounds tessellation/Boolean residuals rather than treating them as designed material overlap. The FeatureScript instead reserves the exact TPU solid volume and rejects volume intersections with the remaining PLA using Onshape collision classification; that B-rep behavior still needs Onshape acceptance.

The cross-section in `manufacturing_preview.png` is calculated from an independent planar material partition and is explicitly not an Onshape render or a strength prediction.

### Required review before merging or manufacturing

No successful Onshape compilation/regeneration is claimed for this branch. Compile the complete Feature Studio source, exercise all four manufacturing modes, inspect exact body counts and partitions, and validate export/assignment in the installed Bambu Studio version. Print a material/anchor coupon and validate retention, bridging and fatigue before relying on a full assembly. Projecting tabs are intentionally rejected in Folded mode; inset eyelets and the newer mating dovetails are available in both representations. No merging is authorized by the development request; this branch is to be reviewed in a pull request and merged only after the user's approval.

## Modifier, hole-row and dovetail refinement

The submitted user source matched the previous repository source except for its **Codex Kresling V2** display name, which this update preserves. The original short modifier was a bisector-oriented rectangular prism with hidden width-based end shortening; the replacement uses a full-length spine and panel-following clipped wings. Panel generation, exact development, tiling and material reservation remain the same underlying model.

The user's final hole clarification was **horizontal centers with axes normal to the panels**. No sideways bores or assembly rotation were introduced. A common row-height datum is used for parallel seam copies where feasible; tapered fan edges and module section edges use local horizontal rows. This geometric constraint is described in `MANUFACTURING.md`.

The final local run passed the original 13 valid designs in both modes and five expected rejections. All **32 manufacturing/refinement test methods** passed in 193.225 seconds, with configuration subcases. Python compilation, the source scan and `git diff --check` also passed. The 13 refinement methods cover:

- Rigid transport of reference vertices and the complete print-bed rotation.
- Full crease ribbon endpoints, 1.5 times actual computed gap width, reduced penetration, explicit margins and the absolute-width alternative. Basic, repeated/alternating, tapered and capped designs run in both folded and net frames.
- Horizontal hole spacing, normal-axis drilling and connected owners for default one-hole and larger two-hole seam patterns. The physically incompatible paired two-hole pattern on the smaller tube is rejected.
- Rounded complementary profile offsets, remaining dovetail undercut, disconnected/interfering supports, folded/net owner attachment and clearance from other panels and all original hinges. Basic, repeated/alternating and tapered seams are included.
- Continuous rails, one-way end stops, captured TPU-assisted male keys, and combined PLA/TPU hinges, full ribbons and rigid/TPU-assisted dovetails. The combined TPU-assisted example uses one segment.

These are independent NumPy/Shapely/Manifold calculations, not execution of FeatureScript. Mesh corner arcs are tessellated; the source uses analytic sketch arcs. Tests do not execute the optional Onshape chamfer operation. The source checks report **116 unique parameter IDs and 43 complete helpers**; they do not type-check or compile FeatureScript. New API schemas were reviewed against the published library for `skArc`, `opTransform`, `opPattern`, `opChamfer`, coordinate transforms and bounding-box queries.

The two preview figures are independent calculations, not Onshape screenshots. `make_refinement_preview.py` reproduces the full-ribbon net, horizontal rows and mating cross-section. `make_manufacturing_preview.py` now shows the thinner modifier envelope.

Before accepting this update in Onshape, compile the entire file, regenerate the example nets and folded states, inspect exact partitions and hole positions, and test both rigid and TPU-assisted closure options. Inspect a Front projection for row centers and Top view for the circular net bores. Exercise modifier Disabled while using the modifier manufacturing mode. Check continuous/segmented joints, the optional chamfer and end stop separately. The three local insertion samples do not establish an assembly path for a whole multi-cell structure. Raised dovetail supports require support/clearance checks, and fit, retention and fatigue require physical coupons. No physical test, clinical safety or completed Onshape recompilation is claimed.
