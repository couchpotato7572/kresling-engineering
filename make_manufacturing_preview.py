"""Reference cross-section of a captured hinge, not an Onshape rendering."""
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon as Patch, Rectangle
from shapely.geometry import box
from shapely.ops import unary_union


def draw(ax, geom, fill, edge):
    geoms = geom.geoms if hasattr(geom, 'geoms') else [geom]
    for polygon in geoms:
        ax.add_patch(Patch(list(polygon.exterior.coords), facecolor=fill, edgecolor=edge, linewidth=.9))


def main():
    thickness, web, gap, overlap, embed = 1.2, .4, .8, .8, 2.2
    head_width, shaft_width, head_height = 2.6, 1.6, .4
    keys = [box(-gap-overlap, thickness-web, gap+overlap, thickness)]
    for sign in [-1, 1]:
        center = sign*(gap+embed)
        low, high = sorted([sign*(gap+overlap/2), center])
        keys.extend([box(low, thickness-web, high, thickness),
                     box(center-shaft_width/2, 0, center+shaft_width/2, thickness),
                     box(center-head_width/2, 0, center+head_width/2, head_height),
                     box(center-head_width/2, thickness-head_height, center+head_width/2, thickness)])
    tpu = unary_union(keys)
    panels = unary_union([box(-7, 0, -gap, thickness), box(gap, 0, 7, thickness)])
    pla = panels.difference(tpu)
    fig, ax = plt.subplots(figsize=(12, 4), facecolor='#f6f8fa')
    ax.set_facecolor('#f6f8fa')
    draw(ax, pla, '#7ca6db', '#244b70')
    draw(ax, tpu, '#64b68e', '#1d6345')
    ax.add_patch(Rectangle((-4, 0), 8, 1.6, fill=False, edgecolor='#e59025', linestyle='--', linewidth=1.8))
    ax.annotate('Rigid layer captures the wider TPU heads', xy=(-3.9, .6), xytext=(-6.8, 2.0),
                arrowprops={'arrowstyle': '->', 'color': '#244b70'}, color='#244b70')
    ax.annotate('Flexible web', xy=(0, 1), xytext=(-.5, 2.0),
                arrowprops={'arrowstyle': '->', 'color': '#1d6345'}, color='#1d6345')
    ax.text(2.8, 2.0, 'Dashed orange: slicer-only volume', color='#a9600e')
    ax.set(xlim=(-7.3, 7.3), ylim=(-.4, 2.6), xlabel='Distance across crease (mm)', ylabel='Thickness (mm)')
    ax.set_aspect('equal')
    ax.spines[['top', 'right']].set_visible(False)
    fig.suptitle('Material partition at one captive hinge', fontsize=16, fontweight='bold')
    fig.text(.5, .02, 'Independent flat reference cross-section • not Onshape regeneration or a strength prediction', ha='center', fontsize=9)
    fig.tight_layout(rect=(0, .06, 1, .92))
    fig.savefig(Path(__file__).with_name('manufacturing_preview.png'), dpi=160)


if __name__ == '__main__':
    main()
