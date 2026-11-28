FeatureScript 2144;
import(path : "onshape/std/common.fs", version : "2144.0");
import(path : "onshape/std/chamfertype.gen.fs", version : "2144.0");

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

export enum KManufacturing
{
    annotation { "Name" : "Single material" }
    SINGLE,
    annotation { "Name" : "PLA panels + TPU hinges" }
    MULTI,
    annotation { "Name" : "PLA + TPU + slicer modifiers" }
    MULTI_MODIFIERS,
    annotation { "Name" : "Independent single-material modules" }
    MODULAR
}

export enum KCreaseSelection
{
    annotation { "Name" : "All retained creases" }
    ALL,
    annotation { "Name" : "Mountain folds" }
    MOUNTAIN,
    annotation { "Name" : "Valley folds" }
    VALLEY,
    annotation { "Name" : "Diagonal panel creases" }
    DIAGONAL,
    annotation { "Name" : "Axial panel creases" }
    AXIAL,
    annotation { "Name" : "Section boundaries" }
    SECTION
}

export enum KWebProfile
{
    annotation { "Name" : "Continuous strip per crease" }
    CONTINUOUS,
    annotation { "Name" : "Individual anchored segments" }
    SEGMENTED
}

export enum KAnchorProfile
{
    annotation { "Name" : "Round captive through-rivets" }
    RIVET,
    annotation { "Name" : "Captive rectangular beam keys" }
    BEAM_KEY
}

export enum KSeamConnection
{
    annotation { "Name" : "Open seam" }
    OPEN,
    annotation { "Name" : "Rounded eyelets for post-print lacing" }
    LACING,
    annotation { "Name" : "Mating dovetail loop closure" }
    DOVETAIL
}

export enum KSectionConnection
{
    annotation { "Name" : "Independent modules" }
    INDEPENDENT,
    annotation { "Name" : "Rounded eyelets for section lacing" }
    LACING
}

export enum KModuleLayout
{
    annotation { "Name" : "Keep assembled coordinates" }
    ASSEMBLED,
    annotation { "Name" : "Spaced net print layout" }
    SPACED
}

export enum KModifierGeneration
{
    annotation { "Name" : "Automatic from manufacturing mode" }
    AUTO,
    annotation { "Name" : "Enabled" }
    ON,
    annotation { "Name" : "Disabled" }
    OFF
}

export enum KModifierWidth
{
    annotation { "Name" : "Multiplier of actual hinge gap" }
    RELATIVE,
    annotation { "Name" : "Absolute ribbon width" }
    ABSOLUTE
}

export enum KModifierCoverage
{
    annotation { "Name" : "Full crease length" }
    FULL,
    annotation { "Name" : "Printed hinge interval" }
    HINGE
}

export enum KConnectionLayout
{
    annotation { "Name" : "Horizontal row in printable net" }
    HORIZONTAL,
    annotation { "Name" : "Along crease with staggered pairs (legacy)" }
    LEGACY
}

export enum KDovetailLayout
{
    annotation { "Name" : "Segmented sliding keys" }
    SEGMENTED,
    annotation { "Name" : "Continuous sliding rail" }
    CONTINUOUS
}

export enum KDovetailMaterial
{
    annotation { "Name" : "Rigid mating halves" }
    RIGID,
    annotation { "Name" : "TPU-assisted male with captive anchors" }
    TPU_MALE
}

// Defaults also preserve regeneration of instances saved before these controls existed.
const K_MANUFACTURING_DEFAULTS = {
    "modifierGeneration" : KModifierGeneration.AUTO,
    "modifierWidthMode" : KModifierWidth.RELATIVE,
    "modifierWidthMultiplier" : 1.5,
    "modifierCoverage" : KModifierCoverage.FULL,
    "modifierStartMargin" : 0 * millimeter,
    "modifierEndMargin" : 0 * millimeter,
    "connectionLayout" : KConnectionLayout.HORIZONTAL,
    "connectionHolePitch" : 6 * millimeter,
    "dovetailLayout" : KDovetailLayout.SEGMENTED,
    "dovetailMaterial" : KDovetailMaterial.RIGID,
    "dovetailWidth" : 4 * millimeter,
    "dovetailDepth" : 2 * millimeter,
    "dovetailAngle" : 15 * degree,
    "dovetailWall" : 1 * millimeter,
    "dovetailClearance" : 0.2 * millimeter,
    "dovetailRadius" : 0.3 * millimeter,
    "dovetailCount" : 2,
    "dovetailLength" : 6 * millimeter,
    "dovetailSpacing" : 2 * millimeter,
    "dovetailLeadInEnabled" : false,
    "dovetailLeadIn" : 0.2 * millimeter,
    "dovetailStop" : false,
    "manufacturingMode" : KManufacturing.SINGLE,
    "modifierEnabled" : false,
    "modifierOnly" : false,
    "modifierSelection" : KCreaseSelection.ALL,
    "modifierWidth" : 8 * millimeter,
    "modifierHeight" : 0.1 * millimeter,
    "modifierDepth" : 0.4 * millimeter,
    "modifierOffset" : 0 * millimeter,
    "tpuSelection" : KCreaseSelection.ALL,
    "foldSpecific" : false,
    "mountainWebT" : 0.4 * millimeter,
    "valleyWebT" : 0.4 * millimeter,
    "mountainSpan" : 1.6 * millimeter,
    "valleySpan" : 1.6 * millimeter,
    "webProfile" : KWebProfile.CONTINUOUS,
    "segmentLength" : 4 * millimeter,
    "anchorProfile" : KAnchorProfile.RIVET,
    "anchorShaftD" : 1.6 * millimeter,
    "anchorHeadD" : 2.6 * millimeter,
    "anchorHeadH" : 0.4 * millimeter,
    "anchorEngagement" : 2.2 * millimeter,
    "anchorBeamW" : 1.2 * millimeter,
    "anchorMinPitch" : 5 * millimeter,
    "anchorCount" : 2,
    "anchorLigament" : 0.6 * millimeter,
    "seamConnection" : KSeamConnection.OPEN,
    "seamEyeletW" : 5 * millimeter,
    "seamEyeletDepth" : 5 * millimeter,
    "seamEyeletHoleD" : 2 * millimeter,
    "seamEyeletCount" : 1,
    "projectEyeletTabs" : false,
    "eyeletCollarHeight" : 0.4 * millimeter,
    "sectionConnection" : KSectionConnection.INDEPENDENT,
    "moduleLayout" : KModuleLayout.SPACED,
    "moduleSpacing" : 14 * millimeter,
    "limitCreases" : false,
    "creaseIndices" : [],
    "colorManufacturingRoles" : true
};

annotation { "Feature Type Name" : "Codex Kresling V2",
    "Feature Type Description" : "Faceted design states and exact cut nets; not a rigid-folding simulation" }
export const kreslingEngineering = defineFeature(function(context is Context, id is Id, definition is map)
precondition
{
    annotation { "Group Name" : "Generation Mode", "Collapsed By Default" : false }
    {
        annotation { "Name" : "Generate", "Default" : KMode.FOLDED }
        definition.mode is KMode;
        annotation { "Name" : "Material construction", "Default" : KManufacturing.SINGLE }
        definition.manufacturingMode is KManufacturing;
        annotation { "Name" : "Crease modifiers", "Default" : KModifierGeneration.AUTO,
            "Description" : "Automatic follows the manufacturing mode. Disabled always suppresses slicer-only geometry." }
        definition.modifierGeneration is KModifierGeneration;
        annotation { "Name" : "Crease guide sketches in net", "Default" : true }
        definition.guides is boolean;
    }
    annotation { "Group Name" : "Basic Kresling Geometry", "Collapsed By Default" : false }
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
    annotation { "Group Name" : "Twist and End Plates", "Collapsed By Default" : true }
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
    annotation { "Group Name" : "Material and Hinge Settings", "Collapsed By Default" : true }
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
    annotation { "Group Name" : "Manufacturing and Connections", "Collapsed By Default" : true }
    {
    annotation { "Group Name" : "Infill Modifier Settings", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Legacy modifier enable", "Default" : false, "UIHint" : UIHint.ALWAYS_HIDDEN }
        definition.modifierEnabled is boolean;
        if (definition.modifierGeneration == KModifierGeneration.ON || (definition.modifierGeneration == KModifierGeneration.AUTO && (definition.manufacturingMode == KManufacturing.MULTI_MODIFIERS || definition.modifierEnabled)))
        {
        annotation { "Name" : "Modifier crease types", "Default" : KCreaseSelection.ALL }
        definition.modifierSelection is KCreaseSelection;
        annotation { "Name" : "Ribbon width input", "Default" : KModifierWidth.RELATIVE }
        definition.modifierWidthMode is KModifierWidth;
        if (definition.modifierWidthMode == KModifierWidth.RELATIVE)
        {
            annotation { "Name" : "Width multiplier", "Description" : "Multiplies the actual sum of both computed crease setbacks, including thickness clearance." }
            isReal(definition.modifierWidthMultiplier, { (unitless) : [1.05, 1.5, 5] } as RealBoundSpec);
        }
        else
        {
        annotation { "Name" : "Modifier width across crease" }
        isLength(definition.modifierWidth, { (millimeter) : [1, 8, 30] } as LengthBoundSpec);
        }
        annotation { "Name" : "Seam coverage", "Default" : KModifierCoverage.FULL }
        definition.modifierCoverage is KModifierCoverage;
        annotation { "Name" : "Start margin (inward)", "Description" : "Zero reaches the reference crease endpoint." }
        isLength(definition.modifierStartMargin, { (millimeter) : [0, 0, 20] } as LengthBoundSpec);
        annotation { "Name" : "End margin (inward)" }
        isLength(definition.modifierEndMargin, { (millimeter) : [0, 0, 20] } as LengthBoundSpec);
        annotation { "Name" : "Modifier surface allowance" }
        isLength(definition.modifierHeight, { (millimeter) : [0, 0.1, 3] } as LengthBoundSpec);
        annotation { "Name" : "Modifier penetration from hinge side" }
        isLength(definition.modifierDepth, { (millimeter) : [0.1, 0.4, 8] } as LengthBoundSpec);
        annotation { "Name" : "Modifier offset from centerline" }
        isLength(definition.modifierOffset, { (millimeter) : [-10, 0, 10] } as LengthBoundSpec);
        }
    }
    if (definition.manufacturingMode == KManufacturing.MULTI || definition.manufacturingMode == KManufacturing.MULTI_MODIFIERS)
    {
    annotation { "Group Name" : "Multi-Material Hinges", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Creases receiving TPU", "Default" : KCreaseSelection.ALL, "Description" : "Other retained creases keep integral rigid-material webs; connectivity is preserved." }
        definition.tpuSelection is KCreaseSelection;
        annotation { "Name" : "Different mountain and valley dimensions", "Default" : false }
        definition.foldSpecific is boolean;
        if (definition.foldSpecific)
        {
        annotation { "Name" : "Mountain TPU thickness" }
        isLength(definition.mountainWebT, { (millimeter) : [0.15, 0.4, 2] } as LengthBoundSpec);
        annotation { "Name" : "Valley TPU thickness" }
        isLength(definition.valleyWebT, { (millimeter) : [0.15, 0.4, 2] } as LengthBoundSpec);
        annotation { "Name" : "Mountain minimum span" }
        isLength(definition.mountainSpan, { (millimeter) : [0.6, 1.6, 12] } as LengthBoundSpec);
        annotation { "Name" : "Valley minimum span" }
        isLength(definition.valleySpan, { (millimeter) : [0.6, 1.6, 12] } as LengthBoundSpec);
        }
        annotation { "Name" : "TPU web profile", "Default" : KWebProfile.CONTINUOUS }
        definition.webProfile is KWebProfile;
        if (definition.webProfile == KWebProfile.SEGMENTED)
        {
        annotation { "Name" : "Length of each anchored segment" }
        isLength(definition.segmentLength, { (millimeter) : [1.5, 4, 20] } as LengthBoundSpec);
        }
    }
    annotation { "Group Name" : "Hinge Interlocking", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Captive anchor geometry", "Default" : KAnchorProfile.RIVET }
        definition.anchorProfile is KAnchorProfile;
        annotation { "Name" : "Anchor shaft diameter / width" }
        isLength(definition.anchorShaftD, { (millimeter) : [0.8, 1.6, 6] } as LengthBoundSpec);
        annotation { "Name" : "Anchor head diameter / width" }
        isLength(definition.anchorHeadD, { (millimeter) : [1.2, 2.6, 10] } as LengthBoundSpec);
        annotation { "Name" : "Flush anchor head thickness" }
        isLength(definition.anchorHeadH, { (millimeter) : [0.2, 0.4, 2] } as LengthBoundSpec);
        annotation { "Name" : "Anchor center inset into panel" }
        isLength(definition.anchorEngagement, { (millimeter) : [1.2, 2.2, 10] } as LengthBoundSpec);
        annotation { "Name" : "Connecting beam width" }
        isLength(definition.anchorBeamW, { (millimeter) : [0.8, 1.2, 5] } as LengthBoundSpec);
        annotation { "Name" : "Minimum anchor spacing" }
        isLength(definition.anchorMinPitch, { (millimeter) : [2, 5, 30] } as LengthBoundSpec);
        annotation { "Name" : "Rigid ligament around anchor head" }
        isLength(definition.anchorLigament, { (millimeter) : [0.4, 0.6, 4] } as LengthBoundSpec);
        annotation { "Name" : "Anchors on each side of each crease" }
        isInteger(definition.anchorCount, { (unitless) : [1, 2, 8] } as IntegerBoundSpec);
    }
    }
    annotation { "Group Name" : "Net End Connections", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Longitudinal seam", "Default" : KSeamConnection.OPEN, "Description" : "Choose lacing or a mating sliding dovetail after wrapping. No bridge crosses the flat net." }
        definition.seamConnection is KSeamConnection;
    }
    if (definition.seamConnection == KSeamConnection.DOVETAIL)
    {
    annotation { "Group Name" : "Loop Closure and Dovetail", "Collapsed By Default" : false }
    {
        annotation { "Name" : "Rail layout", "Default" : KDovetailLayout.SEGMENTED }
        definition.dovetailLayout is KDovetailLayout;
        annotation { "Name" : "Joint material", "Default" : KDovetailMaterial.RIGID }
        definition.dovetailMaterial is KDovetailMaterial;
        annotation { "Name" : "Head width" }
        isLength(definition.dovetailWidth, { (millimeter) : [2.5, 4, 10] } as LengthBoundSpec);
        annotation { "Name" : "Engagement depth" }
        isLength(definition.dovetailDepth, { (millimeter) : [1, 2, 5] } as LengthBoundSpec);
        annotation { "Name" : "Dovetail taper angle" }
        isAngle(definition.dovetailAngle, { (degree) : [5, 15, 30] } as AngleBoundSpec);
        annotation { "Name" : "Receiver wall thickness" }
        isLength(definition.dovetailWall, { (millimeter) : [0.6, 1, 3] } as LengthBoundSpec);
        annotation { "Name" : "Mating clearance per side" }
        isLength(definition.dovetailClearance, { (millimeter) : [0.1, 0.2, 0.6] } as LengthBoundSpec);
        annotation { "Name" : "Corner radius" }
        isLength(definition.dovetailRadius, { (millimeter) : [0.05, 0.3, 0.8] } as LengthBoundSpec);
        if (definition.dovetailLayout == KDovetailLayout.SEGMENTED)
        {
        annotation { "Name" : "Segments per cell seam" }
        isInteger(definition.dovetailCount, { (unitless) : [1, 2, 6] } as IntegerBoundSpec);
        annotation { "Name" : "Key length along seam" }
        isLength(definition.dovetailLength, { (millimeter) : [3, 6, 30] } as LengthBoundSpec);
        annotation { "Name" : "Minimum gap between keys" }
        isLength(definition.dovetailSpacing, { (millimeter) : [0.5, 2, 12] } as LengthBoundSpec);
        }
        annotation { "Name" : "Chamfer the insertion end", "Default" : false }
        definition.dovetailLeadInEnabled is boolean;
        if (definition.dovetailLeadInEnabled)
        {
        annotation { "Name" : "Lead-in chamfer" }
        isLength(definition.dovetailLeadIn, { (millimeter) : [0.05, 0.2, 0.6] } as LengthBoundSpec);
        }
        annotation { "Name" : "One-way axial end stop", "Default" : false, "Description" : "Stops further insertion in one direction; does not lock both axial directions or establish retention strength." }
        definition.dovetailStop is boolean;
    }
    }
    annotation { "Group Name" : "Tiling Connections", "Collapsed By Default" : true }
    {
        if (definition.manufacturingMode == KManufacturing.MODULAR)
        {
        annotation { "Name" : "Module boundary connection", "Default" : KSectionConnection.INDEPENDENT }
        definition.sectionConnection is KSectionConnection;
        annotation { "Name" : "Module layout", "Default" : KModuleLayout.SPACED }
        definition.moduleLayout is KModuleLayout;
        if (definition.moduleLayout == KModuleLayout.SPACED)
        {
        annotation { "Name" : "Space between module reference envelopes" }
        isLength(definition.moduleSpacing, { (millimeter) : [2, 14, 100] } as LengthBoundSpec);
        }
        }
    }
    if (definition.seamConnection == KSeamConnection.LACING || (definition.manufacturingMode == KManufacturing.MODULAR && definition.sectionConnection == KSectionConnection.LACING))
    {
    annotation { "Group Name" : "Connection Settings", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Connection hole layout", "Default" : KConnectionLayout.HORIZONTAL,
            "Description" : "Hole centers follow net +X; every bore remains normal to its owning panel. The pattern is transported into the folded model." }
        definition.connectionLayout is KConnectionLayout;
        if (definition.connectionLayout == KConnectionLayout.HORIZONTAL)
        {
            annotation { "Name" : "Horizontal hole spacing" }
            isLength(definition.connectionHolePitch, { (millimeter) : [3, 6, 30] } as LengthBoundSpec);
        }
        annotation { "Name" : "Protruding assembly tabs (net only)", "Default" : false }
        definition.projectEyeletTabs is boolean;
        if (!definition.projectEyeletTabs)
        {
            annotation { "Name" : "Eyelet collar height above panel" }
            isLength(definition.eyeletCollarHeight, { (millimeter) : [0, 0.4, 2] } as LengthBoundSpec);
        }
        annotation { "Name" : "Collar diameter / tab width" }
        isLength(definition.seamEyeletW, { (millimeter) : [3, 5, 16] } as LengthBoundSpec);
        annotation { "Name" : "Minimum collar inset / tab extension" }
        isLength(definition.seamEyeletDepth, { (millimeter) : [3, 5, 16] } as LengthBoundSpec);
        annotation { "Name" : "Lacing hole diameter" }
        isLength(definition.seamEyeletHoleD, { (millimeter) : [1, 2, 8] } as LengthBoundSpec);
        annotation { "Name" : "Eyelets on each side of a connection" }
        isInteger(definition.seamEyeletCount, { (unitless) : [1, 1, 6] } as IntegerBoundSpec);
    }
    }
    annotation { "Group Name" : "Export and Manufacturing", "Collapsed By Default" : true }
    {
        annotation { "Name" : "Color material and modifier roles", "Default" : true }
        definition.colorManufacturingRoles is boolean;
        if (definition.modifierGeneration == KModifierGeneration.ON || (definition.modifierGeneration == KModifierGeneration.AUTO && (definition.manufacturingMode == KManufacturing.MULTI_MODIFIERS || definition.modifierEnabled)))
        {
        annotation { "Name" : "Output only modifiers for separate export", "Default" : false }
        definition.modifierOnly is boolean;
        }
        annotation { "Name" : "Restrict TPU and modifiers to listed crease IDs", "Default" : false }
        definition.limitCreases is boolean;
        if (definition.limitCreases)
        {
            annotation { "Name" : "One-based crease IDs", "Item name" : "Crease" }
            definition.creaseIndices is array;
            for (var selected in definition.creaseIndices)
            {
                annotation { "Name" : "Crease ID" }
                isInteger(selected.manufacturingCreaseIndex, { (unitless) : [1, 1, 1024] } as IntegerBoundSpec);
            }
        }
    }
    }
}
{
    definition = mergeMaps(K_MANUFACTURING_DEFAULTS, definition);
    const splitMaterials = definition.manufacturingMode == KManufacturing.MULTI || definition.manufacturingMode == KManufacturing.MULTI_MODIFIERS;
    const modular = definition.manufacturingMode == KManufacturing.MODULAR;
    const makeModifiers = definition.modifierGeneration == KModifierGeneration.ON || (definition.modifierGeneration == KModifierGeneration.AUTO && (definition.manufacturingMode == KManufacturing.MULTI_MODIFIERS || definition.modifierEnabled));
    kManufacturingValidation(definition);
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
                var creaseHalfGap = halfGap;
                if (splitMaterials && definition.foldSpecific)
                    creaseHalfGap = max(definition.gap, ridge ? definition.mountainSpan : definition.valleySpan) / mm / 2;
                var gA = creaseHalfGap;
                var gB = creaseHalfGap;
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
    for (var k = 0; k < size(links); k += 1)
        links[k].manufacturingId = k + 1;
    if (definition.limitCreases)
        for (var selected in definition.creaseIndices)
            if (selected.manufacturingCreaseIndex > size(links))
                throw regenError("A selected crease ID exceeds the retained crease count.", ["creaseIndices"]);
    if (modular)
    {
        var retained = [];
        for (var cr in links)
            if (kModuleOf(faces[cr.a], definition) == kModuleOf(faces[cr.b], definition))
                retained = append(retained, cr);
        links = retained;
    }
    var printFrames = flatFaces;
    if (modular && definition.moduleLayout == KModuleLayout.SPACED)
        printFrames = kSpaceModules(printFrames, definition);
    const rowFrames = printFrames;
    printFrames = kPlaceNet(printFrames, definition.netAngle / radian);
    for (var i = 0; i < size(faces); i += 1)
    {
        faces[i].netPts = rowFrames[i].pts;
        printFrames[i].netPts = rowFrames[i].pts;
    }
    if (definition.mode == KMode.NET)
    {
        faces = printFrames;
        for (var a = 0; a < size(faces); a += 1)
            for (var b = a + 1; b < size(faces); b += 1)
                if (kOverlap2d(faces[a].pts, faces[b].pts))
                    throw regenError("The exact cut net overlaps between faces " ~ (a + 1) ~ " and " ~ (b + 1) ~
                                     ". Change seam, twist or profile; alternatively export cells separately.", ["seam", "twist", "profile"]);
    }

    // Validate the complete inset polygon against EVERY original half-plane.
    // This detects inverted/empty insets, which the original script did not.
    var panelQueries = [];
    var panelHoleKeepouts = [];
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
        var eyeletIndex = 0;
        var eyeletHoles = [];
        for (var cut in creases)
        {
            if (!kNeedsEyelets(cut, faces, definition) || (cut.a != i && cut.b != i))
                continue;
            const edge = cut.a == i ? cut.ia : cut.ib;
            const eyeletId = panelId + ("eyelets" ~ eyeletIndex);
            eyeletHoles = concatenateArrays([eyeletHoles, kEyelets(context, eyeletId, f, edge, gaps[i], cut.a == i,
                { "face" : faces[cut.a == i ? cut.b : cut.a], "edge" : cut.a == i ? cut.ib : cut.ia,
                  "gaps" : gaps[cut.a == i ? cut.b : cut.a], "ownGap" : gaps[i][edge] }, definition)]);
            pieces = append(pieces, qBodyType(qCreatedBy(eyeletId, EntityType.BODY), BodyType.SOLID));
            eyeletIndex += 1;
        }
        if (size(pieces) > 1)
            opBoolean(context, panelId + "merge", { "tools" : qUnion(pieces), "operationType" : BooleanOperationType.UNION });
        const panel = qBodyType(qCreatedBy(panelId, EntityType.BODY), BodyType.SOLID);
        for (var h = 0; h < size(eyeletHoles); h += 1)
            kCircleCut(context, panelId + ("lacingHole" ~ h), panel, eyeletHoles[h].start, eyeletHoles[h].axis,
                definition.seamEyeletHoleD / mm / 2, eyeletHoles[h].depth);
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
        var keepouts = [];
        for (var hole in eyeletHoles)
            keepouts = append(keepouts, { "center" : hole.keepoutCenter, "radius" : hole.keepoutRadius });
        if (r > 0)
            keepouts = append(keepouts, { "center" : kCentroid(shape), "radius" : r });
        panelHoleKeepouts = append(panelHoleKeepouts, keepouts);
        panelQueries = append(panelQueries, panel);
    }
    var closureTPU = [];
    var closureParts = [];
    var closureBodies = [];
    if (definition.seamConnection == KSeamConnection.DOVETAIL)
    {
        for (var cut in creases)
            if (!faces[cut.a].cap && !faces[cut.b].cap && faces[cut.a].cell == faces[cut.b].cell &&
                abs(faces[cut.a].order - faces[cut.b].order) == 2 * n - 1)
                closureParts = concatenateArrays([closureParts,
                    kDovetailPair(context, id + ("closure" ~ faces[cut.a].cell), cut, faces, gaps, panelQueries, panelHoleKeepouts, definition)]);
        for (var i = 0; i < size(closureParts); i += 1)
        {
            const part = closureParts[i];
            for (var j = 0; j < size(panelQueries); j += 1)
                if (j != part.owner && kHasVolumeContact(context, part.body, panelQueries[j]))
                    throw regenError("A dovetail support hits another panel. Reduce joint size or increase the tube dimensions.");
            for (var j = 0; j < i; j += 1)
                if (kHasVolumeContact(context, part.body, closureParts[j].body))
                    throw regenError("Dovetail segments or support arms interfere.");
        }
        for (var i = 0; i < size(closureParts); i += 1)
        {
            const part = closureParts[i];
            closureBodies = append(closureBodies, part.body);
            if (part.tpu)
                closureTPU = append(closureTPU, part.body);
        }
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
    var tpuQueries = closureTPU;
    var rigidHingeQueries = [];
    var hingeOwners = [];
    var hingeCreaseIds = [];
    var modifierSpecs = [];
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
        const mountain = cr.outer != definition.invertHinges;
        const flexible = splitMaterials && kSelectCrease(cr, faces, mountain, definition.tpuSelection, definition);
        const webThickness = flexible && definition.foldSpecific ?
            (mountain ? definition.mountainWebT : definition.valleyWebT) / mm : hingeT;
        if (webThickness < definition.minHinge / mm || webThickness > 0.8 * min(fA.t, fB.t))
            throw regenError("Selected web thickness violates printable minimum or adjoining wall thickness.", ["mountainWebT", "valleyWebT", "hingeT"]);
        var interval = [definition.relief / mm + cornerR, norm(P1 - P0) - definition.relief / mm - cornerR];
        interval = kClipAttachment(interval, fA, cr.ia, gaps[cr.a], P0, ev, uA, gA, overlap, cornerR);
        interval = kClipAttachment(interval, fB, cr.ib, gaps[cr.b], P0, ev, uB, gB, overlap, cornerR);
        if (interval[1] - interval[0] < max(1, 3 * webThickness))
            throw regenError("A crease has insufficient hinge attachment length. Increase cell size or reduce gaps, overlap or end relief.", ["hingeWidth", "overlap", "relief"]);
        var stations = [];
        if (flexible)
        {
            const headRadius = definition.anchorHeadD / mm / 2 * (definition.anchorProfile == KAnchorProfile.BEAM_KEY ? sqrt(2) : 1);
            const room = headRadius + definition.anchorLigament / mm + cornerR;
            var anchorInterval = interval;
            anchorInterval = kClipAnchor(anchorInterval, fA, gaps[cr.a], P0, ev, (gA + definition.anchorEngagement / mm) * uA, room);
            anchorInterval = kClipAnchor(anchorInterval, fB, gaps[cr.b], P0, ev, (gB + definition.anchorEngagement / mm) * uB, room);
            const available = anchorInterval[1] - anchorInterval[0];
            if (available <= 0 || (definition.anchorCount > 1 && available / definition.anchorCount < definition.anchorMinPitch / mm))
                throw regenError("Captive anchors do not fit this crease with the requested spacing and ligament. Reduce anchor count or increase cell size.", ["anchorCount", "anchorMinPitch", "anchorHeadD"]);
            for (var a = 0; a < definition.anchorCount; a += 1)
                stations = append(stations, anchorInterval[0] + available * (a + 0.5) / definition.anchorCount);
        }
        var segments = [interval];
        if (flexible && definition.webProfile == KWebProfile.SEGMENTED)
        {
            segments = [];
            const half = definition.segmentLength / mm / 2;
            for (var station in stations)
            {
                if (station - half < interval[0] || station + half > interval[1])
                    throw regenError("An anchored segment extends beyond its valid crease interval.", ["segmentLength"]);
                segments = append(segments, [station - half, station + half]);
            }
        }
        for (var part = 0; part < size(segments); part += 1)
        {
            const segment = segments[part];
            const Pw = P0 + segment[0] * ev;
            var poly = [];
            if (cr.outer)
            {
                const pA = Pw + (gA + overlap) * uA;
                const pB = Pw + (gB + overlap) * uB;
                const middle = Pw + kOffset(fA.n, fB.n, webThickness, webThickness);
                poly = [pA, Pw, pB, pB + webThickness * fB.n, middle, pA + webThickness * fA.n];
            }
            else if (definition.mode == KMode.NET)
            {
                // A sloped inner web joins unequal plate/panel thicknesses in a net.
                const pA = Pw + (gA + overlap) * uA + fA.t * fA.n;
                const pB = Pw + (gB + overlap) * uB + fB.t * fB.n;
                const middle = Pw + (fA.t + fB.t) / 2 * fA.n;
                poly = [pA, middle, pB, pB - webThickness * fB.n, middle - webThickness * fA.n, pA - webThickness * fA.n];
            }
            else
            {
                const Pi = Pw + kOffset(fA.n, fB.n, fA.t, fB.t);
                const wA = gA + overlap - dot(Pi - Pw, uA);
                const wB = gB + overlap - dot(Pi - Pw, uB);
                const pA = Pi + wA * uA;
                const pB = Pi + wB * uB;
                const middle = Pw + kOffset(fA.n, fB.n, fA.t - webThickness, fB.t - webThickness);
                poly = [pA, Pi, pB, pB - webThickness * fB.n, middle, pA - webThickness * fA.n];
            }
            const hingeId = id + ("hinge" ~ k) + ("piece" ~ part);
            kExtrudePoly(context, hingeId + "web", poly, ev, segment[1] - segment[0]);
            var webPieces = [qCreatedBy(hingeId + "web" + "ext", EntityType.BODY)];
            if (flexible)
                for (var a = 0; a < size(stations); a += 1)
                {
                    if (definition.webProfile == KWebProfile.SEGMENTED && a != part)
                        continue;
                    const base = P0 + stations[a] * ev;
                    const anchorId = hingeId + ("anchors" ~ a);
                    kAnchor(context, anchorId + "A", fA, panelHoleKeepouts[cr.a], base, ev, uA, gA, cr.outer, webThickness, panelQueries[cr.a], definition);
                    kAnchor(context, anchorId + "B", fB, panelHoleKeepouts[cr.b], base, ev, uB, gB, cr.outer, webThickness, panelQueries[cr.b], definition);
                    webPieces = append(webPieces, qBodyType(qCreatedBy(anchorId, EntityType.BODY), BodyType.SOLID));
                }
            const web = kJoinBodies(context, hingeId + "merge", webPieces);
            if (size(evaluateQuery(context, web)) != 1)
                throw regenError("A TPU anchor or segment is disconnected from its hinge strip.");
            var forbidden = [];
            for (var j = 0; j < size(panelQueries); j += 1)
                if (j != cr.a && j != cr.b)
                    forbidden = append(forbidden, panelQueries[j]);
            forbidden = concatenateArrays([forbidden, hingeQueries, closureBodies]);
            if (size(forbidden) > 0 && size(evCollision(context, { "tools" : web, "targets" : qUnion(forbidden) })) > 0)
                throw regenError("A hinge or anchor contacts a different panel or hinge. Increase relief, reduce anchor size or change geometry.", ["relief", "anchorHeadD", "compression"]);
            if (!kHasVolumeContact(context, web, panelQueries[cr.a]) || !kHasVolumeContact(context, web, panelQueries[cr.b]))
                throw regenError("A hinge does not overlap both intended panels.", ["overlap"]);
            hingeQueries = append(hingeQueries, web);
            hingeOwners = append(hingeOwners, kModuleOf(fA, definition));
            hingeCreaseIds = append(hingeCreaseIds, cr.manufacturingId);
            if (flexible)
            {
                tpuQueries = append(tpuQueries, web);
                kNameBodies(context, web, "TPU__crease_" ~ cr.manufacturingId ~ "__segment_" ~ (part + 1), color(0.15, 0.65, 0.4), definition);
            }
            else
                rigidHingeQueries = append(rigidHingeQueries, web);
        }
        if (makeModifiers && kSelectCrease(cr, faces, mountain, definition.modifierSelection, definition))
        {
            const from = (definition.modifierCoverage == KModifierCoverage.FULL ? 0 : interval[0]) + definition.modifierStartMargin / mm;
            const to = (definition.modifierCoverage == KModifierCoverage.FULL ? norm(P1 - P0) : interval[1]) - definition.modifierEndMargin / mm;
            if (to - from < 0.5)
                throw regenError("Modifier margins leave less than 0.5 mm of coverage.", ["modifierStartMargin", "modifierEndMargin"]);
            const spec = { "crease" : cr, "start" : P0 + from * ev, "ev" : ev, "length" : to - from,
                "faceA" : fA, "faceB" : fB, "gapsA" : gaps[cr.a], "gapsB" : gaps[cr.b],
                "uA" : uA, "uB" : uB, "gA" : gA, "gB" : gB, "outer" : cr.outer };
            modifierSpecs = append(modifierSpecs, spec);
        }
        guideData = append(guideData, { "a" : P0, "b" : P1, "mountain" : mountain });
    }
    for (var i = 0; i < size(modifierSpecs); i += 1)
    {
        const spec = modifierSpecs[i];
        var unrelated = [qUnion(closureBodies), qSubtraction(panels, qUnion([panelQueries[spec.crease.a], panelQueries[spec.crease.b]]))];
        for (var h = 0; h < size(hingeQueries); h += 1)
            if (hingeCreaseIds[h] != spec.crease.manufacturingId)
                unrelated = append(unrelated, hingeQueries[h]);
        kModifier(context, id + ("modifier" ~ i), spec, panelQueries[spec.crease.a], panelQueries[spec.crease.b], qUnion(unrelated), definition);
    }
    // Keep the joint bodies distinct during all crease-interference checks;
    // otherwise fusing a receiver into an owner could hide a blocked hinge.
    for (var i = 0; i < size(closureParts); i += 1)
        if (!closureParts[i].tpu)
        {
            const part = closureParts[i];
            panelQueries[part.owner] = kJoinBodies(context, id + ("closureOwner" ~ i), [panelQueries[part.owner], part.body]);
            if (size(evaluateQuery(context, panelQueries[part.owner])) != 1)
                throw regenError("A loop-closure half is disconnected from its owning panel.");
        }
    var printed = [];
    if (splitMaterials)
    {
        if (size(tpuQueries) == 0)
            throw regenError("No retained creases match the TPU selection. Choose a different filter or crease IDs.", ["tpuSelection", "creaseIndices"]);
        // Reserve the actual TPU volume in PLA; coincident material volumes are
        // never exported. keepTools preserves the independent TPU solids.
        opBoolean(context, id + "reserveTPU", { "tools" : qUnion(tpuQueries), "targets" : panels,
            "operationType" : BooleanOperationType.SUBTRACTION, "keepTools" : true });
        for (var panel in panelQueries)
            if (size(evaluateQuery(context, panel)) != 1)
                throw regenError("Anchor cavities split or erase a rigid panel. Increase ligament or reduce anchors.", ["anchorHeadD", "anchorLigament"]);
        const rigid = kJoinBodies(context, id + "rigidMaterial", concatenateArrays([panelQueries, rigidHingeQueries]));
        if (kHasVolumeContact(context, rigid, qUnion(tpuQueries)))
            throw regenError("Printable PLA and TPU volumes overlap after material reservation.");
        for (var web in tpuQueries)
            if (size(evCollision(context, { "tools" : web, "targets" : rigid })) == 0)
                throw regenError("A TPU solid has no contact with the remaining PLA assembly.");
        kNameBodies(context, rigid, "PLA__rigid", color(0.3, 0.55, 0.85), definition);
        printed = [rigid, qUnion(tpuQueries)];
    }
    else if (modular)
    {
        for (var cell = 0; cell < count; cell += 1)
        {
            var parts = [];
            for (var i = 0; i < size(faces); i += 1)
                if (kModuleOf(faces[i], definition) == cell)
                    parts = append(parts, panelQueries[i]);
            for (var i = 0; i < size(hingeQueries); i += 1)
                if (hingeOwners[i] == cell)
                    parts = append(parts, hingeQueries[i]);
            const module = kJoinBodies(context, id + ("module" ~ cell), parts);
            if (size(evaluateQuery(context, module)) != 1)
                throw regenError("An independent module is not one connected solid.");
            kNameBodies(context, module, "SINGLE_MATERIAL__module_" ~ (cell + 1), color(0.3, 0.55, 0.85), definition);
            printed = append(printed, module);
        }
    }
    else
    {
        const result = kJoinBodies(context, id + "union", [panels, qUnion(hingeQueries)]);
        if (size(evaluateQuery(context, result)) != 1)
            throw regenError("The result is disconnected. No printable part was accepted.");
        kNameBodies(context, result, definition.mode == KMode.NET ? "Kresling printable cut net" : "Kresling folded design state", color(0.3, 0.55, 0.85), definition);
        printed = [result];
    }
    if (makeModifiers)
    {
        if (size(modifierSpecs) == 0)
            throw regenError("No retained creases match the modifier selection.", ["modifierSelection", "creaseIndices"]);
        if (definition.modifierOnly)
            opDeleteBodies(context, id + "modifierExportOnly", { "entities" : qUnion(printed) });
    }
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
             "referencePts" : pts, "referenceN" : n, "referenceC" : c,
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

// ---------------------------------------------------------------------------
// Manufacturing bodies. Material volumes and slicer-only volumes stay separate.
// ---------------------------------------------------------------------------
function kManufacturingValidation(d is map)
{
    const mm = millimeter;
    const split = d.manufacturingMode == KManufacturing.MULTI || d.manufacturingMode == KManufacturing.MULTI_MODIFIERS;
    if (d.limitCreases && size(d.creaseIndices) == 0)
        throw regenError("Enter at least one crease ID, or disable the crease-ID restriction.", ["creaseIndices"]);
    if (split)
    {
        const envelope = d.anchorHeadD / 2 * (d.anchorProfile == KAnchorProfile.BEAM_KEY ? sqrt(2) : 1);
        if (d.anchorHeadD < d.anchorShaftD + 0.6 * mm)
            throw regenError("Anchor heads must be at least 0.6 mm wider than their shafts for positive capture.", ["anchorHeadD", "anchorShaftD"]);
        if (d.anchorEngagement < envelope + d.anchorLigament)
            throw regenError("Anchor engagement must fit the head envelope plus a rigid edge ligament.", ["anchorEngagement", "anchorLigament"]);
        if (d.anchorHeadH < d.minHinge || d.panelT / mm < 2 * d.anchorHeadH / mm + 0.4 - 1e-6 ||
            (d.caps != KCaps.NONE && d.plateT / mm < 2 * d.anchorHeadH / mm + 0.4 - 1e-6))
            throw regenError("Captive heads need printable TPU thickness and at least a 0.4 mm rigid layer between the two head pockets.", ["anchorHeadH", "panelT", "plateT"]);
        if (d.anchorBeamW < d.minWall || d.anchorMinPitch < 2 * envelope + d.clearance)
            throw regenError("Anchor beams or spacing are smaller than printable wall and head-clearance limits.", ["anchorBeamW", "anchorMinPitch"]);
        if (d.anchorEngagement <= d.overlap ||
            (d.webProfile == KWebProfile.SEGMENTED && (d.segmentLength < d.anchorBeamW + 0.2 * mm || d.segmentLength + d.clearance >= d.anchorMinPitch)))
            throw regenError("Anchors must extend past the web overlap; segments must fit the beam and leave space between segments.", ["anchorEngagement", "overlap", "segmentLength"]);
    }
    if (d.seamConnection == KSeamConnection.DOVETAIL)
    {
        const neck = d.dovetailWidth - 2 * d.dovetailDepth * tan(d.dovetailAngle);
        const cornerLoss = d.dovetailRadius * (1 / tan((90 * degree - d.dovetailAngle) / 2) - 1);
        const capture = (d.dovetailWidth - neck) / 2 - cornerLoss - d.dovetailClearance;
        if (neck < 2 * d.minWall || capture < 0.2 * mm || d.dovetailWall < d.minWall)
            throw regenError("Dovetail neck, receiver wall or remaining undercut is too small. Adjust width, depth, angle or clearance.", ["dovetailWidth", "dovetailAngle", "dovetailClearance", "dovetailWall"]);
        if (d.dovetailMaterial == KDovetailMaterial.TPU_MALE && !split)
            throw regenError("A TPU-assisted dovetail requires a PLA/TPU manufacturing mode.", ["manufacturingMode", "dovetailMaterial"]);
        if (d.dovetailLeadInEnabled && d.dovetailLeadIn >= min(d.dovetailRadius, d.dovetailWall / 3))
            throw regenError("Lead-in chamfer must be smaller than the corner radius and one-third of receiver wall thickness.", ["dovetailLeadIn"]);
    }
    const laced = d.seamConnection == KSeamConnection.LACING ||
        (d.manufacturingMode == KManufacturing.MODULAR && d.sectionConnection == KSectionConnection.LACING);
    if (laced && (d.seamEyeletW < d.seamEyeletHoleD + 2 * d.minWall ||
        d.seamEyeletDepth < (d.projectEyeletTabs ? d.seamEyeletW : d.seamEyeletW / 2 + d.minWall)))
        throw regenError("Eyelets require a printable hole ligament and enough inset or extension for the selected arrangement.", ["seamEyeletW", "seamEyeletDepth", "seamEyeletHoleD"]);
    if (laced && d.connectionLayout == KConnectionLayout.HORIZONTAL && d.connectionHolePitch < d.seamEyeletW + d.clearance)
        throw regenError("Horizontal hole spacing must exceed collar width plus clearance.", ["connectionHolePitch", "seamEyeletW"]);
    if (laced && d.projectEyeletTabs && d.mode != KMode.NET)
        throw regenError("Protruding eyelet tabs are a flat assembly option. Use inset eyelets for a folded-compatible lacing seam.", ["projectEyeletTabs"]);
    if (d.manufacturingMode == KManufacturing.MODULAR && d.mode == KMode.NET)
    {
        if (d.sectionConnection == KSectionConnection.LACING && d.projectEyeletTabs && d.moduleLayout != KModuleLayout.SPACED)
            throw regenError("Section eyelets need the spaced net layout to avoid entering the neighboring module.", ["moduleLayout"]);
        if (d.moduleLayout == KModuleLayout.SPACED && laced && d.projectEyeletTabs && d.moduleSpacing < 2 * d.seamEyeletDepth + d.clearance)
            throw regenError("Module spacing must exceed two eyelet extensions plus clearance.", ["moduleSpacing"]);
    }
}

function kModuleOf(face is map, d is map) returns number
{
    if (!face.cap)
        return face.cell;
    return face.ids[0] < d.sides ? 0 : d.cells - 1;
}

function kSelectCrease(cr is map, faces is array, mountain is boolean, choice is KCreaseSelection, d is map) returns boolean
{
    if (d.limitCreases)
    {
        var listed = false;
        for (var entry in d.creaseIndices)
            if (entry.manufacturingCreaseIndex == cr.manufacturingId)
                listed = true;
        if (!listed)
            return false;
    }
    if (choice == KCreaseSelection.ALL)
        return true;
    if (choice == KCreaseSelection.MOUNTAIN)
        return mountain;
    if (choice == KCreaseSelection.VALLEY)
        return !mountain;
    const a = faces[cr.a];
    const b = faces[cr.b];
    const section = !a.cap && !b.cap && a.cell != b.cell;
    if (choice == KCreaseSelection.SECTION)
        return section;
    if (a.cap || b.cap || section)
        return false;
    const diagonal = floor(a.order / 2) == floor(b.order / 2);
    return choice == KCreaseSelection.DIAGONAL ? diagonal : !diagonal;
}

function kSpaceModules(faces is array, d is map) returns array
{
    var cursor = 0;
    for (var cell = 0; cell < d.cells; cell += 1)
    {
        var xmin = 1e10;
        var xmax = -1e10;
        var ymin = 1e10;
        for (var f in faces)
            if (kModuleOf(f, d) == cell)
                for (var p in f.pts)
                {
                    xmin = min(xmin, p[0]);
                    xmax = max(xmax, p[0]);
                    ymin = min(ymin, p[1]);
                }
        const shift = vector(cursor - xmin, -ymin, 0);
        for (var i = 0; i < size(faces); i += 1)
            if (kModuleOf(faces[i], d) == cell)
            {
                var pts = [];
                for (var p in faces[i].pts)
                    pts = append(pts, p + shift);
                faces[i].pts = pts;
                faces[i].c = kCentroid(pts);
            }
        cursor += xmax - xmin + d.moduleSpacing / millimeter;
    }
    return faces;
}

function kClipAnchor(interval is array, face is map, gaps is array, origin is Vector, ev is Vector,
                     offset is Vector, radius is number) returns array
{
    for (var e = 0; e < size(face.pts); e += 1)
    {
        const u = kEdgeInward(face.pts, e, face.c);
        const need = gaps[e] + radius - dot(origin + offset - face.pts[e], u);
        const slope = dot(ev, u);
        if (abs(slope) < 1e-9)
        {
            if (need > 1e-6)
                throw regenError("A connection footprint does not fit its panel. Increase inset or panel size, or reduce the anchor/boss width.", ["anchorEngagement", "anchorHeadD", "seamEyeletDepth", "seamEyeletW"]);
        }
        else if (slope > 0)
            interval[0] = max(interval[0], need / slope);
        else
            interval[1] = min(interval[1], need / slope);
    }
    return interval;
}

function kJoinBodies(context is Context, id is Id, bodies is array) returns Query
{
    const q = qBodyType(qUnion(bodies), BodyType.SOLID);
    if (size(evaluateQuery(context, q)) > 1)
        opBoolean(context, id, { "tools" : q, "operationType" : BooleanOperationType.UNION });
    return q;
}

function kNameBodies(context is Context, bodies is Query, prefix is string, appearance is Color, d is map)
{
    var index = 1;
    for (var body in evaluateQuery(context, bodies))
    {
        setProperty(context, { "entities" : body, "propertyType" : PropertyType.NAME, "value" : prefix ~ "__part_" ~ index });
        if (d.colorManufacturingRoles)
            setProperty(context, { "entities" : body, "propertyType" : PropertyType.APPEARANCE, "value" : appearance });
        index += 1;
    }
}

function kCylinder(context is Context, id is Id, center is Vector, normal is Vector, radius is number, depth is number)
{
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(center * millimeter, normal) });
    skCircle(sk, "circle", { "center" : vector(0, 0) * millimeter, "radius" : radius * millimeter });
    skSolve(sk);
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : normal,
        "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

function kRectangle(origin is Vector, x is Vector, y is Vector, xmin is number, xmax is number, ymin is number, ymax is number) returns array
{
    return [origin + xmin * x + ymin * y, origin + xmax * x + ymin * y,
            origin + xmax * x + ymax * y, origin + xmin * x + ymax * y];
}

function kAnchor(context is Context, id is Id, face is map, keepouts is array, base is Vector, ev is Vector,
                 u is Vector, gap is number, outer is boolean, webT is number, panel is Query, d is map)
{
    const mm = millimeter;
    const headR = d.anchorHeadD / mm / 2;
    const shaftR = d.anchorShaftD / mm / 2;
    const headT = d.anchorHeadH / mm;
    const embed = d.anchorEngagement / mm;
    const center = base + (gap + embed) * u;
    const envelope = headR * (d.anchorProfile == KAnchorProfile.BEAM_KEY ? sqrt(2) : 1);
    for (var hole in keepouts)
        if (norm(center - hole.center) < hole.radius + envelope + d.anchorLigament / mm)
            throw regenError("An anchor pocket reaches a lacing, ventilation or end-plate hole. Reduce holes or change anchor spacing.", ["ventD", "holeD", "seamEyeletDepth", "anchorCount"]);
    const beamOrigin = base + (outer ? 0 : face.t - webT) * face.n;
    const beam = kRectangle(beamOrigin, ev, u, -d.anchorBeamW / mm / 2, d.anchorBeamW / mm / 2,
                            gap + d.overlap / mm / 2, gap + embed);
    kExtrudePoly(context, id + "beam", beam, face.n, webT);
    if (d.anchorProfile == KAnchorProfile.RIVET)
    {
        kCylinder(context, id + "shaft", center, face.n, shaftR, face.t);
        kCylinder(context, id + "outerHead", center, face.n, headR, headT);
        kCylinder(context, id + "innerHead", center + (face.t - headT) * face.n, face.n, headR, headT);
    }
    else
    {
        kExtrudePoly(context, id + "shaft", kRectangle(center, ev, u, -shaftR, shaftR, -shaftR, shaftR), face.n, face.t);
        const head = kRectangle(center, ev, u, -headR, headR, -headR, headR);
        kExtrudePoly(context, id + "outerHead", head, face.n, headT);
        var inner = [];
        for (var p in head)
            inner = append(inner, p + (face.t - headT) * face.n);
        kExtrudePoly(context, id + "innerHead", inner, face.n, headT);
    }
    if (!kHasVolumeContact(context, qCreatedBy(id + "shaft" + "ext", EntityType.BODY), panel))
        throw regenError("An anchor shaft does not pass through its intended panel.");
}

function kNeedsEyelets(cr is map, faces is array, d is map) returns boolean
{
    const a = faces[cr.a];
    const b = faces[cr.b];
    if (a.cap || b.cap)
        return false;
    if (a.cell != b.cell)
        return d.manufacturingMode == KManufacturing.MODULAR && d.sectionConnection == KSectionConnection.LACING;
    return d.seamConnection == KSeamConnection.LACING && abs(a.order - b.order) == 2 * d.sides - 1;
}

function kEyelets(context is Context, id is Id, face is map, edge is number, gaps is array, firstSide is boolean, peer is map, d is map) returns array
{
    const mm = millimeter;
    const p0 = face.pts[edge];
    const p1 = face.pts[(edge + 1) % size(face.pts)];
    const ev = normalize(p1 - p0);
    const u = kEdgeInward(face.pts, edge, face.c);
    const radius = d.seamEyeletW / mm / 2;
    const gap = gaps[edge];
    const horizontal = d.connectionLayout == KConnectionLayout.HORIZONTAL;
    const row = kConnectionRowDirection(face);
    const pitch = d.connectionHolePitch / mm;
    const targetDepth = kEyeletTargetDepth(face, edge, gap, d);
    var interval = [radius + d.clearance / mm, norm(p1 - p0) - radius - d.clearance / mm];
    if (horizontal)
    {
        for (var i = 0; i < d.seamEyeletCount; i += 1)
        {
            const delta = (i - (d.seamEyeletCount - 1) / 2) * pitch * row;
            if (d.projectEyeletTabs)
                interval = kClipAttachment(interval, face, edge, gaps, p0 + dot(delta, ev) * ev, ev, u, gap, d.overlap / mm, radius);
            else
                interval = kClipAnchor(interval, face, gaps, p0, ev, targetDepth * u + delta,
                    radius + max(d.minWall / mm, d.roundPanels ? d.cornerR / mm : 0));
        }
    }
    else if (d.projectEyeletTabs)
        interval = kClipAttachment(interval, face, edge, gaps, p0, ev, u, gap, d.overlap / mm, radius);
    else
        interval = kClipAnchor(interval, face, gaps, p0, ev, targetDepth * u,
            radius + max(d.minWall / mm, d.roundPanels ? d.cornerR / mm : 0));
    const length = interval[1] - interval[0];
    if (length < 0 || (!horizontal && length / (2 * d.seamEyeletCount) < 2 * radius + d.clearance / mm))
        throw regenError("The connection-hole pattern does not fit its panel. Reduce count/spacing or increase inset or panel size.", ["seamEyeletCount", "connectionHolePitch", "seamEyeletDepth"]);
    const ascending = face.ids[edge] < face.ids[(edge + 1) % size(face.ids)];
    var rowDistance = (interval[0] + interval[1]) / 2;
    if (horizontal && face.cell == peer.face.cell)
        rowDistance = kPairedRowStation(face, edge, interval, peer, d);
    var holes = [];
    for (var i = 0; i < d.seamEyeletCount; i += 1)
    {
        const fraction = (2 * i + (firstSide ? 0.5 : 1.5)) / (2 * d.seamEyeletCount);
        const distance = horizontal ? rowDistance :
            (ascending ? interval[0] + length * fraction : interval[1] - length * fraction);
        const delta = horizontal ? (i - (d.seamEyeletCount - 1) / 2) * pitch * row : vector(0, 0, 0);
        const base = p0 + distance * ev;
        const center = base + targetDepth * u + delta;
        const partId = id + ("eyelet" ~ i);
        if (d.projectEyeletTabs)
        {
            const neckBase = base + dot(delta, ev) * ev;
            const neckDepth = targetDepth + dot(delta, u);
            const neck = kRectangle(neckBase, ev, u, -radius, radius, neckDepth, gap + d.overlap / mm);
            kExtrudePoly(context, partId + "neck", neck, face.n, face.t);
        }
        const collar = d.projectEyeletTabs ? 0 : d.eyeletCollarHeight / mm;
        kCylinder(context, partId + "tip", center, face.n, radius, face.t + collar);
        const ventilationR = d.ventD / mm / 2;
        if (ventilationR > 0 && norm(center - kCentroid(kInset(face.pts, gaps, face.c))) < ventilationR + d.seamEyeletHoleD / mm / 2 + d.ligament / mm)
            throw regenError("A lacing eyelet leaves insufficient ligament to a ventilation hole.", ["ventD", "seamEyeletDepth"]);
        const rib = d.ribs ? d.ribH / mm : 0;
        holes = append(holes, { "center" : center, "keepoutCenter" : center, "keepoutRadius" : d.seamEyeletHoleD / mm / 2,
            "start" : center, "axis" : face.n, "depth" : face.t + max(collar, rib) });
    }
    return holes;
}

// Intersect a convex panel footprint with a half-plane, without new CAD bodies.
function kClipFootprint(pts is array, origin is Vector, inward is Vector) returns array
{
    if (size(pts) == 0)
        return [];
    var out = [];
    for (var i = 0; i < size(pts); i += 1)
    {
        const a = pts[i];
        const b = pts[(i + 1) % size(pts)];
        const da = dot(a - origin, inward);
        const db = dot(b - origin, inward);
        if (da >= -1e-8)
            out = append(out, a);
        if ((da > 1e-8 && db < -1e-8) || (da < -1e-8 && db > 1e-8))
            out = append(out, a + da / (da - db) * (b - a));
    }
    var clean = [];
    for (var p in out)
        if (size(clean) == 0 || norm(p - clean[size(clean) - 1]) > 1e-6)
            clean = append(clean, p);
    if (size(clean) > 1 && norm(clean[0] - clean[size(clean) - 1]) < 1e-6)
        clean = subArray(clean, 0, size(clean) - 1);
    return clean;
}

function kRibbonOffset(nA is Vector, nB is Vector, a is number, b is number) returns Vector
{
    if (dot(nA, nB) > 0.999999)
        return (a + b) / 2 * nA;
    return kOffset(nA, nB, a, b);
}

function kModifier(context is Context, id is Id, spec is map, panelA is Query, panelB is Query, otherPanels is Query, d is map) returns Query
{
    const mm = millimeter;
    const a = spec.faceA;
    const b = spec.faceB;
    const gapWidth = spec.gA + spec.gB;
    const width = d.modifierWidthMode == KModifierWidth.RELATIVE ? gapWidth * d.modifierWidthMultiplier : d.modifierWidth / mm;
    const extraA = (width - gapWidth) / 2 + d.modifierOffset / mm;
    const extraB = (width - gapWidth) / 2 - d.modifierOffset / mm;
    if (min(extraA, extraB) < 0.02)
        throw regenError("The ribbon must overlap both panel edges. Increase width multiplier or reduce offset.", ["modifierWidthMultiplier", "modifierWidth", "modifierOffset"]);
    const allowance = d.modifierHeight / mm;
    const lowA = spec.outer ? -allowance : max(0, a.t - d.modifierDepth / mm);
    const lowB = spec.outer ? -allowance : max(0, b.t - d.modifierDepth / mm);
    const highA = spec.outer ? min(a.t, d.modifierDepth / mm) : a.t + allowance;
    const highB = spec.outer ? min(b.t, d.modifierDepth / mm) : b.t + allowance;
    const wa = spec.gA + min(extraA, 0.03);
    const wb = spec.gB + min(extraB, 0.03);
    const p = spec.start;
    const lowMiddle = p + kRibbonOffset(a.n, b.n, lowA, lowB);
    const highMiddle = p + kRibbonOffset(a.n, b.n, highA, highB);
    const section = [p + wa * spec.uA + lowA * a.n, lowMiddle, p + wb * spec.uB + lowB * b.n,
        p + wb * spec.uB + highB * b.n, highMiddle, p + wa * spec.uA + highA * a.n];
    kExtrudePoly(context, id + "spine", section, spec.ev, spec.length);
    var pieces = [qCreatedBy(id + "spine" + "ext", EntityType.BODY)];
    const sides = [{ "face" : a, "gaps" : spec.gapsA, "u" : spec.uA, "extent" : spec.gA + extraA, "low" : lowA, "high" : highA },
                   { "face" : b, "gaps" : spec.gapsB, "u" : spec.uB, "extent" : spec.gB + extraB, "low" : lowB, "high" : highB }];
    for (var i = 0; i < 2; i += 1)
    {
        const side = sides[i];
        var footprint = kInset(side.face.pts, side.gaps, side.face.c);
        footprint = kClipFootprint(footprint, p, spec.ev);
        footprint = kClipFootprint(footprint, p + spec.length * spec.ev, -spec.ev);
        footprint = kClipFootprint(footprint, p + side.extent * side.u, -side.u);
        if (size(footprint) < 3 || norm(cross(footprint[1] - footprint[0], footprint[2] - footprint[0])) < 1e-8)
            continue;
        var shifted = [];
        for (var point in footprint)
            shifted = append(shifted, point + side.low * side.face.n);
        const wingId = id + ("wing" ~ i);
        kExtrudePoly(context, wingId, shifted, side.face.n, side.high - side.low);
        pieces = append(pieces, qCreatedBy(wingId + "ext", EntityType.BODY));
    }
    const ribbon = kJoinBodies(context, id + "union", pieces);
    // Only the modifier is trimmed. Structural panels are never modified by a
    // slicer volume, and a full-length spine replaces the old end shortening.
    if (kHasVolumeContact(context, ribbon, otherPanels))
        opBoolean(context, id + "excludeUnrelated", { "tools" : otherPanels, "targets" : ribbon,
            "operationType" : BooleanOperationType.SUBTRACTION, "keepTools" : true });
    if (size(evaluateQuery(context, ribbon)) != 1)
        throw regenError("Clipping this ribbon would interrupt continuous seam coverage. Reduce ribbon width/depth or change the design.");
    if (!kHasVolumeContact(context, ribbon, panelA) || !kHasVolumeContact(context, ribbon, panelB))
        throw regenError("The ribbon does not cover both intended panels. Increase its width or penetration.", ["modifierWidthMultiplier", "modifierDepth"]);
    const middle = (lowMiddle + highMiddle) / 2;
    for (var fraction in [0.001, 0.25, 0.5, 0.75, 0.999])
        if (size(evaluateQuery(context, qContainsPoint(ribbon, (middle + fraction * spec.length * spec.ev) * mm))) == 0)
            throw regenError("An unrelated panel cuts across the ribbon centerline. Adjust the design or use printed-hinge coverage.", ["modifierCoverage", "modifierDepth"]);
    const ribbonBounds = evBox3d(context, { "topology" : ribbon,
        "cSys" : coordSystem(p * mm, spec.ev, a.n), "tight" : true });
    if (abs(ribbonBounds.minCorner[0] / mm) > 1e-5 || abs(ribbonBounds.maxCorner[0] / mm - spec.length) > 1e-5)
        throw regenError("Modifier clipping lost an endpoint. Increase the explicit end margin or reduce depth.", ["modifierStartMargin", "modifierEndMargin"]);
    kNameBodies(context, ribbon, "MODIFIER_ONLY_NOT_PRINT__crease_" ~ spec.crease.manufacturingId, color(0.95, 0.55, 0.1), d);
    return ribbon;
}

function kReferencePoint(face is map, point is Vector) returns Vector
{
    const o = face.referencePts[0];
    const x = normalize(face.referencePts[1] - o);
    const y = cross(face.referenceN, x);
    const nx = normalize(face.pts[1] - face.pts[0]);
    const ny = cross(face.n, nx);
    const relative = point - o;
    return face.pts[0] + dot(relative, x) * nx + dot(relative, y) * ny + dot(relative, face.referenceN) * face.n;
}

function kConnectionRowDirection(face is map) returns Vector
{
    const netX = normalize(face.netPts[1] - face.netPts[0]);
    const netY = cross(vector(0, 0, 1), netX);
    const currentX = normalize(face.pts[1] - face.pts[0]);
    const currentY = cross(face.n, currentX);
    return normalize(dot(vector(1, 0, 0), netX) * currentX + dot(vector(1, 0, 0), netY) * currentY);
}

function kReferenceFace(face is map) returns map
{
    return mergeMaps(face, { "pts" : face.referencePts, "n" : face.referenceN, "c" : face.referenceC });
}

function kMoveFromReference(context is Context, id is Id, bodies is Query, face is map)
{
    const referenceSystem = coordSystem(face.referencePts[0] * millimeter, normalize(face.referencePts[1] - face.referencePts[0]), face.referenceN);
    const currentSystem = coordSystem(face.pts[0] * millimeter, normalize(face.pts[1] - face.pts[0]), face.n);
    opTransform(context, id, { "bodies" : bodies, "transform" : toWorld(currentSystem) * fromWorld(referenceSystem) });
}

function kHull(points is array, normal is Vector) returns array
{
    var pts = [];
    for (var p in points)
    {
        var duplicate = false;
        for (var q in pts)
            if (norm(p - q) < 1e-7)
                duplicate = true;
        if (!duplicate)
            pts = append(pts, p);
    }
    var first = 0;
    const x = normalize(pts[1] - pts[0]);
    const y = cross(normal, x);
    for (var i = 1; i < size(pts); i += 1)
        if (dot(pts[i], x) < dot(pts[first], x) - 1e-8 ||
            (abs(dot(pts[i] - pts[first], x)) < 1e-8 && dot(pts[i], y) < dot(pts[first], y)))
            first = i;
    var out = [];
    var current = first;
    for (var step = 0; step <= size(pts); step += 1)
    {
        out = append(out, pts[current]);
        var next = (current + 1) % size(pts);
        for (var i = 0; i < size(pts); i += 1)
        {
            if (i == current)
                continue;
            const turn = dot(normal, cross(pts[next] - pts[current], pts[i] - pts[current]));
            if (turn < -1e-8 || (abs(turn) < 1e-8 && norm(pts[i] - pts[current]) > norm(pts[next] - pts[current])))
                next = i;
        }
        current = next;
        if (current == first)
            return out;
    }
    throw regenError("A joint support profile could not be constructed.");
}

function kOutsetConvex(pts is array, distance is number) returns array
{
    const c = kCentroid(pts);
    var out = [];
    for (var i = 0; i < size(pts); i += 1)
    {
        const u = kEdgeInward(pts, (i + size(pts) - 1) % size(pts), c);
        const v = kEdgeInward(pts, i, c);
        out = append(out, pts[i] - distance * (u + v) / (1 + dot(u, v)));
    }
    return out;
}

function kSketchPoint(point is Vector, origin is Vector, x is Vector, y is Vector) returns Vector
{
    return vector(dot(point - origin, x), dot(point - origin, y)) * millimeter;
}

// Analytic circular sketch arcs, not a faceted imitation of a corner fillet.
function kExtrudeRounded(context is Context, id is Id, pts is array, dir is Vector, depth is number, radius is number)
{
    const count = size(pts);
    const normal = normalize(cross(pts[1] - pts[0], pts[2] - pts[0]));
    var corners = [];
    for (var i = 0; i < count; i += 1)
    {
        const a = normalize(pts[(i + count - 1) % count] - pts[i]);
        const b = normalize(pts[(i + 1) % count] - pts[i]);
        const half = acos(max(-1, min(1, dot(a, b)))) / 2;
        const tangent = radius / tan(half);
        const center = pts[i] + radius / sin(half) * normalize(a + b);
        const start = pts[i] + tangent * a;
        const end = pts[i] + tangent * b;
        const r0 = normalize(start - center);
        const r1 = normalize(end - center);
        const sweep = atan2(dot(normal, cross(r0, r1)), dot(r0, r1));
        const mid = center + radius * (cos(sweep / 2) * r0 + sin(sweep / 2) * cross(normal, r0));
        corners = append(corners, { "start" : start, "mid" : mid, "end" : end, "tangent" : tangent });
    }
    const x = normalize(pts[1] - pts[0]);
    const y = cross(normalize(dir), x);
    const origin = pts[0];
    const sk = newSketchOnPlane(context, id + "sk", { "sketchPlane" : plane(origin * millimeter, dir, x) });
    for (var i = 0; i < count; i += 1)
    {
        const next = (i + 1) % count;
        if (corners[i].tangent + corners[next].tangent >= norm(pts[next] - pts[i]) - 1e-5)
            throw regenError("Dovetail corner radius consumes a profile edge. Reduce the radius.", ["dovetailRadius"]);
        skArc(sk, "arc" ~ i, { "start" : kSketchPoint(corners[i].start, origin, x, y),
            "mid" : kSketchPoint(corners[i].mid, origin, x, y), "end" : kSketchPoint(corners[i].end, origin, x, y) });
        skLineSegment(sk, "edge" ~ i, { "start" : kSketchPoint(corners[i].end, origin, x, y),
            "end" : kSketchPoint(corners[next].start, origin, x, y) });
    }
    skSolve(sk);
    if (size(evaluateQuery(context, qSketchRegion(id + "sk"))) != 1)
        throw regenError("The rounded joint profile does not enclose one region.");
    opExtrude(context, id + "ext", { "entities" : qSketchRegion(id + "sk"), "direction" : dir,
        "endBound" : BoundingType.BLIND, "endDepth" : depth * millimeter });
    opDeleteBodies(context, id + "del", { "entities" : qCreatedBy(id + "sk", EntityType.BODY) });
}

function kDovetailPair(context is Context, id is Id, cr is map, faces is array, gaps is array,
                       panels is array, keepouts is array, d is map) returns array
{
    const mm = millimeter;
    const a = faces[cr.a];
    const b = faces[cr.b];
    const ra = kReferenceFace(a);
    const rb = kReferenceFace(b);
    const ia = cr.ia;
    const ib = cr.ib;
    const ascending = ra.ids[ia] < ra.ids[(ia + 1) % size(ra.ids)];
    const p0 = ra.pts[ascending ? ia : (ia + 1) % size(ra.pts)];
    const p1 = ra.pts[ascending ? (ia + 1) % size(ra.pts) : ia];
    const ev = normalize(p1 - p0);
    if (norm(ra.n + rb.n) < 1e-5)
        throw regenError("Dovetail seam normals are ill-conditioned. Change twist or use a lacing seam.");
    const inside = normalize(ra.n + rb.n);
    var across = normalize(cross(ev, inside));
    const ua = kEdgeInward(ra.pts, ia, ra.c);
    const ub = kEdgeInward(rb.pts, ib, rb.c);
    if (dot(across, ub) < 0)
        across = -across;
    const normal = normalize(cross(across, inside));
    const ga = gaps[cr.a][ia];
    const gb = gaps[cr.b][ib];
    const width = d.dovetailWidth / mm;
    const depth = d.dovetailDepth / mm;
    const wall = d.dovetailWall / mm;
    const clearance = d.dovetailClearance / mm;
    const radius = d.dovetailRadius / mm;
    const neck = width - 2 * depth * tan(d.dovetailAngle);
    const halfHeight = width / 2 + wall;
    const maxX = depth + 2 * wall + clearance;
    const height = halfHeight + max((a.t + clearance + maxX * abs(dot(across, ra.n))) / dot(inside, ra.n),
                                   (b.t + clearance + maxX * abs(dot(across, rb.n))) / dot(inside, rb.n));
    var interval = [d.relief / mm + clearance, norm(p1 - p0) - d.relief / mm - clearance - (d.dovetailStop ? wall : 0)];
    interval = kClipAttachment(interval, ra, ia, gaps[cr.a], p0, ev, ua, ga, d.overlap / mm, radius);
    interval = kClipAttachment(interval, rb, ib, gaps[cr.b], p0, ev, ub, gb, d.overlap / mm, radius);
    const assisted = d.dovetailMaterial == KDovetailMaterial.TPU_MALE;
    if (assisted)
    {
        const headRadius = d.anchorHeadD / mm / 2 * (d.anchorProfile == KAnchorProfile.BEAM_KEY ? sqrt(2) : 1);
        interval = kClipAnchor(interval, ra, gaps[cr.a], p0, ev, (ga + d.anchorEngagement / mm) * ua,
                               headRadius + d.anchorLigament / mm);
    }
    const available = interval[1] - interval[0];
    const count = d.dovetailLayout == KDovetailLayout.CONTINUOUS ? 1 : d.dovetailCount;
    const length = d.dovetailLayout == KDovetailLayout.CONTINUOUS ? available : d.dovetailLength / mm;
    const occupied = length + 2 * clearance + (d.dovetailStop ? wall : 0);
    if (length < 3 || available < length || (count > 1 && available / count < occupied + d.dovetailSpacing / mm))
        throw regenError("Dovetail segments do not fit with receiver and insertion clearance. Reduce count/length or increase cell height.", ["dovetailCount", "dovetailLength", "dovetailSpacing"]);
    var records = [];
    for (var i = 0; i < count; i += 1)
    {
        const middle = (interval[0] + interval[1]) / 2 + (i - (count - 1) / 2) * available / count;
        const from = middle - length / 2;
        const origin = p0 + from * ev;
        const joint = origin + height * inside;
        const item = id + ("segment" ~ i);
        const maleId = item + "male";
        const femaleId = item + "female";
        const trapezoid = [joint - neck / 2 * inside, joint + depth * across - width / 2 * inside,
                           joint + depth * across + width / 2 * inside, joint + neck / 2 * inside];
        kExtrudeRounded(context, maleId + "key", trapezoid, ev, length, radius);
        if (d.dovetailLeadInEnabled)
            opChamfer(context, maleId + "leadIn", { "entities" : qCapEntity(maleId + "key" + "ext", CapType.START, EntityType.EDGE),
                "chamferType" : ChamferType.EQUAL_OFFSETS, "width" : d.dovetailLeadIn, "tangentPropagation" : false });
        kExtrudePoly(context, maleId + "neck", kRectangle(joint, across, inside, -2 * wall - clearance, 0.2, -neck / 2, neck / 2), ev, length);
        const dockA = kRectangle(joint, across, inside, -2 * wall - clearance, -wall - clearance, -neck / 2, neck / 2);
        const rootA = kRectangle(origin, ua, ra.n, ga + 0.1, ga + d.overlap / mm, a.t - min(a.t, d.minWall / mm), a.t);
        kExtrudePoly(context, maleId + "support", kHull(concatenateArrays([rootA, dockA]), normal), ev, length);
        const male = kJoinBodies(context, maleId + "union", [qBodyType(qCreatedBy(maleId, EntityType.BODY), BodyType.SOLID)]);
        const receiverStart = origin - clearance * ev;
        const receiverJoint = joint - clearance * ev;
        const receiverOuter = kRectangle(receiverJoint, across, inside, -wall, depth + wall, -halfHeight, halfHeight);
        kExtrudeRounded(context, femaleId + "sleeve", receiverOuter, ev, length + 2 * clearance, min(radius, wall / 3));
        const dockB = kRectangle(receiverJoint, across, inside, depth + 0.7 * wall, depth + wall, -width / 2, width / 2);
        const rootB = kRectangle(receiverStart, ub, rb.n, gb + 0.1, gb + d.overlap / mm, b.t - min(b.t, d.minWall / mm), b.t);
        kExtrudePoly(context, femaleId + "support", kHull(concatenateArrays([rootB, dockB]), normal), ev, length + 2 * clearance);
        const female = kJoinBodies(context, femaleId + "union", [qBodyType(qCreatedBy(femaleId, EntityType.BODY), BodyType.SOLID)]);
        var toolProfile = [];
        for (var p in kOutsetConvex(trapezoid, clearance))
            toolProfile = append(toolProfile, p - (clearance + 0.1) * ev);
        kExtrudeRounded(context, item + "cavity", toolProfile, ev, length + 2 * clearance + 0.2, radius + clearance);
        kExtrudePoly(context, item + "mouth", kRectangle(joint - (clearance + 0.1) * ev, across, inside,
            -wall - 0.1, 0.25, -neck / 2 - clearance, neck / 2 + clearance), ev, length + 2 * clearance + 0.2);
        opBoolean(context, item + "socketCut", { "tools" : qUnion([qCreatedBy(item + "cavity" + "ext", EntityType.BODY),
            qCreatedBy(item + "mouth" + "ext", EntityType.BODY)]), "targets" : female, "operationType" : BooleanOperationType.SUBTRACTION });
        if (d.dovetailStop)
        {
            var stop = [];
            for (var p in receiverOuter)
                stop = append(stop, p + (length + 2 * clearance - 0.01) * ev);
            kExtrudeRounded(context, femaleId + "stop", stop, ev, wall + 0.01, min(radius, wall / 3));
            kJoinBodies(context, femaleId + "stopUnion", [female, qCreatedBy(femaleId + "stop" + "ext", EntityType.BODY)]);
        }
        if (size(evaluateQuery(context, male)) != 1 || size(evaluateQuery(context, female)) != 1 || kHasVolumeContact(context, male, female))
            throw regenError("The dovetail halves interfere or a support is disconnected. Adjust angle, clearance or dimensions.");
        records = append(records, { "male" : male, "female" : female, "middle" : p0 + middle * ev, "length" : length });
    }
    var maleBodies = [];
    var femaleBodies = [];
    for (var record in records)
    {
        maleBodies = append(maleBodies, record.male);
        femaleBodies = append(femaleBodies, record.female);
    }
    // Check sampled axial entry positions in the shared folded datum before
    // mapping each side back to its own panel. This is not a folding simulation.
    for (var step = 0; step < 3; step += 1)
    {
        const probeId = id + ("entryProbe" ~ step);
        opPattern(context, probeId, { "entities" : qUnion(maleBodies),
            "transforms" : [transform(-step / 2 * (length + 2 * clearance) * ev * mm)], "instanceNames" : ["probe"] });
        const probe = qCreatedBy(probeId, EntityType.BODY);
        if (kHasVolumeContact(context, probe, qUnion(femaleBodies)))
            throw regenError("The dovetail insertion samples hit a receiver or stop. Increase key spacing or disable end stops.", ["dovetailSpacing", "dovetailStop"]);
        opDeleteBodies(context, probeId + "delete", { "entities" : probe });
    }
    var out = [];
    for (var i = 0; i < size(records); i += 1)
    {
        var record = records[i];
        const transformId = id + ("mapped" ~ i);
        kMoveFromReference(context, transformId + "male", record.male, a);
        kMoveFromReference(context, transformId + "female", record.female, b);
        if (assisted)
        {
            const anchorId = transformId + "anchor";
            const base = kReferencePoint(a, record.middle);
            const currentEv = normalize(kReferencePoint(a, record.middle + ev) - base);
            const currentU = normalize(kReferencePoint(a, record.middle + ua) - base);
            kAnchor(context, anchorId, a, keepouts[cr.a], base, currentEv, currentU, ga, false,
                min((d.hingeControl == KHinge.THICKNESS ? d.hingeT : d.panelT - d.creaseDepth) / mm, 0.8 * a.t), panels[cr.a], d);
            record.male = kJoinBodies(context, transformId + "anchorUnion", [record.male, qBodyType(qCreatedBy(anchorId, EntityType.BODY), BodyType.SOLID)]);
            kNameBodies(context, record.male, "TPU__dovetail_male__cell_" ~ (a.cell + 1) ~ "__segment_" ~ (i + 1), color(0.15, 0.65, 0.4), d);
        }
        if (!kHasVolumeContact(context, record.male, panels[cr.a]) || !kHasVolumeContact(context, record.female, panels[cr.b]))
            throw regenError("A dovetail support does not overlap its owning panel.");
        if (size(evaluateQuery(context, record.male)) != 1 || size(evaluateQuery(context, record.female)) != 1)
            throw regenError("A dovetail half is disconnected after mapping or anchoring.");
        out = append(out, { "body" : record.male, "owner" : cr.a, "mate" : cr.b, "tpu" : assisted });
        out = append(out, { "body" : record.female, "owner" : cr.b, "mate" : cr.a, "tpu" : false });
    }
    return out;
}

function kPairedRowStation(face is map, edge is number, interval is array, peer is map, d is map) returns number
{
    const mm = millimeter;
    const other = peer.face;
    const ownNetEdge = normalize(face.netPts[(edge + 1) % size(face.netPts)] - face.netPts[edge]);
    const otherNetEdge = normalize(other.netPts[(peer.edge + 1) % size(other.netPts)] - other.netPts[peer.edge]);
    // Tapered developments can be fan-shaped: the two edge copies then do not
    // share one horizontal datum without changing the panels. Keep each row
    // horizontal locally; pair exact levels only for parallel edge copies.
    if (norm(cross(ownNetEdge, otherNetEdge)) > 1e-6)
        return (interval[0] + interval[1]) / 2;
    const p0 = other.pts[peer.edge];
    const p1 = other.pts[(peer.edge + 1) % size(other.pts)];
    const ev = normalize(p1 - p0);
    const u = kEdgeInward(other.pts, peer.edge, other.c);
    const radius = d.seamEyeletW / mm / 2;
    const target = kEyeletTargetDepth(other, peer.edge, peer.gaps[peer.edge], d);
    var otherInterval = [radius + d.clearance / mm, norm(p1 - p0) - radius - d.clearance / mm];
    const row = kConnectionRowDirection(other);
    for (var i = 0; i < d.seamEyeletCount; i += 1)
    {
        const delta = (i - (d.seamEyeletCount - 1) / 2) * d.connectionHolePitch / mm * row;
        if (d.projectEyeletTabs)
            otherInterval = kClipAttachment(otherInterval, other, peer.edge, peer.gaps, p0 + dot(delta, ev) * ev, ev, u,
                peer.gaps[peer.edge], d.overlap / mm, radius);
        else
            otherInterval = kClipAnchor(otherInterval, other, peer.gaps, p0, ev, target * u + delta,
                radius + max(d.minWall / mm, d.roundPanels ? d.cornerR / mm : 0));
    }
    const specs = [{ "face" : face, "edge" : edge, "interval" : interval },
                   { "face" : other, "edge" : peer.edge, "interval" : otherInterval }];
    // Compare both feasible row-height bands in the unrotated print datum.
    const ownGap = d.projectEyeletTabs ? 0 : peer.ownGap;
    var bands = [];
    for (var i = 0; i < 2; i += 1)
    {
        const entry = specs[i];
        const pts = entry.face.netPts;
        const dir = normalize(pts[(entry.edge + 1) % size(pts)] - pts[entry.edge]);
        const inward = kEdgeInward(pts, entry.edge, kCentroid(pts));
        const offset = i == 0 ? kEyeletTargetDepth(face, edge, ownGap, d) : target;
        const y0 = pts[entry.edge][1] + offset * inward[1];
        const ya = y0 + entry.interval[0] * dir[1];
        const yb = y0 + entry.interval[1] * dir[1];
        bands = append(bands, { "lo" : min(ya, yb), "hi" : max(ya, yb), "origin" : y0, "slope" : dir[1] });
    }
    const lo = max(bands[0].lo, bands[1].lo);
    const hi = min(bands[0].hi, bands[1].hi);
    if (hi < lo - 1e-6)
        throw regenError("The paired seam rows cannot share a horizontal datum inside both panels. Reduce hole count/inset or enlarge the cell; legacy layout is available.", ["seamEyeletCount", "seamEyeletDepth", "connectionLayout"]);
    if (abs(bands[0].slope) < 1e-8)
        return (interval[0] + interval[1]) / 2;
    return ((lo + hi) / 2 - bands[0].origin) / bands[0].slope;
}

function kEyeletTargetDepth(face is map, edge is number, gap is number, d is map) returns number
{
    const radius = d.seamEyeletW / millimeter / 2;
    if (d.projectEyeletTabs)
        return -d.seamEyeletDepth / millimeter + radius;
    var inset = d.seamEyeletDepth / millimeter;
    if (d.connectionLayout == KConnectionLayout.HORIZONTAL)
    {
        const u = kEdgeInward(face.pts, edge, face.c);
        const extent = (d.seamEyeletCount - 1) / 2 * d.connectionHolePitch / millimeter;
        const ligament = max(d.minWall / millimeter, d.roundPanels ? d.cornerR / millimeter : 0);
        inset = max(inset, radius + ligament + extent * abs(dot(kConnectionRowDirection(face), u)));
    }
    return gap + inset;
}
