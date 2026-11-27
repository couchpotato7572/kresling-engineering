# Kresling Engineering — FeatureScript 2144

This package is a complete source implementation of a **static folded design and its exact cut-panel net**. It retains the original triangular wall construction, inward panel thickening, edge setbacks, and thin hinge webs. It adds repeated cells, exact unfolding, tapering, custom schedules, and optional prototype attachment features.

**Verification status:** the geometry was checked numerically and with an independent mesh Boolean kernel. The FeatureScript has not been compiled or regenerated in an Onshape session. Treat it as a source implementation awaiting that acceptance check, not an Onshape-certified or clinically validated tool. There are no placeholder functions.

**Mechanical scope:** this is an open-seam, compliant prototype generator. Compression changes the designed triangle dimensions. It does not predict the motion, load capacity, locking, or fatigue life of a fixed printed specimen. Finite-thickness hinge bend allowances are not solved.

The new **Manufacturing and Connections** group is documented separately in [MANUFACTURING.md](MANUFACTURING.md). It adds disjoint PLA/TPU material bodies, captive anchors, slicer-only volumes, lacing eyelets and independent modules. The original parameter reference below still applies to the common geometry.

## Files and installation

- `kreslingEngineering.fs`: complete Feature Studio source, including helpers.
- `VALIDATION.md`: checks performed, results, and remaining Onshape acceptance work.
- `verify_geometry.py` and `validation_results.json`: independent numerical and mesh-kernel verification.
- `geometry_preview.png`: calculated reference preview, not an Onshape screenshot.

1. Create a **Feature Studio** in Onshape.
2. Click inside the code editor, press Ctrl+A (Cmd+A on macOS), and replace all its text with `kreslingEngineering.fs`. Remove the starter header along with the rest of the starter text; the pasted file already contains its own header. The first two lines must remain `FeatureScript 2144;` and the matching `2144.0` common-library import. Version 2144 intentionally matches your source; it is not presented as the newest language version.
3. Commit the Feature Studio and add **Kresling Engineering** to a Part Studio using the custom-feature button.
4. Start with the defaults. The expected result is one solid, six sides, one cell, 60 mm circumdiameter, 30 mm reference height, 75° twist, and open ends.
5. Follow the Onshape acceptance checklist in `VALIDATION.md` before relying on an export. If your document upgrades the FeatureScript version, upgrade the header and library import together and recheck regeneration.

For a folded model, select **Folded solid**. For the corresponding net, select **Printable net** while retaining every geometry, material, clearance, and accessory setting. Use separate Part Studios or configurations for the two representations. Changing compression or twist requires regenerating the matching net.

The net's outer reference surface lies on Z = 0; material extends in +Z. Rotation changes its XY orientation, not its geometry. Hide crease guides and select only the solid when exporting STL or 3MF. The guide sketches are reference geometry, not engraved text or printable fold markings.

## Analysis of the supplied script

The original creates bottom and top regular polygons and connects them with two triangles per side:

- Lower triangle: `(B_i, B_(i+1), T_i)`.
- Upper triangle: `(T_i, T_(i+1), B_(i+1))`.

It optionally includes polygonal end plates. Shared edges receive setbacks, convex creases receive extra thickness clearance, panels extrude inward, and six-point hinge cross-sections bridge selected panel edges. A final Boolean joins the bodies, and a center-axis cutter optionally makes a hole.

These useful constructions are preserved. Their limitations were:

1. Height and twist were independent shape inputs, with no fixed-edge-length folding constraint.
2. There was no flat development, repetition, taper, or per-cell schedule.
3. Polygon insets were calculated without proving that the inset was nonempty and uninverted.
4. A short hinge could be silently omitted, disconnecting the structure.
5. A union was assumed to produce one body without checking connectivity.
6. Edge-only clearance missed end-plate contacts at vertices.
7. A fixed hinge end margin did not ensure that the attachment strip fit the trimmed triangles.
8. The nearly parallel offset fallback could silently violate one of its two plane equations.
9. Hole sizing used a specialized mid-height formula, and an axis cutter could remove wall material.
10. There was no check of unintended contacts before the union concealed them.
11. For regular coaxial cells, the restriction to less than one polygon sector selects a convex antiprism-like regime. The new angle range extends beyond that restriction so the default 75° six-sided example has both convex and concave creases. Self-intersecting high-twist states are still rejected.

The replacement uses vertex identities for adjacency, validates convex inset half-planes, clips hinge attachment intervals, clears complete end-plate slabs, rejects singular offsets, checks component contacts, and requires exactly one final solid. Holes are cut locally in their owning panels or end plates, with explicit ligament checks.

## Geometry and the folding limitation

Let `n` be the side count, `a = 2π/n`, `R_k` the radius of ring k, `C_k` its center, and `theta_k` its accumulated rotation. A ring vertex is

```
V_(k,i) = C_k + R_k [cos(theta_k + i*a), sin(theta_k + i*a), 0].
```

The ring center contains its actual Z coordinate. Each cell connects two rings with the original two-triangle pattern. A clockwise cell mirrors both its twist and triangle diagonal. Alternating cells therefore share identical ring vertices while reversing chirality correctly.

For coaxial equal-radius rings, the bottom edge and two sloping edge lengths are

```
s = 2 R sin(a/2)
b² = h² + 2 R² [1 - cos(phi)]
d² = h² + 2 R² [1 - cos(a - phi)].
```

Keeping the circumferential edges and a regular cross-section fixes R. Keeping b and d also fixes their squared difference:

```
b² - d² = 2 R² [cos(a - phi) - cos(phi)].
```

Within the original interval `0 < phi < a`, this determines phi uniquely; h follows. The extended angle range can admit isolated alternative configurations, but still does not supply a continuous rigid-folding path. Height and twist therefore cannot be arbitrary independent motion controls for the same rigid triangles. Real Kresling specimens can move through facet bending, ring distortion, hinge extension, or other compliant deformation. Those effects require a mechanical model and measurements.

The feature uses

```
actual cell height = reference cell height * (1 - design compression).
```

It then recalculates that design's panels. For example, 0.2 means a 30 mm reference cell is redesigned to 24 mm. Minimum and maximum compression are **input limits**, not verified travel limits or physical stops. The feature checks the selected state; it does not certify the entire interval or a folding path.

For a polygon-perimeter input P, radius is derived as

```
R = P / [2 n sin(π/n)].
```

This is the regular polygon perimeter, not the circumference of its circumcircle. A circumdiameter is a reference ring dimension; inward material and optional accessories determine the finished envelope and available internal space.

For progressive taper, with ring fraction u = k / cell count,

```
R_k = R_bottom + (R_top - R_bottom) * u^p.
```

Linear taper uses p = 1. Center offsets interpolate linearly unless a custom schedule is selected.

### Exact net and deliberate cuts

The feature unfolds faces across a connected tree of retained creases. Each new face is positioned using its original edge and perpendicular in-plane coordinates, on the opposite side of the shared edge from its parent. All distances within each reference polygon and each trimmed panel are preserved.

One longitudinal seam per cell is cut. Initially, one shared ring edge connects neighboring cells. Additional ring hinges are restored automatically **only when both endpoints coincide in the exact development**. Constant untapered rows normally retain every inter-cell ring hinge. Tapered or asymmetric rows may require more relief cuts and retain only one bridge. End plates attach as hinged flaps along one edge so they can unfold.

This deliberately avoids forcing a nondevelopable closed faceted surface into a plane. The folded and flat outputs use the same retained connection graph. Some exact layouts overlap, especially with custom heights, twist changes, offsets, or caps; those nets are rejected. Changing seam may help, but it is not guaranteed. A practical fallback is separate one-cell nets joined with a separately designed connector.

The net preserves **panel geometry**, not an exact neutral-axis development of every thick hinge. Folded and flat hinge volumes differ slightly because the offset surfaces and sharp web bends are rebuilt in each representation. Hinge strain, bend allowance, corner radii through the material thickness, springback, and closure preload remain physical-design work. Changing web sides is not a substitute for that work.

Mountain and valley guide groups describe convexity of the **outer reference surface in the selected folded state**. Red is mountain; blue is valley. In the XY net the outer surface faces the bed, so the fold direction appears reversed when viewed from +Z on the inner face. Inverting hinge sides changes web placement, not the mountain/valley classification. Boundary guides are not provided for relief cuts; the open panel edges reveal them.

## Parameter reference

Lengths below are millimeters; angular values are degrees in the dialog. Lengths display in the Part Studio's selected units. Boolean settings default off unless stated otherwise.

### Generation Mode

| Parameter | Meaning and default |
|---|---|
| Generate | Folded solid by default; Printable net gives the exact cut-panel development. |
| Crease guide sketches in net | On. Adds named mountain/valley construction sketches above the net. Has no effect on the printable solid. |

### Basic Dimensions

| Parameter | Meaning and default |
|---|---|
| Polygon sides | 6; allowed 3–16. The side count is constant along a feature. |
| Cross-section measurement | Circumdiameter by default, or polygon perimeter. |
| Bottom circumdiameter | 60; shown for diameter input. |
| Bottom polygon perimeter | 180; shown for perimeter input. |
| Reference height input | Per-cell height by default, or overall reference height divided by cell count. |
| Reference cell height | 30; before design compression. Custom schedules override it. |
| Overall reference height | 90; before compression, when this height mode is selected. Custom schedules override it. |

### Kresling Geometry

| Parameter | Meaning and default |
|---|---|
| Cell twist / folding angle | 75°. Must be strictly between 0° and 180°; individual shapes still require the collision and inset checks. A design angle, not a solved motion angle. Custom schedule entries override its magnitude. |
| Clockwise viewed from +Z | Off; sets initial chirality. |
| End plates | Open ends by default; bottom, top, or both are available. Each plate has one retained hinge and relief cuts elsewhere. |
| End plate thickness | 2; visible when a plate is selected. |
| End plate center hole diameter | 0 = none. A local plate hole with a checked edge ligament. |

### Folding and Compression

| Parameter | Meaning and default |
|---|---|
| Design compression | 0; allowed 0–0.65. Multiplies every reference cell height by 1 minus this value. Redesigns the panels. |
| Allowed minimum compression | 0; lower input bound. |
| Allowed maximum compression | 0.5; upper input bound. Does not guarantee that every intermediate design is collision-free. |

### Tiling and Repetition

| Parameter | Meaning and default |
|---|---|
| Number of cells | 1; allowed 1–8. Large side counts and cell counts increase regeneration time. |
| Alternate twist direction | Off; reverses both handedness and the facet diagonal on every second cell. |
| Seam vertex (zero based) | 0; must be less than side count. Selects the circumferential cut and the initial inter-cell bridge. |

### Tapering and Transitions

| Parameter | Meaning and default |
|---|---|
| Cross-section profile | Constant by default; Linear taper, Progressive taper, or Custom cell schedule. |
| Top circumdiameter | 45; used by linear and progressive taper. Bottom size still comes from Basic Dimensions. |
| Taper exponent | 2; shown for progressive taper. Allowed 0.25–4; 1 gives linear taper. |
| Top center X / Y offset | 0 / 0; the total end offset, distributed across cells. Per-cell change cannot exceed half the smaller adjoining radius. |
| Cells from bottom to top | Custom array with exactly one entry per cell. Add entries in bottom-to-top order. |
| Schedule: Reference cell height | Each cell's uncompressed height; default 30. Actual total height is the sum after compression. |
| Schedule: Top circumdiameter | The diameter of that cell's top ring; default 60. Its bottom ring is the preceding ring. |
| Schedule: Twist magnitude | Default 75°; uses the global handedness and alternation rules. |
| Schedule: Top center X / Y | Absolute XY coordinates relative to the first bottom center; default 0 / 0. They are not incremental displacements. |

The Basic Dimensions height fields and global twist remain visible but are overridden by a custom schedule. Side-count changes require a separate transition component; no incompatible polygon rings are forced together.

### Hinge and Material Settings

| Parameter | Meaning and default |
|---|---|
| Panel thickness | 1.2, extruded inward from the reference surface. |
| Hinge thickness input | Direct web thickness by default, or crease depth. |
| Hinge web thickness | 0.4. Must meet the printable web minimum and be at most 80% of the thinner adjoining wall. It is never silently clamped. |
| Crease depth from panel thickness | 0.8 when this input mode is selected; web thickness = panel thickness minus crease depth. This produces a remaining web, not a separate scored-line cutter. |
| Minimum flexible span across crease | 1.6; a lower bound on the sum of the two panel edge setbacks. Thickness clearance can enlarge it. |
| Hinge-to-panel overlap | 0.8; positive web attachment into each neighboring panel. Also reserved at adjacent hinge ends to prevent unintended merging. |
| Invert web sides | Off; swaps convex/concave-side placement. Requires the same collision checks as the normal arrangement. |

### Printing and Manufacturing

| Parameter | Meaning and default |
|---|---|
| Minimum total crease clearance | 0.6; shared between both panel setbacks. Larger hinge span and geometric thickness clearance take priority. |
| Extra thickness clearance | 0.2 total extra allowance beyond converging thick-face offsets. |
| Minimum printable wall | 0.8; rejects thinner panels and plates. Printer/material setting, not a strength rating. |
| Minimum printable web | 0.3; rejects a thinner effective hinge. |
| Minimum hinge end relief | 1; enlarged where needed to keep the entire attachment inside both inset panels. Hinges shorter than max(1 mm, 3 web thicknesses) are rejected. |
| Round panel outline corners | Off; fillets panel edges parallel to the panel normal before assembly. Does not fillet hinge roots or strap slots. |
| Panel corner radius | 0.3; cannot exceed half the hinge overlap. Onshape may reject an oversized radius on a small panel. |
| Net rotation on print bed | 0°; XY rotation, followed by placement of reference vertices at nonnegative XY coordinates. Tabs may extend beyond that reference envelope. |

### Splint Integration Features

| Parameter | Meaning and default |
|---|---|
| Panel ventilation hole diameter | 0 = none; one circular hole near each inset panel's centroid. It does not imply a medically appropriate ventilation pattern. |
| Minimum hole / slot ligament | 2; preserves material around panel holes and tab slots. |
| Interior panel reinforcement islands | Off; adds a smaller, inset raised panel region. It stiffens panels without deliberately spanning hinges. It protrudes on the inner face and may need padding clearance. |
| Reinforcement height | 0.8. |
| Reinforcement setback from panel edges | 3; an additional inset from the already trimmed panel outline. Too large an inset is rejected. |
| One slotted attachment tab at each end | Off; visible with open ends. Adds one tab to the selected end edge at each end. They are prototype fastening anchors, not a locking mechanism. |
| Tab width along end edge | 16; must fit the end edge with the selected ligaments. |
| Tab extension | 10, outward in the owning facet plane beyond the free end edge. |
| Strap slot opening | 2; the slot's other dimension is tab width minus two ligaments. The tab must also preserve ligaments before and after the slot. |

## Example configurations

Keep unlisted settings at their defaults. Each case was checked as both a folded mesh solid and a net solid in the independent validation.

| Setting | Basic tube | Extended tube | Tapered tube |
|---|---:|---:|---:|
| Sides | 6 | 6 | 6 |
| Bottom circumdiameter | 60 | 60 | 60 |
| Reference height input | Per cell | Per cell | Per cell |
| Reference cell height | 30 | 30 | 30 |
| Cells | 1 | 3 | 3 |
| Profile | Constant | Constant | Linear taper |
| Top circumdiameter | — | — | 45 |
| Twist | 75° | 75° | 75° |
| Compression | 0 | 0 | 0 |
| Panel / web thickness | 1.2 / 0.4 | 1.2 / 0.4 | 1.2 / 0.4 |
| End plates | Open | Open | Open |
| Resulting height | 30 | 90 | 90 |
| Faces / retained hinges | 12 / 11 | 36 / 45 | 36 / 35 |

A checked custom three-cell schedule is `(reference height, top diameter, twist, absolute X, absolute Y)`:

```
Cell 1: (30, 56, 25°, 1, 0)
Cell 2: (30, 52, 25°, 2, 0)
Cell 3: (30, 48, 25°, 3, 0)
```

This has different cell diameters and offset centers. Other custom schedules can be valid in 3D but have overlapping nets. Such a net is intentionally not generated.

## Printing and mechanical development

For a first nonclinical FDM coupon with a 0.4 mm nozzle, the default 1.2 mm panels, 0.4 mm web, 1.6 mm flexible span, and 0.2 mm extra thickness clearance are starting dimensions, not universal manufacturing rules. A 0.4 mm web corresponds to two 0.2 mm layers when its orientation permits. Confirm actual slicer paths and layer quantization rather than assuming that any nominal CAD thickness becomes printed material. Test a single cell before a full-length structure.

The net has all panel reference planes on the bed, but valley-side webs sit above the bed. They require short bridges, whose free span can grow substantially when thickness-clearance setbacks dominate. Inspect bridge direction and sag in the slicer; support material in a hinge can obstruct motion. Unequal plate thicknesses produce sloped web transitions. Folded-mode solids may require considerably more support and usually are less convenient to print.

Orient extrusion paths across the flexure where possible and test both orientations. A single material must provide both panel stiffness and hinge fatigue resistance; those requirements compete. PLA hinges are not a reliable default for repeated flexing. TPU is compliant but may lack useful deployed stiffness. Polypropylene and other hinge-capable materials need printer-specific process development and still require cycling tests. Avoid sharp edge contact with skin, and do not infer biocompatibility or suitability for prolonged contact from a filament name.

The modeled web uses sharp bends and straight attachment edges. Panel outline rounding reduces exposed corner sharpness; it does **not** create a strain-controlled hinge root. A later hinge module should introduce a calibrated bend radius and smooth root transitions, then regenerate the net with a matching neutral-axis allowance. Perforating a narrow hinge can worsen fatigue and is deliberately not offered as an unverified default.

For splint development, separately assess deformation and retention forces, pressure distribution, strap loads, torsion, lateral stability, fatigue, creep, cleaning, padding clearance, and failure behavior. Open seams and relief-cut cell boundaries change the load path. A tapered interface with only one retained bridge is particularly asymmetric and must not be treated as a structural collar. Raised ribs and tabs can create local pressure points. Adjustable compression in CAD does not establish controlled compression on a limb.

This prototype geometry does not establish medical suitability or clinical safety. Any intended orthopedic use needs appropriate engineering and clinical evaluation; initial mechanical tests should use fixtures or limb surrogates.

## Deliberate limitations and next stages

| Requested capability | This implementation and practical next step |
|---|---|
| Continuous compression of a fixed printed net | Not solved. Add a compliant-mechanism model that permits ring distortion/facet strain, with measured constitutive properties and collision checks over the entire motion path. |
| Guaranteed stable deployed state or locking | Not claimed. Design and validate a separate strap, latch, removable collar, or mechanical stop using measured loads. The input compression bounds do not implement stops. |
| Different side counts along the structure | Not supported inside one feature. Use independent Kresling sections connected by a dedicated transition collar, triangulated adaptor, or soft connector. Such a collar must be designed around the mating rings; it is not inherently a foldable Kresling cell. |
| Arbitrary overlapping exact nets | Rejected. Try another seam or split the print into separate cells with a connector. An automatic packing/cut-graph search is a separate extension. |
| Seam closure | One longitudinal slit per cell remains. Optional inset lacing eyelets support cord closure after wrapping; optional protruding tabs are net-only assembly features. No permanent bridge crosses the flat net. |
| Rigid end plates attached around the entire rim | Not in the matching flat net. The supplied caps are single-edge hinged flaps. A removable cap or separately assembled connector is the practical alternative. |
| Reinforcement bands, padding channels, anatomical contact shell | Not generated. Design these around a validated interface/clearance envelope; islands and slotted tabs are the included integration primitives. |
| Rounded hinge roots and perforation patterns | Not supplied. Panel outline fillets are included. Root geometry needs bend-allowance and fatigue development; holes in hinges require testing before adding a generator. |
| Guaranteed regeneration over all parameter combinations | Not claimed. Invalid combinations fail with errors; only representative states have independent checks. Onshape compilation and B-rep regeneration remain required. |

The next useful engineering milestone is a one-cell material coupon with measured fold moment, reversible travel, and cycle life. Its results should determine hinge geometry and permitted motion before adding locking hardware or anatomical sizing.

## Onshape API references

- [Feature UI, conditional parameters, groups, and arrays](https://cad.onshape.com/FsDoc/uispec.html)
- [Standard library documentation](https://cad.onshape.com/FsDoc/library.html), including `opExtrude`, `opBoolean`, `opFillet`, `evCollision`, and queries.
- [Feature type definitions](https://cad.onshape.com/FsDoc/feature-types.html)
- [Modeling and operation identifiers](https://cad.onshape.com/FsDoc/modeling.html)

API names and schemas were checked against these published references. This does not replace compiling against the pinned 2144 library in Onshape.
