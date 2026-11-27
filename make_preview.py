from pathlib import Path
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Polygon
from matplotlib.colors import LightSource
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
import verify_geometry as v
p=Path(__file__).parent
fig=plt.figure(figsize=(14,6.5),facecolor='#f6f8fa')
gs=fig.add_gridspec(1,2,width_ratios=[1,1.7],left=.02,right=.98,bottom=.17,top=.82,wspace=.10)
ax=fig.add_subplot(gs[0],projection='3d');mesh=np.load(p/'basic_folded_mesh.npz');verts=mesh['vertices'];tri=verts[mesh['faces']]
col=Poly3DCollection(tri,facecolors='#81b4c2',edgecolors='#81b4c2',linewidths=0,alpha=1,shade=True,lightsource=LightSource(azdeg=315,altdeg=40))
col.set_zsort('average');ax.add_collection3d(col)
f,l,g=v.build(v.Config());flat,err=v.unfold(f,l)
shift=np.min(np.vstack([face['p'] for face in flat]),axis=0)
for face in flat:face['p']-=shift
# Mesh shading retains depth occlusion; reference lines are shown only in the net.
ax.set(xlim=(-33,33),ylim=(-33,33),zlim=(0,36));ax.set_box_aspect((1,1,.6));ax.view_init(elev=26,azim=-57);ax.set_axis_off();ax.set_title('Folded design state',fontsize=14,color='#152e3b',pad=6)
ax2=fig.add_subplot(gs[1]);ax2.set_aspect('equal');ax2.set_facecolor('#f6f8fa')
for face,gap in zip(flat,g):
 ax2.add_patch(Polygon(face['p'][:,:2],fill=False,lw=.6,ls=':',edgecolor='#8d9da7'))
 shape=v.inset(face,gap);ax2.add_patch(Polygon(shape[:,:2],facecolor='#81b4c2',edgecolor='#234753',lw=.6))
for link in l:
 face=flat[link['a']];i=link['ia'];a=face['p'][i];b=face['p'][(i+1)%len(face['p'])];color='#d24b39' if link['outer'] else '#2674b8';ax2.plot([a[0],b[0]],[a[1],b[1]],color=color,lw=1.1)
ax2.autoscale_view();ax2.margins(.04);ax2.set_xlabel('mm',color='#506573');ax2.set_ylabel('mm',color='#506573');ax2.spines[['top','right']].set_visible(False);ax2.tick_params(colors='#506573');ax2.set_title('Exact cut-panel net',fontsize=14,color='#152e3b',pad=20)
fig.suptitle('One geometry · two representations',fontsize=22,fontweight='bold',color='#152e3b',x=.5,y=.97)
fig.text(.5,.885,'6 sides   /   Ø60 mm   /   height 30 mm   /   twist 75°',ha='center',fontsize=12,color='#506573')
fig.text(.53,.12,'Red: mountain    Blue: valley    Dotted: reference panel edges',ha='center',fontsize=10,color='#506573')
fig.text(.5,.055,'Calculated preview • no Onshape regeneration performed • compression redesigns the panels',ha='center',fontsize=10,color='#506573')
fig.savefig(p/'geometry_preview.png',dpi=160)
