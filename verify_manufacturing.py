"""Independent manufacturing geometry checks. Does not compile FeatureScript."""
import copy
import json
import math
import unittest
from dataclasses import dataclass, replace
from pathlib import Path
import numpy as np
import manifold3d as m
import verify_geometry as v

@dataclass
class Manufacturing:
    shaft: float = 1.6
    head: float = 2.6
    head_h: float = .4
    embed: float = 2.2
    beam: float = 1.2
    pitch: float = 5
    count: int = 2
    ligament: float = .6
    key: bool = False
    segmented: bool = False
    segment_length: float = 4
    selection: str = 'all'
    mountain_t: float = .4
    valley_t: float = .4
    modifier_width: float = 8
    modifier_depth: float = 1.2
    modifier_height: float = .4
    modifier_offset: float = 0

def rectangle(origin,x,y,x0,x1,y0,y1):
    return np.array([origin+x0*x+y0*y,origin+x1*x+y0*y,origin+x1*x+y1*y,origin+x0*x+y1*y])
def cylinder(center,normal,radius,depth):
    normal=v.unit(normal)
    x=v.unit(np.cross(normal,np.array([1.,0,0]) if abs(normal[0])<.9 else np.array([0.,1,0])))
    y=np.cross(normal,x)
    p=np.array([center+radius*(math.cos(a)*x+math.sin(a)*y) for a in np.linspace(0,2*math.pi,48,endpoint=False)])
    return v.extrude(p,normal,depth)
# Coincident polygonal surfaces produce near-zero debris in Manifold. Ignore
# only components under 1e-7 mm^3. Simplification moves surfaces by at most
# MESH_TOLERANCE; bound re-intersection residue by surface area times four
# such movements rather than pretending a tessellated Boolean is exact CAD.
MESH_TOLERANCE = 1e-5

def partition_tolerance(solid):return max(1e-7,solid.surface_area()*MESH_TOLERANCE*4)

def components(solid):return [p for p in solid.decompose() if p.volume()>1e-7]
def union(parts):return m.Manifold.batch_boolean(parts,m.OpType.Add).set_tolerance(MESH_TOLERANCE)
def vol(x):return max(0.,x.volume())
def clip_circle(interval,f,g,origin,ev,offset,radius):
    for i,p in enumerate(f['p']):
        u=v.inward(f,i);need=g[i]+radius-(origin+offset-p)@u;slope=ev@u
        if abs(slope)<1e-9:assert need<1e-6,'anchor head edge ligament'
        elif slope>0:interval[0]=max(interval[0],need/slope)
        else:interval[1]=min(interval[1],need/slope)
    return interval

def link_frame(c,f,g,l):
    a,b=l['a'],l['b'];fa,fb=f[a],f[b];ia,ib=l['ia'],l['ib']
    p0=fa['p'][ia];p1=fa['p'][(ia+1)%len(fa['p'])];ev=v.unit(p1-p0)
    ua,ub=v.inward(fa,ia),v.inward(fb,ib);ga,gb=g[a][ia],g[b][ib]
    interval=[c.relief+c.corner,np.linalg.norm(p1-p0)-c.relief-c.corner]
    v.clip(interval,fa,ia,g[a],p0,ev,ua,ga,c);v.clip(interval,fb,ib,g[b],p0,ev,ub,gb,c)
    assert interval[1]-interval[0]>=max(1,3*c.hinge),'short hinge'
    return fa,fb,p0,ev,ua,ub,ga,gb,interval

def web_profile(c,fa,fb,pw,ua,ub,ga,gb,outer,net,t):
    na,nb,ta,tb=fa['n'],fb['n'],fa['t'],fb['t']
    if outer:
        pa=pw+(ga+c.overlap)*ua;pb=pw+(gb+c.overlap)*ub;middle=pw+v.offset(na,nb,t,t)
        return np.array([pa,pw,pb,pb+t*nb,middle,pa+t*na])
    if net:
        pa=pw+(ga+c.overlap)*ua+ta*na;pb=pw+(gb+c.overlap)*ub+tb*nb;middle=pw+(ta+tb)/2*na
    else:
        pi=pw+v.offset(na,nb,ta,tb);pa=pi+(ga+c.overlap-(pi-pw)@ua)*ua;pb=pi+(gb+c.overlap-(pi-pw)@ub)*ub;middle=pw+v.offset(na,nb,ta-t,tb-t)
        return np.array([pa,pi,pb,pb-t*nb,middle,pa-t*na])
    return np.array([pa,middle,pb,pb-t*nb,middle-t*na,pa-t*na])

def anchor(f,base,ev,u,gap,outer,t,c,a):
    center=base+(gap+a.embed)*u;n=f['n'];z=0 if outer else f['t']-t
    beam=v.extrude(rectangle(base+z*n,ev,u,-a.beam/2,a.beam/2,gap+c.overlap/2,gap+a.embed),n,t)
    if a.key:
        shaft=v.extrude(rectangle(center,ev,u,-a.shaft/2,a.shaft/2,-a.shaft/2,a.shaft/2),n,f['t'])
        bottom=v.extrude(rectangle(center,ev,u,-a.head/2,a.head/2,-a.head/2,a.head/2),n,a.head_h)
        top=v.extrude(rectangle(center+(f['t']-a.head_h)*n,ev,u,-a.head/2,a.head/2,-a.head/2,a.head/2),n,a.head_h)
    else:
        shaft=cylinder(center,n,a.shaft/2,f['t']);bottom=cylinder(center,n,a.head/2,a.head_h);top=cylinder(center+(f['t']-a.head_h)*n,n,a.head/2,a.head_h)
    # Both heads extend laterally past the neck: a true captive shape, not adhesion-only overlap.
    assert vol(bottom-shaft)>0 and vol(top-shaft)>0
    assert len(union([shaft,bottom,top,beam]).decompose())==1
    return union([shaft,bottom,top,beam])

def selected(l,f,a,c):
    mountain=l['outer']!=c.invert
    if a.selection=='all':return True
    if a.selection=='mountain':return mountain
    if a.selection=='valley':return not mountain
    fa,fb=f[l['a']],f[l['b']]
    if a.selection=='section':return fa['cell']!=fb['cell'] and not(fa['cap'] or fb['cap'])
    return False

def manufacture(c=v.Config(),a=Manufacturing(),net=True,modifiers=False):
    envelope=a.head/2*(math.sqrt(2) if a.key else 1)
    assert a.head>=a.shaft+.6,'positive anchor capture'
    assert a.embed>=envelope+a.ligament,'anchor engagement'
    assert c.t>=2*a.head_h+.4-1e-9,'rigid retention layer'
    assert a.pitch>=2*envelope+c.clearance,'head spacing'
    assert not a.segmented or a.segment_length+c.clearance<a.pitch,'segment spacing'
    f,l,g=v.build(c)
    if net:f,_=v.unfold(f,l)
    panels=[v.extrude(v.inset(face,gap),face['n'],face['t']).set_tolerance(MESH_TOLERANCE) for face,gap in zip(f,g)]
    tpu=[];rigid_webs=[];mods=[];anchors=0
    for k,link in enumerate(l):
        fa,fb,p0,ev,ua,ub,ga,gb,interval=link_frame(c,f,g,link)
        flexible=selected(link,f,a,c);mountain=link['outer']!=c.invert;t=(a.mountain_t if mountain else a.valley_t) if flexible else c.hinge
        assert .3<=t<=.8*min(fa['t'],fb['t']),'TPU thickness'
        stations=[]
        if flexible:
            ai=interval.copy()
            clip_circle(ai,fa,g[link['a']],p0,ev,(ga+a.embed)*ua,envelope+a.ligament+c.corner)
            clip_circle(ai,fb,g[link['b']],p0,ev,(gb+a.embed)*ub,envelope+a.ligament+c.corner)
            available=ai[1]-ai[0]
            assert available>0 and (a.count==1 or available/a.count>=a.pitch),'anchor spacing does not fit'
            stations=[ai[0]+available*(i+.5)/a.count for i in range(a.count)]
        segments=[[s-a.segment_length/2,s+a.segment_length/2] for s in stations] if flexible and a.segmented else [interval]
        for part,seg in enumerate(segments):
            assert seg[0]>=interval[0] and seg[1]<=interval[1],'segment outside hinge interval'
            poly=web_profile(c,fa,fb,p0+seg[0]*ev,ua,ub,ga,gb,link['outer'],net,t)
            pieces=[v.extrude(poly,ev,seg[1]-seg[0])]
            if flexible:
                for i,station in enumerate(stations):
                    if a.segmented and i!=part:continue
                    for face,u,gap in [(fa,ua,ga),(fb,ub,gb)]:pieces.append(anchor(face,p0+station*ev,ev,u,gap,link['outer'],t,c,a));anchors+=1
            web=union(pieces)
            assert len(web.decompose())==1,'floating TPU anchor'
            for j,panel in enumerate(panels):
                hit=vol(web^panel)
                if j in [link['a'],link['b']]:assert hit>1e-7,'missing panel engagement'
                else:assert hit<1e-7,f'unrelated panel contact at crease {k+1}'
            for other in tpu+rigid_webs:assert vol(web^other)<1e-7,f'hinge/anchor contact at crease {k+1}'
            (tpu if flexible else rigid_webs).append(web)
        if modifiers:
            na,nb=fa['n'],fb['n'];normal=v.unit(na+nb);across=v.unit(np.cross(normal,ev))
            if across@ua<0:across=-across
            shift=(fa['t']+fb['t'])/2*normal
            if abs(na@nb)<.999999:shift=v.offset(na,nb,fa['t'],fb['t'])
            margin=max(c.relief,a.modifier_width/2+abs(a.modifier_offset))
            length=interval[1]-interval[0]-2*margin
            assert length>=1,'modifier endpoint clearance'
            origin=p0+(interval[0]+margin)*ev+shift+a.modifier_offset*across
            for face,u,gap in [(fa,ua,ga),(fb,ub,gb)]:
                penetration=min(face['t'],a.modifier_depth)
                for wf in [.5,1]:
                    for df in [.25,.5]:
                        point=p0+(interval[0]+margin)*ev+(gap+.4*wf)*u+(face['t']-penetration*df)*face['n']
                        relative=point-origin
                        assert abs(relative@across)<a.modifier_width/2-1e-6 and -a.modifier_depth+1e-6<relative@normal<a.modifier_height-1e-6,'insufficient modifier footprint'
            poly=rectangle(origin,across,normal,-a.modifier_width/2,a.modifier_width/2,-a.modifier_depth,a.modifier_height)
            mod=v.extrude(poly,ev,length)
            assert vol(mod^panels[link['a']])>1e-7 and vol(mod^panels[link['b']])>1e-7,'modifier misses intended panel'
            for j,panel in enumerate(panels):
                if j not in [link['a'],link['b']]:assert vol(mod^panel)<1e-7,'modifier reaches unrelated panel'
            mods.append(mod)
    assert tpu,'no selected TPU creases'
    flexible=union(tpu);rigid=[]
    for panel in panels:
        remainder=(panel-flexible).set_tolerance(MESH_TOLERANCE)
        assert len(components(remainder))==1,'anchor cavities split PLA panel'
        assert vol(remainder^flexible)<partition_tolerance(panel),'overlapping printable material'
        rigid.append(remainder)
    rigid=union(rigid+rigid_webs)
    assert len(components(union([rigid,flexible])))==1,'disconnected assembled geometry'
    assert vol(rigid^flexible)<partition_tolerance(rigid)
    assert rigid.status()==m.Error.NoError and flexible.status()==m.Error.NoError
    return dict(pla_parts=len(components(rigid)),tpu_parts=len(tpu),anchors=anchors,modifiers=len(mods),material_overlap_mm3=vol(rigid^flexible),partition_volume_tolerance_mm3=partition_tolerance(rigid))

class ManufacturingTests(unittest.TestCase):
    def test_reference_net_and_folded(self):
        for net in [True,False]:
            with self.subTest(net=net):
                r=manufacture(net=net);self.assertEqual(r['pla_parts'],12);self.assertEqual(r['tpu_parts'],11);self.assertEqual(r['anchors'],44)
    def test_repeated_alternating(self):
        for net in [True,False]:
            with self.subTest(net=net):manufacture(v.Config(cells=2,alternate=True),net=net)
    def test_taper_and_custom(self):
        for c in [v.Config(cells=2,top_radius=25),v.Config(cells=2,schedule=[(30,28,25,1,0),(30,26,25,2,0)])]:
            for net in [True,False]:
                with self.subTest(config=c,net=net):manufacture(c,net=net)
    def test_segmented_materials(self):self.assertEqual(manufacture(a=Manufacturing(segmented=True))['tpu_parts'],22)
    def test_selected_mountain_keeps_rigid_webs(self):
        r=manufacture(a=Manufacturing(selection='mountain'));self.assertLess(r['pla_parts'],12);self.assertEqual(r['tpu_parts'],6)
    def test_rectangular_capture(self):manufacture(a=Manufacturing(key=True,embed=2.6))
    def test_capped_cells(self):manufacture(v.Config(caps='both'),a=Manufacturing(count=1))
    def test_clockwise_inverted_hinges(self):manufacture(v.Config(sign=-1,invert=True),a=Manufacturing(count=1))
    def test_fold_specific_thickness(self):manufacture(a=Manufacturing(mountain_t=.5,valley_t=.3))
    def test_modifiers_are_separate_overlapping_volumes(self):
        for net in [True,False]:
            with self.subTest(net=net):self.assertEqual(manufacture(net=net,modifiers=True)['modifiers'],11)
    def test_invalid_manufacturing_dimensions(self):
        for a in [Manufacturing(head=1.8),Manufacturing(embed=.5),Manufacturing(count=8),Manufacturing(segmented=True,segment_length=8),Manufacturing(modifier_offset=20)]:
            with self.subTest(settings=a),self.assertRaises(AssertionError):manufacture(a=a,modifiers=True)


# Module and seam checks use the same reference face graph, but build their own
# material solids and verify actual neck attachment, through-holes, and spacing.
def module_of(face,c):return face['cell'] if not face['cap'] else (0 if face['ids'][0]<c.n else c.cells-1)
def adjacent_edges(faces):
    table={};out=[]
    for b,f in enumerate(faces):
        for ib,v0 in enumerate(f['ids']):
            v1=f['ids'][(ib+1)%len(f['ids'])];key=tuple(sorted([v0,v1]))
            if key in table:
                a,ia=table[key];out.append(dict(a=a,b=b,ia=ia,ib=ib))
            else:table[key]=(b,ib)
    return out

def eyelets(f,e,g,first,c,owner,width=5,depth=5,hole=2,count=1,projected=False,collar=.4):
    p0=f['p'][e];p1=f['p'][(e+1)%len(f['p'])];length=np.linalg.norm(p1-p0);ev=v.unit(p1-p0);u=v.inward(f,e);r=width/2;gap=g[e]
    assert width>=hole+1.6 and depth>=(width if projected else r+.8),'eyelet wall'
    interval=[r+c.clearance,length-r-c.clearance]
    if projected:v.clip(interval,f,e,g,p0,ev,u,gap,replace(c,corner=r))
    else:clip_circle(interval,f,g,p0,ev,(gap+depth)*u,r+.8)
    available=interval[1]-interval[0]
    assert available/(2*count)>=width+c.clearance,'staggered eyelets do not fit'
    ascending=f['ids'][e]<f['ids'][(e+1)%len(f['ids'])]
    parts=[owner];holes=[]
    for i in range(count):
        frac=(2*i+(.5 if first else 1.5))/(2*count)
        station=interval[0]+available*frac if ascending else interval[1]-available*frac
        base=p0+station*ev;center=base+((-depth+r) if projected else gap+depth)*u
        patch=[]
        if projected:patch.append(v.extrude(rectangle(base,ev,u,-r,r,-depth+r,gap+c.overlap),f['n'],f['t']))
        thickness=f['t']+(0 if projected else collar)
        patch.append(cylinder(center,f['n'],r,thickness));patch=union(patch)
        assert vol(patch^owner)>1e-7,'eyelet patch does not engage owner'
        parts.append(patch);holes.append(cylinder(center-.1*f['n'],f['n'],hole/2,thickness+.2))
    result=union(parts)-union(holes)
    assert len(components(result))==1
    return result

def modules(c=v.Config(cells=2),laced=False,seam=False,net=True,spaced=True,spacing=14,projected=False):
    f,l,g=v.build(c);all_edges=adjacent_edges(f)
    if net:f,_=v.unfold(f,l)
    if net and spaced:
        cursor=0
        for cell in range(c.cells):
            vertices=np.vstack([face['p'] for face in f if module_of(face,c)==cell]);xmin,ymin=vertices[:,:2].min(axis=0);xmax=vertices[:,0].max();shift=np.array([cursor-xmin,-ymin,0])
            for face in f:
                if module_of(face,c)==cell:face['p']+=shift
            cursor+=xmax-xmin+spacing
    assert not(laced and projected and net and not spaced),'section eyelets need print spacing'
    assert not(projected and not net),'protruding tabs are net-only'
    panels=[v.extrude(v.inset(face,gap),face['n'],face['t']).set_tolerance(MESH_TOLERANCE) for face,gap in zip(f,g)];n_eyelets=0
    for cr in all_edges:
        a,b=cr['a'],cr['b'];fa,fb=f[a],f[b]
        if fa['cap'] or fb['cap']:continue
        section=fa['cell']!=fb['cell'];longitudinal=not section and abs(fa['order']-fb['order'])==2*c.n-1
        if (section and laced) or (longitudinal and seam):
            for idx,e,first in [(a,cr['ia'],True),(b,cr['ib'],False)]:
                panels[idx]=eyelets(f[idx],e,g[idx],first,c,panels[idx],projected=projected);n_eyelets+=1
    for i,panel in enumerate(panels):
        assert len(components(panel))==1
        for other in panels[:i]:assert vol(panel^other)<1e-7,'eyelet enters another panel'
    results=[]
    for cell in range(c.cells):
        parts=[p for p,face in zip(panels,f) if module_of(face,c)==cell]
        for cr in l:
            if module_of(f[cr['a']],c)!=cell or module_of(f[cr['b']],c)!=cell:continue
            fa,fb,p0,ev,ua,ub,ga,gb,interval=link_frame(c,f,g,cr)
            parts.append(v.extrude(web_profile(c,fa,fb,p0+interval[0]*ev,ua,ub,ga,gb,cr['outer'],net,c.hinge),ev,interval[1]-interval[0]))
        result=union(parts);assert len(components(result))==1,'disconnected module';results.append(result)
    for i,result in enumerate(results):
        for other in results[:i]:assert vol(result^other)<1e-7,'modules interfere'
    return dict(modules=len(results),eyelets=n_eyelets)

class AssemblyTests(unittest.TestCase):
    def test_independent_modules_keep_assembly_coordinates(self):self.assertEqual(modules(net=False,spaced=False)['modules'],2)
    def test_spaced_tapered_modules(self):self.assertEqual(modules(v.Config(cells=2,top_radius=25))['modules'],2)
    def test_modular_section_lacing(self):self.assertEqual(modules(laced=True)['eyelets'],12)
    def test_net_longitudinal_lacing(self):self.assertEqual(modules(v.Config(cells=1),seam=True)['eyelets'],2)
    def test_folded_longitudinal_lacing(self):self.assertEqual(modules(v.Config(cells=1),seam=True,net=False,spaced=False)['eyelets'],2)
    def test_projected_net_tabs(self):self.assertEqual(modules(v.Config(cells=1),seam=True,projected=True)['eyelets'],2)
    def test_folded_section_lacing(self):self.assertEqual(modules(laced=True,net=False,spaced=False)['eyelets'],12)
    def test_reject_unspaced_section_eyelets(self):
        with self.assertRaisesRegex(AssertionError,'print spacing'):modules(laced=True,spaced=False,projected=True)

if __name__=="__main__":unittest.main(verbosity=2)
