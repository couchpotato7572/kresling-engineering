"""Independent mesh checks for full ribbons, transported rows and mating keys.
These tests do not execute FeatureScript or establish print/clinical suitability.
"""
import copy
import math
import unittest
from dataclasses import dataclass, replace
import numpy as np
from shapely.geometry import Polygon, MultiPoint, box
import verify_geometry as v
import verify_manufacturing as m


def frames(c,net=True,angle=0):
    original,links,gaps=v.build(c)
    flat,_=v.unfold(original,links)
    co,si=math.cos(angle),math.sin(angle)
    rotation=np.array([[co,-si,0],[si,co,0],[0,0,1.]])
    row_frames=copy.deepcopy(flat)
    for f in flat:f['p']=f['p']@rotation.T
    out=copy.deepcopy(flat if net else original)
    for f,ref,nf in zip(out,original,row_frames):f['ref_p']=ref['p'];f['ref_n']=ref['n'];f['net_p']=nf['p']
    return out,links,gaps

def map_reference(f,p):
    origin=f['ref_p'][0];x=v.unit(f['ref_p'][1]-origin);y=np.cross(f['ref_n'],x)
    nx=v.unit(f['p'][1]-f['p'][0]);ny=np.cross(f['n'],nx);r=p-origin
    return f['p'][0]+r@x*nx+r@y*ny+r@f['ref_n']*f['n']

def row_direction(f):
    x=v.unit(f['net_p'][1]-f['net_p'][0]);y=np.cross([0,0,1.],x)
    nx=v.unit(f['p'][1]-f['p'][0]);ny=np.cross(f['n'],nx)
    return v.unit(x[0]*nx+y[0]*ny)

def ribbon(c,f,g,l,panels,other_webs=(),ratio=1.5,depth=.4,allowance=.1,start_margin=0,end_margin=0,offset=0,absolute_width=None):
    fa,fb,p0,ev,ua,ub,ga,gb,interval=m.link_frame(c,f,g,l)
    length=np.linalg.norm(fa['p'][(l['ia']+1)%len(fa['p'])]-p0)-start_margin-end_margin
    assert length>.5
    p=p0+start_margin*ev;width=(ga+gb)*ratio if absolute_width is None else absolute_width
    extra_a=(width-ga-gb)/2+offset;extra_b=(width-ga-gb)/2-offset;assert min(extra_a,extra_b)>=.02
    lo=[-allowance,-allowance] if l['outer'] else [max(0,fa['t']-depth),max(0,fb['t']-depth)]
    hi=[min(fa['t'],depth),min(fb['t'],depth)] if l['outer'] else [fa['t']+allowance,fb['t']+allowance]
    def shift(a,b):return (a+b)/2*fa['n'] if fa['n']@fb['n']>.999999 else v.offset(fa['n'],fb['n'],a,b)
    lowmid=p+shift(*lo);highmid=p+shift(*hi)
    wa,wb=ga+min(extra_a,.03),gb+min(extra_b,.03)
    section=np.array([p+wa*ua+lo[0]*fa['n'],lowmid,p+wb*ub+lo[1]*fb['n'],p+wb*ub+hi[1]*fb['n'],highmid,p+wa*ua+hi[0]*fa['n']])
    parts=[v.extrude(section,ev,length)]
    for face,u,gap,gs,low,high,extra in [(fa,ua,ga,g[l['a']],lo[0],hi[0],extra_a),(fb,ub,gb,g[l['b']],lo[1],hi[1],extra_b)]:
        footprint=v.inset(face,gs);coords=np.column_stack([(footprint-p)@ev,(footprint-p)@u])
        clipped=Polygon(coords).intersection(box(0,-10000,length,gap+extra))
        if clipped.area>1e-8:
            pts=np.array([p+x*ev+y*u+low*face['n'] for x,y in np.asarray(clipped.exterior.coords)[:-1]])
            parts.append(v.extrude(pts,face['n'],high-low))
    result=m.union(parts)
    unrelated=[panel for i,panel in enumerate(panels) if i not in [l['a'],l['b']]]+list(other_webs)
    if unrelated:result-=m.union(unrelated)
    assert len(m.components(result))==1,'ribbon interrupted'
    vertices=result.to_mesh().vert_properties[:,:3].astype(float)
    axial=(vertices-p)@ev
    assert abs(axial.min())<1e-4 and abs(axial.max()-length)<1e-4,'ribbon lost endpoint'
    assert m.vol(result^panels[l['a']])>1e-7 and m.vol(result^panels[l['b']])>1e-7
    for other in unrelated:assert m.vol(result^other)<m.partition_tolerance(other),'ribbon reaches unrelated material'
    return result,dict(width=width,length=length,expected_length=length)

@dataclass
class Dovetail:
    width:float=4
    depth:float=2
    angle:float=15
    wall:float=1
    clearance:float=.2
    radius:float=.3
    length:float=6
    count:int=2
    spacing:float=2
    continuous:bool=False
    stop:bool=False


def rounded(poly,r):return poly.buffer(-r,join_style='mitre').buffer(r,quad_segs=16)

def joint_profiles(c,f,g,cr,d=Dovetail()):
    a,b=cr['a'],cr['b'];fa,fb=f[a],f[b];ia,ib=cr['ia'],cr['ib']
    # Joint geometry is defined solely in the folded reference frame.
    ra=dict(fa,p=fa['ref_p'],n=fa['ref_n']);rb=dict(fb,p=fb['ref_p'],n=fb['ref_n'])
    v0,v1=ra['ids'][ia],ra['ids'][(ia+1)%len(ra['ids'])]
    p0=ra['p'][ia] if v0<v1 else ra['p'][(ia+1)%len(ra['p'])]
    p1=ra['p'][(ia+1)%len(ra['p'])] if v0<v1 else ra['p'][ia]
    ev=v.unit(p1-p0);inside=v.unit(ra['n']+rb['n']);across=v.unit(np.cross(ev,inside))
    ua,ub=v.inward(ra,ia),v.inward(rb,ib)
    if across@ub<0:across=-across
    angle=math.radians(d.angle);neck=d.width-2*d.depth*math.tan(angle)
    loss=d.radius*(1/math.tan((math.pi/2-angle)/2)-1)
    assert neck>=1.6 and (d.width-neck)/2-loss-d.clearance>=.2,'insufficient dovetail capture'
    maxx=d.depth+2*d.wall+d.clearance
    h=d.width/2+d.wall
    ycenter=h+max((ff['t']+d.clearance+maxx*abs(across@ff['n']))/(inside@ff['n']) for ff in [ra,rb])
    def project(points):return np.column_stack([(points-p0)@across,(points-p0)@inside])
    def base(face,u,gap):
        return project(np.array([p0+(gap+w)*u+n*face['n'] for w,n in [(.1,face['t']-.8),(c.overlap,face['t']-.8),(c.overlap,face['t']),(.1,face['t'])]]))
    raw=Polygon([(0,-neck/2),(d.depth,-d.width/2),(d.depth,d.width/2),(0,neck/2)])
    male_shape=rounded(raw,d.radius)
    socket=rounded(raw.buffer(d.clearance,join_style='mitre'),d.radius+d.clearance)
    from shapely.affinity import translate
    male_shape=translate(male_shape,yoff=ycenter);socket=translate(socket,yoff=ycenter)
    neck_shape=box(-2*d.wall-d.clearance,ycenter-neck/2,.2,ycenter+neck/2)
    dock_a=np.array([[-2*d.wall-d.clearance,ycenter-neck/2],[-d.wall-d.clearance,ycenter-neck/2],[-d.wall-d.clearance,ycenter+neck/2],[-2*d.wall-d.clearance,ycenter+neck/2]])
    dock_b=np.array([[d.depth+.7*d.wall,ycenter-d.width/2],[d.depth+d.wall,ycenter-d.width/2],[d.depth+d.wall,ycenter+d.width/2],[d.depth+.7*d.wall,ycenter+d.width/2]])
    arm_a=MultiPoint(np.vstack([base(ra,ua,g[a][ia]),dock_a])).convex_hull
    arm_b=MultiPoint(np.vstack([base(rb,ub,g[b][ib]),dock_b])).convex_hull
    male=male_shape.union(neck_shape).union(arm_a)
    outer=rounded(box(-d.wall,ycenter-h,d.depth+d.wall,ycenter+h),min(d.radius,d.wall/3))
    mouth=box(-d.wall-.1,ycenter-neck/2-d.clearance,.25,ycenter+neck/2+d.clearance)
    female=outer.union(arm_b).difference(socket.union(mouth))
    assert male.intersection(female).area<1e-7,'male/receiver interference'
    assert male.is_valid and female.is_valid and male.geom_type=='Polygon' and female.geom_type=='Polygon'
    return male,female,(p0,ev,across,inside),neck


class RefinementTests(unittest.TestCase):
    def test_reference_maps_preserve_all_vertices(self):
        for net in [False,True]:
            f,_,_=frames(v.Config(cells=2,top_radius=25),net,angle=.4)
            for face in f:
                for ref,p in zip(face['ref_p'],face['p']):self.assertLess(np.linalg.norm(map_reference(face,ref)-p),1e-8)
    def test_dovetail_profiles(self):
        for c in [v.Config(),v.Config(sign=-1),v.Config(cells=2,alternate=True),v.Config(cells=2,top_radius=25)]:
            f,_,g=frames(c)
            for cr in m.adjacent_edges(f):
                a,b=f[cr['a']],f[cr['b']]
                if not(a['cap'] or b['cap']) and a['cell']==b['cell'] and abs(a['order']-b['order'])==2*c.n-1:joint_profiles(c,f,g,cr)


def map_solid(solid,f):
    ro=f['ref_p'][0];rx=v.unit(f['ref_p'][1]-ro);ry=np.cross(f['ref_n'],rx)
    co=f['p'][0];cx=v.unit(f['p'][1]-co);cy=np.cross(f['n'],cx)
    matrix=np.column_stack([cx,cy,f['n']])@np.column_stack([rx,ry,f['ref_n']]).T
    return solid.transform(np.column_stack([matrix,co-matrix@ro]))

def from_profile(poly,p0,ev,across,inside,start,length):
    assert not poly.interiors,'unexpected enclosed socket mouth'
    points=np.array([p0+start*ev+x*across+y*inside for x,y in np.asarray(poly.exterior.coords)[:-1]])
    return v.extrude(points,ev,length)

def closure_parts(c,f,g,cr,d=Dovetail(),assisted=False):
    male,female,frame,neck=joint_profiles(c,f,g,cr,d);p0,ev,across,inside=frame
    fa,fb=f[cr['a']],f[cr['b']]
    ra=dict(fa,p=fa['ref_p'],n=fa['ref_n']);rb=dict(fb,p=fb['ref_p'],n=fb['ref_n'])
    ua,ub=v.inward(ra,cr['ia']),v.inward(rb,cr['ib']);ga,gb=g[cr['a']][cr['ia']],g[cr['b']][cr['ib']]
    interval=[c.relief+d.clearance,np.linalg.norm(ra['p'][(cr['ia']+1)%len(ra['p'])]-ra['p'][cr['ia']])-c.relief-d.clearance-(d.wall if d.stop else 0)]
    v.clip(interval,ra,cr['ia'],g[cr['a']],p0,ev,ua,ga,replace(c,corner=d.radius))
    v.clip(interval,rb,cr['ib'],g[cr['b']],p0,ev,ub,gb,replace(c,corner=d.radius))
    if assisted:m.clip_circle(interval,ra,g[cr['a']],p0,ev,(ga+2.2)*ua,1.3+.6)
    available=interval[1]-interval[0];count=1 if d.continuous else d.count;length=available if d.continuous else d.length
    occupied=length+2*d.clearance+(d.wall if d.stop else 0)
    assert length>=3 and available>=length and (count==1 or available/count>=occupied+d.spacing),'joint spacing'
    male_parts=[];female_parts=[];middles=[]
    h=d.width/2+d.wall
    maxx=d.depth+2*d.wall+d.clearance
    yc=h+max((ff['t']+d.clearance+maxx*abs(across@ff['n']))/(inside@ff['n']) for ff in [ra,rb])
    outer=rounded(box(-d.wall,yc-h,d.depth+d.wall,yc+h),min(d.radius,d.wall/3))
    for i in range(count):
        middle=sum(interval)/2+(i-(count-1)/2)*available/count;start=middle-length/2
        male_parts.append(from_profile(male,p0,ev,across,inside,start,length))
        receiver=from_profile(female,p0,ev,across,inside,start-d.clearance,length+2*d.clearance)
        if d.stop:receiver+=from_profile(outer,p0,ev,across,inside,start+length+d.clearance-.01,d.wall+.01)
        female_parts.append(receiver);middles.append(p0+middle*ev)
    all_female=m.union(female_parts)
    for step in [0,.5,1]:
        for male_part in male_parts:assert m.vol(male_part.translate(-step*(length+2*d.clearance)*ev)^all_female)<1e-7,'axial insertion interference'
    out=[]
    for male_part,female_part,middle in zip(male_parts,female_parts,middles):
        mp,fp=map_solid(male_part,fa),map_solid(female_part,fb)
        if assisted:
            base=map_reference(fa,middle);current_ev=v.unit(map_reference(fa,middle+ev)-base);current_u=v.unit(map_reference(fa,middle+ua)-base)
            mp+=m.anchor(fa,base,current_ev,current_u,ga,False,c.hinge,c,m.Manufacturing(count=1))
        out.extend([(mp,cr['a'],assisted),(fp,cr['b'],False)])
    return out


def connection_row(f,e,g,c,count=2,pitch=6,depth=5,radius=2.5,peer=None):
    def layout(face,edge,gs):
        p0=face['p'][edge];ev=v.unit(face['p'][(edge+1)%len(face['p'])]-p0);u=v.inward(face,edge);row=row_direction(face)
        interval=[radius+c.clearance,np.linalg.norm(face['p'][(edge+1)%len(face['p'])]-p0)-radius-c.clearance]
        inset=max(depth,radius+.8+(count-1)/2*pitch*abs(row@u))
        for i in range(count):
            delta=(i-(count-1)/2)*pitch*row
            m.clip_circle(interval,face,gs,p0,ev,(gs[edge]+inset)*u+delta,radius+.8)
        assert interval[1]>=interval[0],'horizontal row does not fit'
        np0=face['net_p'][edge];nev=v.unit(face['net_p'][(edge+1)%len(face['p'])]-np0);nu=v.inward(dict(face,p=face['net_p']),edge)
        y0=np0[1]+(gs[edge]+inset)*nu[1];ys=[y0+interval[j]*nev[1] for j in [0,1]]
        return p0,ev,u,row,interval,inset,(min(ys),max(ys),y0,nev[1])
    p0,ev,u,row,interval,inset,band=layout(f,e,g)
    station=sum(interval)/2
    if peer is not None:
        pf,pe,pg=peer
        own_edge=v.unit(f['net_p'][(e+1)%len(f['p'])]-f['net_p'][e]);peer_edge=v.unit(pf['net_p'][(pe+1)%len(pf['p'])]-pf['net_p'][pe])
        other=layout(pf,pe,pg)[-1]
        if np.linalg.norm(np.cross(own_edge,peer_edge))>1e-6:
            return np.array([p0+station*ev+(g[e]+inset)*u+(i-(count-1)/2)*pitch*row for i in range(count)])
        low=max(band[0],other[0]);high=min(band[1],other[1]);assert high>=low-1e-6,'paired horizontal datum cannot fit'
        if abs(band[3])>1e-8:station=((low+high)/2-band[2])/band[3]
    return np.array([p0+station*ev+(g[e]+inset)*u+(i-(count-1)/2)*pitch*row for i in range(count)])


class GeometryRefinementTests(unittest.TestCase):
    def test_full_length_slim_modifiers(self):
        for c in [v.Config(),v.Config(cells=2,alternate=True),v.Config(cells=2,top_radius=25),v.Config(caps='both')]:
            for net in [True,False]:
                with self.subTest(cells=c.cells,top=c.top_radius,net=net):
                    f,l,g=frames(c,net);panels=[v.extrude(v.inset(ff,gg),ff['n'],ff['t']) for ff,gg in zip(f,g)]
                    webs=[]
                    for link in l:
                        fa,fb,p0,ev,ua,ub,ga,gb,interval=m.link_frame(c,f,g,link)
                        webs.append(v.extrude(m.web_profile(c,fa,fb,p0+interval[0]*ev,ua,ub,ga,gb,link['outer'],net,c.hinge),ev,interval[1]-interval[0]))
                    for i,link in enumerate(l):
                        _,data=ribbon(c,f,g,link,panels,[w for j,w in enumerate(webs) if i!=j])
                        self.assertAlmostEqual(data['width'],1.5*(g[link['a']][link['ia']]+g[link['b']][link['ib']]))
    def test_explicit_modifier_margins(self):
        c=v.Config();f,l,g=frames(c);panels=[v.extrude(v.inset(ff,gg),ff['n'],ff['t']) for ff,gg in zip(f,g)]
        _,data=ribbon(c,f,g,l[0],panels,start_margin=.3,end_margin=.7)
        edge=np.linalg.norm(f[l[0]['a']]['p'][(l[0]['ia']+1)%3]-f[l[0]['a']]['p'][l[0]['ia']])
        self.assertAlmostEqual(data['length'],edge-1)
    def test_hole_rows_keep_normal_axes(self):
        for c,count in [(v.Config(),1),(v.Config(radius=40,top_radius=40),2),
                        (v.Config(cells=2,radius=40,top_radius=35),2),(v.Config(cells=2,radius=40,top_radius=40,alternate=True),2)]:
            for net in [True,False]:
                f,_,g=frames(c,net)
                for cr in m.adjacent_edges(f):
                    a,b=f[cr['a']],f[cr['b']]
                    if a['cap'] or b['cap'] or a['cell']!=b['cell'] or abs(a['order']-b['order'])!=2*c.n-1:continue
                    for idx,e in [(cr['a'],cr['ia']),(cr['b'],cr['ib'])]:
                        face=f[idx]
                        other_idx,other_edge=(cr['b'],cr['ib']) if idx==cr['a'] else (cr['a'],cr['ia'])
                        centers=connection_row(face,e,g[idx],c,count=count,peer=(f[other_idx],other_edge,g[other_idx]))
                        cx=v.unit(face['p'][1]-face['p'][0]);cy=np.cross(face['n'],cx)
                        nx=v.unit(face['net_p'][1]-face['net_p'][0]);ny=np.cross([0,0,1.],nx)
                        flat=np.array([face['net_p'][0]+(p-face['p'][0])@cx*nx+(p-face['p'][0])@cy*ny for p in centers])
                        self.assertLess(np.ptp(flat[:,1]),1e-8)
                        if count>1:self.assertAlmostEqual(flat[1,0]-flat[0,0],6)
                        self.assertLess(max(abs((centers-face['p'][0])@face['n'])),1e-8)
                        owner=v.extrude(v.inset(face,g[idx]),face['n'],face['t'])
                        bosses=[m.cylinder(p,face['n'],2.5,face['t']+.4) for p in centers]
                        cutters=[m.cylinder(p-.1*face['n'],face['n'],1,face['t']+.6) for p in centers]
                        drilled=m.union([owner]+bosses)-m.union(cutters)
                        self.assertEqual(len(m.components(drilled)),1)
                        for tool in cutters:self.assertLess(m.vol(drilled^tool),m.partition_tolerance(drilled))
    def test_reject_rows_that_cannot_fit_both_seam_panels(self):
        c=v.Config();f,_,g=frames(c)
        cr=next(x for x in m.adjacent_edges(f) if abs(f[x['a']]['order']-f[x['b']]['order'])==11)
        with self.assertRaisesRegex(AssertionError,'paired horizontal datum'):
            connection_row(f[cr['a']],cr['ia'],g[cr['a']],c,count=2,peer=(f[cr['b']],cr['ib'],g[cr['b']]))
    def test_rounded_socket_is_a_clearance_offset_of_male(self):
        for angle in [10,15,25]:
            depth,width,radius,clearance=2,4,.3,.2
            neck=width-2*depth*math.tan(math.radians(angle))
            raw=Polygon([(0,-neck/2),(depth,-width/2),(depth,width/2),(0,neck/2)])
            male=rounded(raw,radius)
            socket=rounded(raw.buffer(clearance,join_style='mitre'),radius+clearance)
            reference=male.buffer(clearance,quad_segs=32)
            self.assertLess(socket.hausdorff_distance(reference),.002)
    def test_invalid_dovetail_dimensions_are_rejected(self):
        c=v.Config();f,_,g=frames(c)
        cr=next(x for x in m.adjacent_edges(f) if abs(f[x['a']]['order']-f[x['b']]['order'])==11)
        for d in [Dovetail(depth=5,angle=30),Dovetail(clearance=.8),Dovetail(count=6,length=20)]:
            with self.subTest(settings=d),self.assertRaises(AssertionError):closure_parts(c,f,g,cr,d)
    def test_absolute_width_is_still_supported(self):
        c=v.Config();f,l,g=frames(c);panels=[v.extrude(v.inset(ff,gg),ff['n'],ff['t']) for ff,gg in zip(f,g)]
        _,data=ribbon(c,f,g,l[0],panels,absolute_width=4)
        self.assertEqual(data['width'],4)
    def test_print_rotation_rotates_the_whole_pattern(self):
        c=v.Config();f,_,g=frames(c,True,angle=.4)
        face=f[0];row=row_direction(face);self.assertAlmostEqual(row[0],math.cos(.4));self.assertAlmostEqual(row[1],math.sin(.4))
    def test_dovetail_3d_owner_fit_and_clearance(self):
        for c in [v.Config(),v.Config(cells=2,alternate=True),v.Config(cells=2,top_radius=25)]:
            for net in [True,False]:
                with self.subTest(cells=c.cells,top=c.top_radius,net=net):
                    f,_,g=frames(c,net);panels=[v.extrude(v.inset(ff,gg),ff['n'],ff['t']) for ff,gg in zip(f,g)];parts=[]
                    for cr in m.adjacent_edges(f):
                        a,b=f[cr['a']],f[cr['b']]
                        if not(a['cap'] or b['cap']) and a['cell']==b['cell'] and abs(a['order']-b['order'])==2*c.n-1:parts+=closure_parts(c,f,g,cr)
                    _,links,_=v.build(c)
                    webs=[]
                    for link in links:
                        fa,fb,p0,ev,ua,ub,ga,gb,interval=m.link_frame(c,f,g,link)
                        webs.append(v.extrude(m.web_profile(c,fa,fb,p0+interval[0]*ev,ua,ub,ga,gb,link['outer'],net,c.hinge),ev,interval[1]-interval[0]))
                    for i,(part,owner,_) in enumerate(parts):
                        for web in webs:self.assertLess(m.vol(part^web),1e-7,'dovetail obstructs a folding crease')
                        self.assertEqual(len(m.components(part)),1);self.assertGreater(m.vol(part^panels[owner]),1e-7)
                        for j,panel in enumerate(panels):
                            if j!=owner:self.assertLess(m.vol(part^panel),1e-7,'joint hits unrelated panel')
                        for other,_,_ in parts[:i]:self.assertLess(m.vol(part^other),1e-7,'joint halves interfere')
    def test_mixed_material_modifiers_and_dovetail_together(self):
        for closure in ['rigid','tpu']:
            with self.subTest(closure=closure):
                result=m.manufacture(modifiers=True,closure=closure)
                self.assertEqual(result['modifiers'],11)
    def test_continuous_stop_and_tpu_variants(self):
        c=v.Config();f,_,g=frames(c);cr=next(x for x in m.adjacent_edges(f) if abs(f[x['a']]['order']-f[x['b']]['order'])==11)
        for d,assisted in [(Dovetail(continuous=True),False),(Dovetail(stop=True),False),(Dovetail(count=1),True)]:
            with self.subTest(continuous=d.continuous,stop=d.stop,assisted=assisted):closure_parts(c,f,g,cr,d,assisted)


if __name__=="__main__":unittest.main(verbosity=2)
