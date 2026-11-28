"""Independent geometry illustration; does not run Onshape FeatureScript."""
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon as Patch, Circle
import numpy as np
import verify_refinements as r
import verify_geometry as v
import verify_manufacturing as m


def draw(ax, geometry, fill, edge, alpha=1):
    for polygon in geometry.geoms if hasattr(geometry, 'geoms') else [geometry]:
        if polygon.geom_type != 'Polygon':
            continue
        ax.add_patch(Patch(np.asarray(polygon.exterior.coords), facecolor=fill,
                           edgecolor=edge, linewidth=.7, alpha=alpha))
        for hole in polygon.interiors:
            ax.add_patch(Patch(np.asarray(hole.coords), facecolor='#f6f8fa', edgecolor=edge, linewidth=.7))


def main():
    fig, axes=plt.subplots(2, 1, figsize=(13, 8), facecolor='#f6f8fa', gridspec_kw={'height_ratios':[1.2,1]})
    for ax in axes:
        ax.set_facecolor('#f6f8fa');ax.set_aspect('equal');ax.spines[['top','right']].set_visible(False)
    c=v.Config(radius=40,top_radius=40)
    f,links,g=r.frames(c);panels=[v.extrude(v.inset(ff,gg),ff['n'],ff['t']) for ff,gg in zip(f,g)]
    for ff,gg in zip(f,g):axes[0].add_patch(Patch(v.inset(ff,gg)[:,:2],facecolor='#7ca6db',edgecolor='#244b70',linewidth=.7))
    for cr in links:
        ribbon,_=r.ribbon(c,f,g,cr,panels)
        for outline in ribbon.project().to_polygons():
            axes[0].add_patch(Patch(outline,facecolor='#e59025',edgecolor='#a9600e',alpha=.75,linewidth=.4))
    seam=next(cr for cr in m.adjacent_edges(f) if abs(f[cr['a']]['order']-f[cr['b']]['order'])==11)
    for idx,e,peer,pe in [(seam['a'],seam['ia'],seam['b'],seam['ib']),(seam['b'],seam['ib'],seam['a'],seam['ia'])]:
        for p in r.connection_row(f[idx],e,g[idx],c,count=2,peer=(f[peer],pe,g[peer])):
            axes[0].add_patch(Circle(p[:2],2.5,facecolor='#7ca6db',edgecolor='#244b70'))
            axes[0].add_patch(Circle(p[:2],1,facecolor='#f6f8fa',edgecolor='#244b70'))
    axes[0].autoscale_view();axes[0].set_title('80 mm tube net: full crease ribbons and horizontal normal-axis hole rows',loc='left',fontsize=11)
    axes[0].set(xlabel='Net X (mm)',ylabel='Net Y (mm)')
    male,female,_,_=r.joint_profiles(c,f,g,seam)
    draw(axes[1],male,'#64b68e','#1d6345');draw(axes[1],female,'#7ca6db','#244b70')
    axes[1].autoscale_view();axes[1].margins(.12)
    axes[1].set_title('Dovetail in shared folded datum: green male, blue receiver, 0.2 mm per-side clearance',loc='left',fontsize=11)
    axes[1].set(xlabel='Across seam (mm)',ylabel='Inward (mm)')
    fig.suptitle('Kresling manufacturing refinements',fontsize=16,fontweight='bold')
    fig.text(.5,.015,'Independent mesh/profile illustration • not Onshape regeneration, an assembly simulation or a strength prediction',ha='center',fontsize=9)
    fig.tight_layout(rect=(0,.045,1,.95))
    fig.savefig(Path(__file__).with_name('refinement_preview.png'),dpi=160)


if __name__=='__main__':
    main()
