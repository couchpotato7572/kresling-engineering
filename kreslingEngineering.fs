FeatureScript 2144;
import(path : "onshape/std/common.fs", version : "2144.0");

// Exact triangle cut nets and thick-panel design states. See ENGINEERING_GUIDE.md.
// All internal geometry uses unitless millimetres.

export enum KMode
{
    annotation { "Name" : "Folded solid" }
    FOLDED,
    annotation { "Name" : "Printable net" }
    NET
}

export enum KSize
{
    annotation { "Name" : "Circumdiameter" }
    DIAMETER,
    annotation { "Name" : "Polygon perimeter" }
    CIRCUMFERENCE
}

export enum KHeight
{
    annotation { "Name" : "Overall reference height" }
    TOTAL,
    annotation { "Name" : "Reference height per cell" }
    CELL
}

export enum KProfile
{
    annotation { "Name" : "Constant" }
    CONSTANT,
    annotation { "Name" : "Linear taper" }
    LINEAR,
    annotation { "Name" : "Progressive taper" }
    POWER,
    annotation { "Name" : "Custom cell schedule" }
    CUSTOM
}

export enum KCaps
{
    annotation { "Name" : "Open ends" }
    NONE,
    annotation { "Name" : "Bottom only" }
    BOTTOM,
    annotation { "Name" : "Top only" }
    TOP,
    annotation { "Name" : "Both ends" }
    BOTH
}

export enum KHinge
{
    annotation { "Name" : "Specify hinge thickness" }
    THICKNESS,
    annotation { "Name" : "Specify crease depth" }
    DEPTH
}

annotation { "Feature Type Name" : "Kresling Engineering",
    "Feature Type Description" : "Faceted design states and exact cut nets; not a rigid-folding simulation" }
export const kreslingEngineering = defineFeature(function(context is Context, id is Id, definition is map)
precondition
{
    annotation { "Group Name" : "Generation Mode", "Collapsed By Default" : false }
    {
        annotation { "Name" : "Generate", "Default" : KMode.FOLDED }
        definition.mode is KMode;
        annotation { "Name" : "Crease guide sketches in net", "Default" : true }
        definition.guides is boolean;
    }
    annotation { "Group Name" : "Basic Dimensions", "Collapsed By Default" : false }
    {
        annotation { "Name" : "Polygon sides" }
        isInteger(definition.sides, { (unitless) : [3, 6, 16] } as IntegerBoundSpec);
        annotation { "Name" : "Cross-section measurement", "Default" : KSize.DIAMETER }
        definition.sizeMode is KSize;
        if (definition.sizeMode == KSize.DIAMETER)
        {
        annotation { "Name" : "Bottom circumdiameter" }
        isLength(definition.diameter, { (millimeter) : [12, 60, 600] } as LengthBoundSpec);
        }
        else
        {
        annotation { "Name" : "Bottom polygon perimeter" }
        isLength(definition.perimeter, { (millimeter) : [30, 180, 2000] } as LengthBoundSpec);
        }
        annotation { "Name" : "Reference height input", "Default" : KHeight.CELL }
        definition.heightMode is KHeight;
        if (definition.heightMode == KHeight.TOTAL)
        {
        annotation { "Name" : "Overall reference height" }
        isLength(definition.totalHeight, { (millimeter) : [5, 90, 1500] } as LengthBoundSpec);
        }
        else
        {
        annotation { "Name" : "Reference cell height" }
        isLength(definition.cellHeight, { (millimeter) : [5, 30, 300] } as LengthBoundSpec);
        }
    }
    annotation { "Group Name" : "Kresling Geometry", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Cell twist / folding angle" }
        isAngle(definition.twist, { (degree) : [1, 75, 179] } as AngleBoundSpec);
        annotation { "Name" : "Clockwise viewed from +Z", "Default" : false }
        definition.clockwise is boolean;
        annotation { "Name" : "End plates", "Default" : KCaps.NONE }
        definition.caps is KCaps;
        if (definition.caps != KCaps.NONE)
        {
        annotation { "Name" : "End plate thickness" }
        isLength(definition.plateT, { (millimeter) : [0.4, 2, 10] } as LengthBoundSpec);
        annotation { "Name" : "End plate center hole diameter" }
        isLength(definition.holeD, { (millimeter) : [0, 0, 300] } as LengthBoundSpec);
        }
    }
    annotation { "Group Name" : "Folding and Compression", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Design compression (0 to 0.65)", "Description" : "Redesigns triangle dimensions; does not simulate folding a fixed printed net." }
        isReal(definition.compression, { (unitless) : [0, 0, 0.65] } as RealBoundSpec);
        annotation { "Name" : "Allowed minimum compression" }
        isReal(definition.minCompression, { (unitless) : [0, 0, 0.65] } as RealBoundSpec);
        annotation { "Name" : "Allowed maximum compression" }
        isReal(definition.maxCompression, { (unitless) : [0, 0.5, 0.65] } as RealBoundSpec);
    }
    annotation { "Group Name" : "Tiling and Repetition", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Number of cells" }
        isInteger(definition.cells, { (unitless) : [1, 1, 8] } as IntegerBoundSpec);
        annotation { "Name" : "Alternate twist direction", "Default" : false }
        definition.alternate is boolean;
        annotation { "Name" : "Seam vertex (zero based)" }
        isInteger(definition.seam, { (unitless) : [0, 0, 15] } as IntegerBoundSpec);
    }
    annotation { "Group Name" : "Tapering and Transitions", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Cross-section profile", "Default" : KProfile.CONSTANT }
        definition.profile is KProfile;
        if (definition.profile == KProfile.LINEAR || definition.profile == KProfile.POWER)
        {
        annotation { "Name" : "Top circumdiameter" }
        isLength(definition.topD, { (millimeter) : [12, 45, 600] } as LengthBoundSpec);
        }
        if (definition.profile == KProfile.POWER)
        {
        annotation { "Name" : "Taper exponent" }
        isReal(definition.exponent, { (unitless) : [0.25, 2, 4] } as RealBoundSpec);
        }
        if (definition.profile != KProfile.CUSTOM)
        {
        annotation { "Name" : "Top center X offset" }
        isLength(definition.offsetX, { (millimeter) : [-200, 0, 200] } as LengthBoundSpec);
        annotation { "Name" : "Top center Y offset" }
        isLength(definition.offsetY, { (millimeter) : [-200, 0, 200] } as LengthBoundSpec);
        }
        else
        {
            annotation { "Name" : "Cells from bottom to top", "Item name" : "Cell", "UIHint" : UIHint.COLLAPSE_ARRAY_ITEMS }
            definition.schedule is array;
            for (var cell in definition.schedule)
            {
                annotation { "Name" : "Reference cell height" }
                isLength(cell.h, { (millimeter) : [5, 30, 300] } as LengthBoundSpec);
                annotation { "Name" : "Top circumdiameter" }
                isLength(cell.d, { (millimeter) : [12, 60, 600] } as LengthBoundSpec);
                annotation { "Name" : "Twist magnitude" }
                isAngle(cell.cellTwist, { (degree) : [1, 75, 179] } as AngleBoundSpec);
                annotation { "Name" : "Top center X (absolute)" }
                isLength(cell.x, { (millimeter) : [-200, 0, 200] } as LengthBoundSpec);
                annotation { "Name" : "Top center Y (absolute)" }
                isLength(cell.y, { (millimeter) : [-200, 0, 200] } as LengthBoundSpec);
            }
        }
    }
    annotation { "Group Name" : "Hinge and Material Settings", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Panel thickness" }
        isLength(definition.panelT, { (millimeter) : [0.4, 1.2, 8] } as LengthBoundSpec);
        annotation { "Name" : "Hinge thickness input", "Default" : KHinge.THICKNESS }
        definition.hingeControl is KHinge;
        if (definition.hingeControl == KHinge.THICKNESS)
        {
        annotation { "Name" : "Hinge web thickness" }
        isLength(definition.hingeT, { (millimeter) : [0.15, 0.4, 2] } as LengthBoundSpec);
        }
        else
        {
        annotation { "Name" : "Crease depth from panel thickness" }
        isLength(definition.creaseDepth, { (millimeter) : [0.1, 0.8, 7] } as LengthBoundSpec);
        }
        annotation { "Name" : "Minimum flexible span across crease" }
        isLength(definition.hingeWidth, { (millimeter) : [0.6, 1.6, 12] } as LengthBoundSpec);
        annotation { "Name" : "Hinge-to-panel overlap" }
        isLength(definition.overlap, { (millimeter) : [0.3, 0.8, 4] } as LengthBoundSpec);
        annotation { "Name" : "Invert web sides", "Default" : false }
        definition.invertHinges is boolean;
    }
    annotation { "Group Name" : "Printing and Manufacturing", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Minimum total crease clearance" }
        isLength(definition.gap, { (millimeter) : [0.2, 0.6, 8] } as LengthBoundSpec);
        annotation { "Name" : "Extra thickness clearance" }
        isLength(definition.clearance, { (millimeter) : [0.05, 0.2, 2] } as LengthBoundSpec);
        annotation { "Name" : "Minimum printable wall" }
        isLength(definition.minWall, { (millimeter) : [0.3, 0.8, 3] } as LengthBoundSpec);
        annotation { "Name" : "Minimum printable web" }
        isLength(definition.minHinge, { (millimeter) : [0.1, 0.3, 1] } as LengthBoundSpec);
        annotation { "Name" : "Minimum hinge end relief" }
        isLength(definition.relief, { (millimeter) : [0.4, 1, 6] } as LengthBoundSpec);
        annotation { "Name" : "Round panel outline corners", "Default" : false }
        definition.roundPanels is boolean;
        if (definition.roundPanels)
        {
        annotation { "Name" : "Panel corner radius" }
        isLength(definition.cornerR, { (millimeter) : [0.1, 0.3, 2] } as LengthBoundSpec);
        }
        annotation { "Name" : "Net rotation on print bed" }
        isAngle(definition.netAngle, { (degree) : [-180, 0, 180] } as AngleBoundSpec);
    }
    annotation { "Group Name" : "Splint Integration Features", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Panel ventilation hole diameter (0 = none)" }
        isLength(definition.ventD, { (millimeter) : [0, 0, 20] } as LengthBoundSpec);
        annotation { "Name" : "Minimum hole / slot ligament" }
        isLength(definition.ligament, { (millimeter) : [0.5, 2, 8] } as LengthBoundSpec);
        annotation { "Name" : "Interior panel reinforcement islands", "Default" : false }
        definition.ribs is boolean;
        if (definition.ribs)
        {
        annotation { "Name" : "Reinforcement height" }
        isLength(definition.ribH, { (millimeter) : [0.3, 0.8, 4] } as LengthBoundSpec);
        annotation { "Name" : "Reinforcement setback from panel edges" }
        isLength(definition.ribInset, { (millimeter) : [1, 3, 12] } as LengthBoundSpec);
        }
        if (definition.caps == KCaps.NONE)
        {
        annotation { "Name" : "One slotted attachment tab at each end", "Default" : false }
        definition.tabs is boolean;
            if (definition.tabs)
            {
        annotation { "Name" : "Tab width along end edge" }
        isLength(definition.tabW, { (millimeter) : [6, 16, 40] } as LengthBoundSpec);
        annotation { "Name" : "Tab extension" }
        isLength(definition.tabL, { (millimeter) : [5, 10, 30] } as LengthBoundSpec);
        annotation { "Name" : "Strap slot opening" }
        isLength(definition.slotOpening, { (millimeter) : [1, 2, 6] } as LengthBoundSpec);
            }
        }
    }
}
{
    const mm = millimeter;
    const n = definition.sides;
    const count = definition.cells;
    const alpha = 2 * PI / n;
    if (definition.seam >= n)
        throw regenError("Seam vertex must be smaller than the polygon side count.", ["seam"]);
    if (definition.minCompression > definition.maxCompression ||
        definition.compression < definition.minCompression || definition.compression > definition.maxCompression)
        throw regenError("Compression must lie between the selected minimum and maximum.",
                         ["compression", "minCompression", "maxCompression"]);
    if (definition.profile == KProfile.CUSTOM && size(definition.schedule) != count)
        throw regenError("The custom schedule must contain exactly one entry per cell.", ["schedule", "cells"]);

    const panelT = definition.panelT / mm;
    const plateT = definition.caps == KCaps.NONE ? panelT : definition.plateT / mm;
    const hingeT = definition.hingeControl == KHinge.THICKNESS ?
                   definition.hingeT / mm : panelT - definition.creaseDepth / mm;
    const overlap = definition.overlap / mm;
    const clearance = definition.clearance / mm;
    const halfGap = max(definition.gap / mm, definition.hingeWidth / mm) / 2;
    const ligament = definition.ligament / mm;
    const ventR = definition.ventD / mm / 2;
    const holeR = definition.caps == KCaps.NONE ? 0 : definition.holeD / mm / 2;
    const cornerR = definition.roundPanels ? definition.cornerR / mm : 0;
    if (panelT < definition.minWall / mm || plateT < definition.minWall / mm)
        throw regenError("Panel and end plate thickness must meet the printable wall minimum.", ["panelT", "plateT", "minWall"]);
    if (hingeT < definition.minHinge / mm || hingeT > 0.8 * min(panelT, plateT))
        throw regenError("Web thickness must meet the printable minimum and be at most 80% of each adjoining wall.",
                         ["hingeT", "creaseDepth", "minHinge"]);
    if (cornerR > overlap / 2)
        throw regenError("Use a panel corner radius no greater than half the hinge overlap.", ["cornerR", "overlap"]);

    const baseR = definition.sizeMode == KSize.DIAMETER ? definition.diameter / mm / 2 :
                  definition.perimeter / mm / (2 * n * sin(alpha / 2 * radian));
    const referenceH = definition.heightMode == KHeight.TOTAL ? definition.totalHeight / mm / count : definition.cellHeight / mm;
    const hand = definition.clockwise ? -1 : 1;
    var vertices = [];
    var centers = [vector(0, 0, 0)];
    var radii = [baseR];
    var signs = [];
    var angle = 0;
    var z = 0;
    for (var ring = 0; ring <= count; ring += 1)
    {
        if (ring > 0)
        {
            const k = ring - 1;
            const sg = hand * ((definition.alternate && k % 2 == 1) ? -1 : 1);
            signs = append(signs, sg);
            var r = baseR;
            var cx = 0;
            var cy = 0;
            var h = referenceH;
            var phi = definition.twist / radian;
            if (definition.profile == KProfile.CUSTOM)
            {
                const row = definition.schedule[k];
                r = row.d / mm / 2;
                h = row.h / mm;
                phi = row.cellTwist / radian;
                cx = row.x / mm;
                cy = row.y / mm;
            }
            else
            {
                const fraction = ring / count;
                if (definition.profile != KProfile.CONSTANT)
                {
                    const exponent = definition.profile == KProfile.POWER ? definition.exponent : 1;
                    r = baseR + (definition.topD / mm / 2 - baseR) * fraction ^ exponent;
                }
                cx = definition.offsetX / mm * fraction;
                cy = definition.offsetY / mm * fraction;
            }
            if (phi <= 0 || phi >= PI - 1e-5)
                throw regenError("Each cell twist must be positive and smaller than 180 degrees.", ["twist", "schedule"]);
            h *= 1 - definition.compression;
            if (h < 4 * max(panelT, plateT))
                throw regenError("Compressed cell height must exceed four wall thicknesses.", ["compression", "cellHeight", "schedule"]);
            const previous = centers[k];
            if (norm(vector(cx - previous[0], cy - previous[1], 0)) > 0.5 * min(r, radii[k]))
                throw regenError("Center offset per cell exceeds half the smaller ring radius. Add cells or reduce the offset.", ["offsetX", "offsetY", "schedule"]);
            z += h;
            angle += sg * phi;
            centers = append(centers, vector(cx, cy, z));
            radii = append(radii, r);
        }
        for (var j = 0; j < n; j += 1)
        {
            const a = angle + alpha * j;
            vertices = append(vertices, centers[ring] + radii[ring] * vector(cos(a * radian), sin(a * radian), 0));
        }
    }

    // Preserve the original two-triangle construction. Mirror the diagonal as
    // well as the twist for a handed cell; shared ring vertices remain identical.
    var faces = [];
    for (var k = 0; k < count; k += 1)
    {
        const inside = (centers[k] + centers[k + 1]) / 2;
        for (var i = 0; i < n; i += 1)
        {
            const a = (definition.seam + signs[k] * i + 2 * n) % n;
            const b = (definition.seam + signs[k] * (i + 1) + 2 * n) % n;
            const idsL = [k * n + a, k * n + b, (k + 1) * n + a];
            const idsU = [(k + 1) * n + a, (k + 1) * n + b, k * n + b];
            faces = append(faces, kFace(idsL, vertices, panelT, inside, k, 2 * i, false));
            faces = append(faces, kFace(idsU, vertices, panelT, inside, k, 2 * i + 1, false));
        }
    }
    if (definition.caps == KCaps.BOTTOM || definition.caps == KCaps.BOTH)
        faces = append(faces, kCap(vertices, n, 0, plateT, centers[0] + vector(0, 0, 1)));
    if (definition.caps == KCaps.TOP || definition.caps == KCaps.BOTH)
        faces = append(faces, kCap(vertices, n, count, plateT, centers[count] - vector(0, 0, 1)));

    // Build adjacency once using vertex identities, rather than comparing every
    // face pair. A dual spanning tree seeds BOTH output modes; compatible
    // inter-cell cycle edges are restored after exact development.
    var edgeTable = {};
    var creases = [];
    var gaps = [];
    for (var f = 0; f < size(faces); f += 1)
    {
        const face = faces[f];
        gaps = append(gaps, makeArray(size(face.pts), 0));
        for (var e = 0; e < size(face.ids); e += 1)
        {
            const v0 = face.ids[e];
            const v1 = face.ids[(e + 1) % size(face.ids)];
            const key = "v" ~ min(v0, v1) ~ "_" ~ max(v0, v1);
            if (edgeTable[key] == undefined)
                edgeTable[key] = { "a" : f, "ia" : e };
            else
            {
                var cr = edgeTable[key];
                cr.b = f;
                cr.ib = e;
                const other = faces[cr.a];
                var keep = false;
                if (face.cap || other.cap)
                    keep = face.cap ? e == 0 : cr.ia == 0;
                else if (face.cell == other.cell)
                    keep = abs(face.order - other.order) == 1;
                else
                {
                    // One bridge between cells, on the physical edge following
                    // the seam vertex counterclockwise, regardless of handedness.
                    const q0 = v0 % n;
                    const q1 = v1 % n;
                    const s0 = definition.seam;
                    const s1 = (s0 + 1) % n;
                    keep = (q0 == s0 && q1 == s1) || (q0 == s1 && q1 == s0);
                }
                cr.keep = keep;
                const uA = kEdgeInward(other.pts, cr.ia, other.c);
                const uB = kEdgeInward(face.pts, e, face.c);
                const ridge = dot(other.n, uB) + dot(face.n, uA) > 1e-8;
                var gA = halfGap;
                var gB = halfGap;
                if (ridge)
                {
                    const shift = kOffset(other.n, face.n, other.t, face.t);
                    gA = max(gA, dot(shift, uA) + clearance / 2);
                    gB = max(gB, dot(shift, uB) + clearance / 2);
                }
                gaps[cr.a][cr.ia] = gA;
                gaps[f][e] = gB;
                cr.outer = ridge != definition.invertHinges;
                creases = append(creases, cr);
            }
        }
    }
    // Edge-only offsets miss contacts between a thick end plate and a side
    // triangle meeting it at just one vertex. Clear the complete plate slab.
    if (definition.caps != KCaps.NONE)
    {
        for (var i = 0; i < size(faces); i += 1)
        {
            if (faces[i].cap)
                continue;
            if (faces[i].cell == 0 && (definition.caps == KCaps.BOTTOM || definition.caps == KCaps.BOTH))
                gaps[i] = kClearPlate(faces[i], gaps[i], plateT + clearance / 2, true);
            if (faces[i].cell == count - 1 && (definition.caps == KCaps.TOP || definition.caps == KCaps.BOTH))
                gaps[i] = kClearPlate(faces[i], gaps[i], z - plateT - clearance / 2, false);
        }
    }
    var links = [];
    for (var cr in creases)
        if (cr.keep)
            links = append(links, cr);
    if (size(links) != size(faces) - 1)
        throw regenError("Internal cut graph is not a spanning tree.");

    // Restore additional inter-cell hinges only when the exact development
    // closes those edges. Constant untapered rows normally close every ring
    // edge; tapered or asymmetric rows may retain fewer bridges.
    const flatFaces = kUnfold(faces, links);
    for (var cr in creases)
    {
        if (cr.keep || faces[cr.a].cap || faces[cr.b].cap || faces[cr.a].cell == faces[cr.b].cell)
            continue;
        if (kFlatEdgeCloses(flatFaces[cr.a], cr.ia, flatFaces[cr.b]))
            links = append(links, cr);
    }
    if (definition.mode == KMode.NET)
    {
        faces = flatFaces;
        for (var a = 0; a < size(faces); a += 1)
            for (var b = a + 1; b < size(faces); b += 1)
                if (kOverlap2d(faces[a].pts, faces[b].pts))
                    throw regenError("The exact cut net overlaps between faces " ~ (a + 1) ~ " and " ~ (b + 1) ~
                                     ". Change seam, twist or profile; alternatively export cells separately.", ["seam", "twist", "profile"]);
        faces = kPlaceNet(faces, definition.netAngle / radian);
    }

    // Validate the complete inset polygon against EVERY original half-plane.
    // This detects inverted/empty insets, which the original script did not.
    var panelQueries = [];
    for (var i = 0; i < size(faces); i += 1)
    {
        const f = faces[i];
        const shape = kInset(f.pts, gaps[i], f.c);
        const panelId = id + ("panel" ~ i);
        kExtrudePoly(context, panelId + "base", shape, f.n, f.t);
        if (definition.roundPanels)
        {
            var edges = [];
            for (var edge in evaluateQuery(context, qCreatedBy(panelId + "base" + "ext", EntityType.EDGE)))
            {
                const tangent = evEdgeTangentLine(context, { "edge" : edge, "parameter" : 0.5 });
                if (abs(dot(tangent.direction, f.n)) > 0.999999)
                    edges = append(edges, edge);
            }
            opFillet(context, panelId + "corners", { "entities" : qUnion(edges), "radius" : cornerR * mm });
        }
        var pieces = [qCreatedBy(panelId + "base" + "ext", EntityType.BODY)];
        if (definition.ribs && !f.cap)
        {
            const ribShape = kInset(shape, makeArray(size(shape), definition.ribInset / mm), kCentroid(shape));
            var ribPts = [];
            for (var p in ribShape)
                ribPts = append(ribPts, p + (f.t - 0.1) * f.n);
            kExtrudePoly(context, panelId + "rib", ribPts, f.n, definition.ribH / mm + 0.1);
            pieces = append(pieces, qCreatedBy(panelId + "rib" + "ext", EntityType.BODY));
        }
        // Optional tabs attach only to open end edges, one tab per end.
        if (definition.caps == KCaps.NONE && definition.tabs)
        {
            for (var e = 0; e < size(f.ids); e += 1)
            {
                const v0 = f.ids[e];
                const v1 = f.ids[(e + 1) % size(f.ids)];
                const bottomEdge = v0 < n && v1 < n;
                const topEdge = v0 >= count * n && v1 >= count * n;
                const q0 = v0 % n;
                const q1 = v1 % n;
                if ((bottomEdge || topEdge) &&
                    ((q0 == definition.seam && q1 == (definition.seam + 1) % n) ||
                     (q1 == definition.seam && q0 == (definition.seam + 1) % n)))
                {
                    kTab(context, panelId + "tab", f, e, definition, ligament, overlap);
                    pieces = append(pieces, qCreatedBy(panelId + "tab" + "ext", EntityType.BODY));
                }
            }
        }
        if (size(pieces) > 1)
            opBoolean(context, panelId + "merge", { "tools" : qUnion(pieces), "operationType" : BooleanOperationType.UNION });
        const panel = qBodyType(qCreatedBy(panelId, EntityType.BODY), BodyType.SOLID);
        const r = f.cap ? holeR : ventR;
        if (r > 0)
        {
            const center = kCentroid(shape);
            for (var e = 0; e < size(shape); e += 1)
                if (dot(center - shape[e], kEdgeInward(shape, e, center)) < r + ligament + cornerR)
                    throw regenError("Hole leaves insufficient edge ligament on face " ~ (i + 1) ~ ". Reduce its diameter.", ["ventD", "holeD", "ligament"]);
            const depth = f.t + (definition.ribs && !f.cap ? definition.ribH / mm : 0);
            kCircleCut(context, panelId + "hole", panel, center, f.n, r, depth);
        }
        if (size(evaluateQuery(context, panel)) != 1)
            throw regenError("Panel reinforcement or attachment did not form one solid. Reduce accessory dimensions.");
        panelQueries = append(panelQueries, panel);
    }
    // Exact solid contact checks at this DESIGN STATE, not a motion sweep.
    const panels = qUnion(panelQueries);
    for (var i = 0; i < size(panelQueries) - 1; i += 1)
    {
        var later = [];
        for (var j = i + 1; j < size(panelQueries); j += 1)
            later = append(later, panelQueries[j]);
        if (size(evCollision(context, { "tools" : panelQueries[i], "targets" : qUnion(later) })) > 0)
            throw regenError("Panels or accessories contact outside a hinge. Increase clearance, reduce compression or simplify the profile.",
                             ["gap", "compression", "profile"]);
    }

    var hingeQueries = [];
    var guideData = [];
    for (var k = 0; k < size(links); k += 1)
    {
        const cr = links[k];
        const fA = faces[cr.a];
        const fB = faces[cr.b];
        const P0 = fA.pts[cr.ia];
        const P1 = fA.pts[(cr.ia + 1) % size(fA.pts)];
        const ev = normalize(P1 - P0);
        const uA = kEdgeInward(fA.pts, cr.ia, fA.c);
        const uB = kEdgeInward(fB.pts, cr.ib, fB.c);
        const gA = gaps[cr.a][cr.ia];
        const gB = gaps[cr.b][cr.ib];
        var interval = [definition.relief / mm + cornerR, norm(P1 - P0) - definition.relief / mm - cornerR];
        interval = kClipAttachment(interval, fA, cr.ia, gaps[cr.a], P0, ev, uA, gA, overlap, cornerR);
        interval = kClipAttachment(interval, fB, cr.ib, gaps[cr.b], P0, ev, uB, gB, overlap, cornerR);
        if (interval[1] - interval[0] < max(1, 3 * hingeT))
            throw regenError("A crease has insufficient hinge attachment length. Increase cell size or reduce gaps, overlap or end relief.",
                             ["hingeWidth", "overlap", "relief"]);
        const Pw = P0 + interval[0] * ev;
        var poly = [];
        if (cr.outer)
        {
            const pA = Pw + (gA + overlap) * uA;
            const pB = Pw + (gB + overlap) * uB;
            const middle = Pw + kOffset(fA.n, fB.n, hingeT, hingeT);
            poly = [pA, Pw, pB, pB + hingeT * fB.n, middle, pA + hingeT * fA.n];
        }
        else if (definition.mode == KMode.NET)
        {
            // A sloped inner web joins unequal plate/panel thicknesses in a net.
            const pA = Pw + (gA + overlap) * uA + fA.t * fA.n;
            const pB = Pw + (gB + overlap) * uB + fB.t * fB.n;
            const middle = Pw + (fA.t + fB.t) / 2 * fA.n;
            poly = [pA, middle, pB, pB - hingeT * fB.n, middle - hingeT * fA.n, pA - hingeT * fA.n];
        }
        else
        {
            const Pi = Pw + kOffset(fA.n, fB.n, fA.t, fB.t);
            const wA = gA + overlap - dot(Pi - Pw, uA);
            const wB = gB + overlap - dot(Pi - Pw, uB);
            const pA = Pi + wA * uA;
            const pB = Pi + wB * uB;
            const middle = Pw + kOffset(fA.n, fB.n, fA.t - hingeT, fB.t - hingeT);
            poly = [pA, Pi, pB, pB - hingeT * fB.n, middle, pA - hingeT * fA.n];
        }
        const hingeId = id + ("hinge" ~ k);
        kExtrudePoly(context, hingeId, poly, ev, interval[1] - interval[0]);
        const web = qCreatedBy(hingeId + "ext", EntityType.BODY);
        var forbidden = [];
        for (var j = 0; j < size(panelQueries); j += 1)
            if (j != cr.a && j != cr.b)
                forbidden = append(forbidden, panelQueries[j]);
        forbidden = concatenateArrays([forbidden, hingeQueries]);
        if (size(forbidden) > 0 && size(evCollision(context, { "tools" : web, "targets" : qUnion(forbidden) })) > 0)
            throw regenError("A hinge contacts a different panel or hinge. Increase end relief or change geometry.", ["relief", "compression", "hingeWidth"]);
        if (!kHasVolumeContact(context, web, panelQueries[cr.a]) || !kHasVolumeContact(context, web, panelQueries[cr.b]))
            throw regenError("A hinge does not overlap both panels. Increase overlap or reduce panel corner radius.", ["overlap", "cornerR"]);
        hingeQueries = append(hingeQueries, web);
        guideData = append(guideData, { "a" : P0, "b" : P1, "mountain" : cr.outer != definition.invertHinges });
    }
    opBoolean(context, id + "union", {
        "tools" : qUnion([panels, qUnion(hingeQueries)]),
        "operationType" : BooleanOperationType.UNION
    });
    const result = qBodyType(qCreatedBy(id, EntityType.BODY), BodyType.SOLID);
    if (size(evaluateQuery(context, result)) != 1)
        throw regenError("The result is disconnected. No printable part was accepted.");
    setProperty(context, { "entities" : result, "propertyType" : PropertyType.NAME,
                           "value" : definition.mode == KMode.NET ? "Kresling printable cut net" : "Kresling folded design state" });
    if (definition.mode == KMode.NET && definition.guides)
        kGuides(context, id + "guides", guideData, max(panelT, plateT) + (definition.ribs ? definition.ribH / mm : 0) + 0.5);
    if (definition.compression > 0)
        reportFeatureWarning(context, id, "Compression changes panel dimensions. Regenerate the matching net; this is not a rigid-folding simulation.");
}
);

// ---------------------------------------------------------------------------
// Geometry helpers. These retain and validate the original inset/extrude model.
// ---------------------------------------------------------------------------
function kCentroid(pts is array) returns Vector
{
    var s = vector(0, 0, 0);
    for (var p in pts)
        s += p;
    return s / size(pts);
}

function kEdgeInward(pts is array, e is number, c is Vector) returns Vector
{
    const p = pts[e];
    const v = pts[(e + 1) % size(pts)] - p;
    if (norm(v) < 1e-6)
        throw regenError("A polygon edge is too short.");
    const unit = normalize(v);
    const w = c - p;
    const u = w - dot(w, unit) * unit;
    if (norm(u) < 1e-6)
        throw regenError("A face is degenerate. Increase height or change twist.");
    return normalize(u);
}

function kFace(ids is array, vertices is array, t is number, interior is Vector,
               cell is number, order is number, cap is boolean) returns map
{
    var pts = [];
    for (var v in ids)
        pts = append(pts, vertices[v]);
    const c = kCentroid(pts);
    const crossV = cross(pts[1] - pts[0], pts[2] - pts[0]);
    if (norm(crossV) < 1e-5)
        throw regenError("A reference face has zero area.");
    var n = normalize(crossV);
    if (dot(n, interior - c) < 0)
        n = -n;
    return { "pts" : pts, "ids" : ids, "n" : n, "c" : c, "t" : t,
             "cell" : cell, "order" : order, "cap" : cap };
}

function kCap(vertices is array, n is number, ring is number, t is number, interior is Vector) returns map
{
    var ids = [];
    for (var i = 0; i < n; i += 1)
        ids = append(ids, ring * n + i);
    return kFace(ids, vertices, t, interior, -1, -1, true);
}

function kOffset(nA is Vector, nB is Vector, dA is number, dB is number) returns Vector
{
    const c = dot(nA, nB);
    const den = 1 - c * c;
    if (den < 1e-8)
    {
        if (c < 0 || abs(dA - dB) > 1e-6)
            throw regenError("Adjacent planes have an ill-conditioned thickness offset. Change twist, height or thickness.");
        return dA * nA;
    }
    return ((dA - c * dB) / den) * nA + ((dB - c * dA) / den) * nB;
}

function kInset(pts is array, gaps is array, c is Vector) returns array
{
    var out = [];
    const m = size(pts);
    for (var i = 0; i < m; i += 1)
    {
        const e0 = (i + m - 1) % m;
        const u0 = kEdgeInward(pts, e0, c);
        const u1 = kEdgeInward(pts, i, c);
        const cs = dot(u0, u1);
        const den = 1 - cs * cs;
        if (den < 1e-9)
            throw regenError("Nearly collinear polygon corners cannot be inset reliably.");
        const p = pts[i] + ((gaps[e0] - cs * gaps[i]) / den) * u0 +
                          ((gaps[i] - cs * gaps[e0]) / den) * u1;
        out = append(out, p);
    }
    for (var p in out)
        for (var e = 0; e < m; e += 1)
            if (dot(p - pts[e], kEdgeInward(pts, e, c)) < gaps[e] - 1e-6)
                throw regenError("Panel inset collapses. Increase cell dimensions or reduce wall thickness and hinge span.");
    for (var i = 0; i < m; i += 1)
        if (norm(out[(i + 1) % m] - out[i]) < 0.2)
            throw regenError("An inset leaves an edge shorter than 0.2 mm. Reduce the inset.");
    if (norm(cross(out[1] - out[0], out[2] - out[0])) < 0.05)
        throw regenError("An inset leaves insufficient panel area.");
    return out;
}

// Uniform extra insets are affine in their setback distance. Solve the
// clearance of every prism vertex, including the panel thickness in Z.
function kClearPlate(face is map, gaps is array, limit is number, bottom is boolean) returns array
{
    const shape = kInset(face.pts, gaps, face.c);
    // Use the linear corner formula without validating the temporary +1 inset.
    var shifted = [];
    const m = size(face.pts);
    for (var i = 0; i < m; i += 1)
    {
        const u0 = kEdgeInward(face.pts, (i + m - 1) % m, face.c);
        const u1 = kEdgeInward(face.pts, i, face.c);
        shifted = append(shifted, (u0 + u1) / (1 + dot(u0, u1)));
    }
    var extra = 0;
    const direction = bottom ? 1 : -1;
    const normalShift = bottom ? min(0, face.t * face.n[2]) : max(0, face.t * face.n[2]);
    for (var i = 0; i < m; i += 1)
    {
        const need = direction * (limit - shape[i][2] - normalShift);
        if (need > 1e-7)
        {
            const slope = direction * shifted[i][2];
            if (slope <= 1e-8)
                throw regenError("End plate clearance cannot be obtained by an inset. Reduce plate thickness or change the cell geometry.");
            extra = max(extra, need / slope);
        }
    }
    for (var i = 0; i < m; i += 1)
        gaps[i] += extra;
    kInset(face.pts, gaps, face.c);
    return gaps;
}

function kFlatEdgeCloses(a is map, edge is number, b is map) returns boolean
{
    for (var ia in [edge, (edge + 1) % size(a.ids)])
    {
        var found = false;
        for (var ib = 0; ib < size(b.ids); ib += 1)
            if (a.ids[ia] == b.ids[ib])
            {
                found = norm(a.pts[ia] - b.pts[ib]) < 1e-6;
                break;
            }
        if (!found)
            return false;
    }
    return true;
}

function kUnfold(faces is array, links is array) returns array
{
    var flat = faces;
    const root = faces[0];
    const origin = root.pts[0];
    const x = normalize(root.pts[1] - origin);
    const y = cross(root.n, x);
    var pts = [];
    for (var p in root.pts)
        pts = append(pts, vector(dot(p - origin, x), dot(p - origin, y), 0));
    flat[0].pts = pts;
    flat[0].c = kCentroid(pts);
    flat[0].n = vector(0, 0, 1);
    var visited = makeArray(size(faces), false);
    visited[0] = true;
    var queue = [0];
    for (var head = 0; head < size(queue); head += 1)
    {
        const a = queue[head];
        for (var link in links)
        {
            if (link.a != a && link.b != a)
                continue;
            const b = link.a == a ? link.b : link.a;
            if (visited[b])
                continue;
            const ea = link.a == a ? link.ia : link.ib;
            const eb = link.a == a ? link.ib : link.ia;
            const original = faces[a];
            const p0 = original.pts[ea];
            const p1 = original.pts[(ea + 1) % size(original.pts)];
            const ew = normalize(p1 - p0);
            const uw = kEdgeInward(faces[b].pts, eb, faces[b].c);
            const q0 = flat[a].pts[ea];
            const q1 = flat[a].pts[(ea + 1) % size(original.pts)];
            const ef = normalize(q1 - q0);
            const uf = -kEdgeInward(flat[a].pts, ea, flat[a].c);
            var next = [];
            for (var p in faces[b].pts)
                next = append(next, q0 + dot(p - p0, ew) * ef + dot(p - p0, uw) * uf);
            flat[b].pts = next;
            flat[b].c = kCentroid(next);
            flat[b].n = vector(0, 0, 1);
            visited[b] = true;
            queue = append(queue, b);
        }
    }
    if (size(queue) != size(faces))
        throw regenError("The unfolding graph is disconnected.");
    return flat;
}

// Strict interior overlap test for convex polygons. Shared boundaries are OK.
function kOverlap2d(a is array, b is array) returns boolean
{
    // Cheap bounding-box rejection before the separating-axis test.
    for (var coordinate in [0, 1])
    {
        var amin = a[0][coordinate];
        var amax = amin;
        var bmin = b[0][coordinate];
        var bmax = bmin;
        for (var p in a)
        {
            amin = min(amin, p[coordinate]);
            amax = max(amax, p[coordinate]);
        }
        for (var p in b)
        {
            bmin = min(bmin, p[coordinate]);
            bmax = max(bmax, p[coordinate]);
        }
        if (min(amax, bmax) - max(amin, bmin) <= 1e-6)
            return false;
    }
    for (var poly in [a, b])
    {
        for (var i = 0; i < size(poly); i += 1)
        {
            const d = poly[(i + 1) % size(poly)] - poly[i];
            const axis = normalize(vector(-d[1], d[0], 0));
            var loA = dot(a[0], axis);
            var hiA = loA;
            var loB = dot(b[0], axis);
            var hiB = loB;
            for (var p in a)
            {
                loA = min(loA, dot(p, axis));
                hiA = max(hiA, dot(p, axis));
            }
            for (var p in b)
            {
                loB = min(loB, dot(p, axis));
                hiB = max(hiB, dot(p, axis));
            }
            if (min(hiA, hiB) - max(loA, loB) <= 1e-6)
                return false;
        }
    }
    return true;
}

function kPlaceNet(faces is array, angle is number) returns array
{
    const c = cos(angle * radian);
    const s = sin(angle * radian);
    var xmin = 1e10;
    var ymin = 1e10;
    for (var i = 0; i < size(faces); i += 1)
    {
        var pts = [];
        for (var p in faces[i].pts)
        {
            const q = vector(c * p[0] - s * p[1], s * p[0] + c * p[1], 0);
            xmin = min(xmin, q[0]);
            ymin = min(ymin, q[1]);
            pts = append(pts, q);
        }
        faces[i].pts = pts;
    }
    for (var i = 0; i < size(faces); i += 1)
    {
        var pts = [];
        for (var p in faces[i].pts)
            pts = append(pts, p - vector(xmin, ymin, 0));
        faces[i].pts = pts;
        faces[i].c = kCentroid(pts);
    }
    return faces;
}

// Keep the full attachment strip clear of neighbouring edge setbacks.
function kClipAttachment(interval is array, face is map, crease is number, gaps is array,
                         P0 is Vector, ev is Vector, u is Vector, gap is number,
                         overlap is number, cornerR is number) returns array
{
    for (var e = 0; e < size(face.pts); e += 1)
    {
        if (e == crease)
            continue;
        const inward = kEdgeInward(face.pts, e, face.c);
        const slope = dot(ev, inward);
        for (var width in [gap, gap + overlap])
        {
            const required = gaps[e] + cornerR + overlap + 0.2 - dot(P0 + width * u - face.pts[e], inward);
            if (abs(slope) < 1e-9)
            {
                if (required > 1e-6)
                    throw regenError("A hinge attachment strip does not fit inside a panel.");
            }
            else if (slope > 0)
                interval[0] = max(interval[0], required / slope);
            else
                interval[1] = min(interval[1], required / slope);
        }
    }
    return interval;
}

function kHasVolumeContact(context is Context, a is Query, b is Query) returns boolean
{
    for (var hit in evCollision(context, { "tools" : a, "targets" : b }))
        if (hit["type"] == ClashType.INTERFERE || hit["type"] == ClashType.TARGET_IN_TOOL || hit["type"] == ClashType.TOOL_IN_TARGET)
            return true;
    return false;
}

function kExtrudePoly(context is Context, id is Id, pts is array, dir is Vector, depth is number)
{
    // Remove collinear vertices (flat webs have these) and check planarity.
    var clean = [];
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i] - pts[(i + size(pts) - 1) % size(pts)];
        const b = pts[(i + 1) % size(pts)] - pts[i];
        if (norm(a) < 1e-6 || norm(b) < 1e-6)
            throw regenError("A hinge profile contains a zero-length edge. Change hinge settings.");
        if (norm(cross(a, b)) > 1e-8 * norm(a) * norm(b) || dot(a, b) < 0)
            clean = append(clean, pts[i]);
    }
    if (size(clean) < 3 || depth <= 0)
        throw regenError("An extrusion profile is degenerate.");
    const o = clean[0];
    const xd = normalize(clean[1] - o);
    const nrm = normalize(dir);
    const yd = cross(nrm, xd);
    if (abs(dot(nrm, xd)) > 1e-7)
        throw regenError("Extrusion profile is not perpendicular to its direction.");
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(o * millimeter, nrm, xd) });
    for (var i = 0; i < size(clean); i += 1)
    {
        const p = clean[i] - o;
        const q = clean[(i + 1) % size(clean)] - o;
        if (abs(dot(p, nrm)) > 1e-5)
            throw regenError("An extrusion profile is not planar.");
        skLineSegment(sk, "s" ~ i, {
            "start" : vector(dot(p, xd), dot(p, yd)) * millimeter,
            "end" : vector(dot(q, xd), dot(q, yd)) * millimeter
        });
    }
    skSolve(sk);
    const regions = qSketchRegion(id + "sk");
    if (size(evaluateQuery(context, regions)) != 1)
        throw regenError("A profile self-intersects or does not enclose one region. Change thickness, gap or twist.");
    opExtrude(context, id + "ext", {
        "entities" : regions, "direction" : nrm,
        "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter
    });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

function kCircleCut(context is Context, id is Id, target is Query, center is Vector, normal is Vector, radius is number, depth is number)
{
    const sk = newSketchOnPlane(context, id + "sk", {
        "sketchPlane" : plane((center - 0.1 * normal) * millimeter, normal)
    });
    skCircle(sk, "c", { "center" : vector(0, 0) * millimeter, "radius" : radius * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : normal,
        "endBound" : BoundingType.BLIND, "endDepth" : (depth + 0.2) * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
    opBoolean(context, id + "cut", { "tools" : qCreatedBy(id + "ext", EntityType.BODY),
        "targets" : qSubtraction(target, qCreatedBy(id + "ext", EntityType.BODY)),
        "operationType" : BooleanOperationType.SUBTRACTION });
}

function kTab(context is Context, id is Id, face is map, e is number, definition is map, ligament is number, overlap is number)
{
    const a = face.pts[e];
    const b = face.pts[(e + 1) % size(face.pts)];
    const mid = (a + b) / 2;
    const x = normalize(b - a);
    const y = -kEdgeInward(face.pts, e, face.c);
    const width = definition.tabW / millimeter;
    const length = definition.tabL / millimeter;
    const opening = definition.slotOpening / millimeter;
    if (width + 2 * ligament > norm(b - a) || width <= 2 * ligament || length < opening + 2 * ligament)
        throw regenError("Tab or strap slot does not fit. Allow the selected ligament on every side.", ["tabW", "tabL", "slotOpening", "ligament"]);
    var pts = [];
    for (var xy in [vector(-width / 2, -overlap), vector(width / 2, -overlap), vector(width / 2, length), vector(-width / 2, length)])
        pts = append(pts, mid + xy[0] * x + xy[1] * y);
    kExtrudePoly(context, id, pts, face.n, face.t);
    var slot = [];
    for (var xy in [vector(-width / 2 + ligament, (length - opening) / 2), vector(width / 2 - ligament, (length - opening) / 2),
                   vector(width / 2 - ligament, (length + opening) / 2), vector(-width / 2 + ligament, (length + opening) / 2)])
        slot = append(slot, mid + xy[0] * x + xy[1] * y - 0.1 * face.n);
    kExtrudePoly(context, id + "slot", slot, face.n, face.t + 0.2);
    opBoolean(context, id + "cut", { "tools" : qCreatedBy(id + "slot" + "ext", EntityType.BODY),
        "targets" : qCreatedBy(id + "ext", EntityType.BODY), "operationType" : BooleanOperationType.SUBTRACTION });
}

function kGuides(context is Context, id is Id, data is array, z is number)
{
    for (var mountain in [true, false])
    {
        var hasCrease = false;
        for (var crease in data)
            if (crease.mountain == mountain)
                hasCrease = true;
        if (!hasCrease)
            continue;
        const groupId = id + (mountain ? "mountain" : "valley");
        const sk = newSketchOnPlane(context, groupId, { "sketchPlane" : plane(vector(0, 0, z) * millimeter, vector(0, 0, 1), vector(1, 0, 0)) });
        var index = 0;
        for (var crease in data)
            if (crease.mountain == mountain)
            {
                skLineSegment(sk, "crease" ~ index, { "start" : vector(crease.a[0], crease.a[1]) * millimeter,
                    "end" : vector(crease.b[0], crease.b[1]) * millimeter, "construction" : true });
                index += 1;
            }
        skSolve(sk);
        setProperty(context, { "entities" : qCreatedBy(groupId, EntityType.BODY), "propertyType" : PropertyType.NAME,
                               "value" : mountain ? "MOUNTAIN - outer convex in folded state" : "VALLEY - outer concave in folded state" });
        setProperty(context, { "entities" : qCreatedBy(groupId, EntityType.BODY), "propertyType" : PropertyType.APPEARANCE,
                               "value" : mountain ? color(0.9, 0.15, 0.1) : color(0.1, 0.3, 0.9) });
    }
}
