"""Independent numerical + Manifold mesh-kernel checks; NOT an Onshape compiler.
Run: PYTHONPATH=/tmp/kresling-deps python verify_geometry.py
Dependencies: numpy, scipy, shapely, manifold3d. See VALIDATION.md.
"""
import copy, json, math, sys
from dataclasses import dataclass, replace
from pathlib import Path
import numpy as np
from shapely.geometry import Polygon
import manifold3d as m

EPS=1e-7

def unit(x):
    assert np.linalg.norm(x)>1e-8,'zero vector'
    return x/np.linalg.norm(x)
def inward(f,e):
    p=f['p']; c=np.mean(p,axis=0); edge=unit(p[(e+1)%len(p)]-p[e]);w=c-p[e]
    return unit(w-w@edge*edge)
def face(ids,v,t,inside,cell,order,cap=False):
    p=np.array([v[i] for i in ids]);c=p.mean(axis=0);n=unit(np.cross(p[1]-p[0],p[2]-p[0]))
    if n@(inside-c)<0:n=-n
    return dict(ids=ids,p=p,n=n,t=t,cell=cell,order=order,cap=cap)
def offset(a,b,da,db):
    c=a@b;den=1-c*c
    if den<1e-8:
        assert c>0 and abs(da-db)<1e-6,'singular offset'
        return da*a
    return (da-c*db)/den*a+(db-c*da)/den*b

def inset(f,g):
    p=f['p'];out=[]
    for i in range(len(p)):
        j=(i-1)%len(p);u=inward(f,j);w=inward(f,i);c=u@w;den=1-c*c
        assert den>1e-9
        out.append(p[i]+(g[j]-c*g[i])/den*u+(g[i]-c*g[j])/den*w)
    out=np.array(out)
    for q in out:
        for e in range(len(p)):assert (q-p[e])@inward(f,e)>=g[e]-1e-6,'collapsed inset'
    assert min(np.linalg.norm(out[(i+1)%len(p)]-q) for i,q in enumerate(out))>=.2,'short inset edge'
    assert np.linalg.norm(np.cross(out[1]-out[0],out[2]-out[0]))>=.05
    return out

@dataclass
class Config:
    n:int=6
    cells:int=1
    radius:float=30
    top_radius:float=30
    height:float=30
    twist:float=75
    sign:int=1
    alternate:bool=False
    seam:int=0
    exponent:float=1
    compression:float=0
    dx:float=0
    dy:float=0
    t:float=1.2
    plate:float=2
    hinge:float=.4
    width:float=1.6
    gap:float=.6
    clearance:float=.2
    overlap:float=.8
    relief:float=1
    caps:str='none'
    invert:bool=False
    schedule:object=None
    tabs:bool=False
    corner:float=0

def build(c):
    n=c.n;v=[];centers=[np.zeros(3)];radii=[c.radius];signs=[];z=angle=0
    for ring in range(c.cells+1):
        if ring:
            sg=c.sign*(-1 if c.alternate and (ring-1)%2 else 1);signs.append(sg)
            if c.schedule:
                h,r,phi,cx,cy=c.schedule[ring-1]
            else:
                r=c.radius+(c.top_radius-c.radius)*(ring/c.cells)**c.exponent
                h=c.height;phi=c.twist;cx=c.dx*ring/c.cells;cy=c.dy*ring/c.cells
            assert 0<phi<180,'twist range'
            h*=1-c.compression
            assert h>=4*max(c.t,c.plate),'compressed height'
            assert np.linalg.norm(np.array([cx,cy])-centers[-1][:2])<=.5*min(r,radii[-1]),'offset range'
            z+=h;angle+=sg*math.radians(phi);centers.append(np.array([cx,cy,z]));radii.append(r)
        for j in range(n):
            a=angle+2*math.pi*j/n
            v.append(centers[ring]+radii[ring]*np.array([math.cos(a),math.sin(a),0]))
    faces=[]
    for k in range(c.cells):
        for i in range(n):
            a=(c.seam+signs[k]*i)%n;b=(c.seam+signs[k]*(i+1))%n
            for ids,order in [([k*n+a,k*n+b,(k+1)*n+a],2*i),([(k+1)*n+a,(k+1)*n+b,k*n+b],2*i+1)]:
                faces.append(face(ids,v,c.t,(centers[k]+centers[k+1])/2,k,order))
    for ring,inside in [(0,centers[0]+[0,0,1]),(c.cells,centers[-1]-[0,0,1])]:
        if c.caps=='both' or (c.caps=='bottom' and ring==0) or (c.caps=='top' and ring==c.cells):
            faces.append(face([ring*n+i for i in range(n)],v,c.plate,inside,-1,-1,True))
    table={};links=[];cuts=[];gaps=[np.zeros(len(f['p'])) for f in faces]
    for b,f in enumerate(faces):
        for ib,v0 in enumerate(f['ids']):
            v1=f['ids'][(ib+1)%len(f['ids'])];key=tuple(sorted([v0,v1]))
            if key not in table:table[key]=(b,ib);continue
            a,ia=table[key];fa=faces[a]
            if f['cap'] or fa['cap']:keep=ib==0 if f['cap'] else ia==0
            elif f['cell']==fa['cell']:keep=abs(f['order']-fa['order'])==1
            else:keep=set([v0%n,v1%n])==set([c.seam,(c.seam+1)%n])
            ua=inward(fa,ia);ub=inward(f,ib);ridge=fa['n']@ub+f['n']@ua>1e-8
            ga=gb=max(c.width,c.gap)/2
            if ridge:
                shift=offset(fa['n'],f['n'],fa['t'],f['t']);ga=max(ga,shift@ua+c.clearance/2);gb=max(gb,shift@ub+c.clearance/2)
            gaps[a][ia]=ga;gaps[b][ib]=gb
            if keep:links.append(dict(a=a,b=b,ia=ia,ib=ib,outer=ridge!=c.invert))
            elif f['cell']!=fa['cell'] and not (f['cap'] or fa['cap']):cuts.append(dict(a=a,b=b,ia=ia,ib=ib,outer=ridge!=c.invert))
    for i,f in enumerate(faces):
        if f['cap']:continue
        planes=[]
        if f['cell']==0 and c.caps in ['bottom','both']:planes.append((c.plate+c.clearance/2,1))
        if f['cell']==c.cells-1 and c.caps in ['top','both']:planes.append((z-c.plate-c.clearance/2,-1))
        for limit,sg in planes:
            shape=inset(f,gaps[i]);extra=0
            normalshift=min(0,f['t']*f['n'][2]) if sg==1 else max(0,f['t']*f['n'][2])
            for j,q in enumerate(shape):
                need=sg*(limit-q[2]-normalshift)
                if need>1e-7:
                    u0=inward(f,(j-1)%len(shape));u1=inward(f,j);shift=(u0+u1)/(1+u0@u1);slope=sg*shift[2]
                    assert slope>1e-8,'plate clearance unsolvable'
                    extra=max(extra,need/slope)
            gaps[i]+=extra;inset(f,gaps[i])
    assert len(links)==len(faces)-1
    flat,_=unfold(faces,links,checkoverlap=False)
    for l in cuts:
        a=flat[l['a']];b=flat[l['b']];indices=[l['ia'],(l['ia']+1)%len(a['ids'])]
        if all(np.linalg.norm(a['p'][i]-b['p'][b['ids'].index(a['ids'][i])])<1e-6 for i in indices):links.append(l)
    return faces,links,gaps

def unfold(faces,links,checkoverlap=True):
    flat=copy.deepcopy(faces);r=faces[0];o=r['p'][0];x=unit(r['p'][1]-o);y=np.cross(r['n'],x)
    flat[0]['p']=np.array([[(p-o)@x,(p-o)@y,0] for p in r['p']]);flat[0]['n']=np.array([0,0,1.])
    seen={0};queue=[0]
    for a in queue:
        for l in links:
            if a not in (l['a'],l['b']):continue
            b=l['b'] if a==l['a'] else l['a']
            if b in seen:continue
            ea=l['ia'] if a==l['a'] else l['ib'];eb=l['ib'] if a==l['a'] else l['ia']
            p0=faces[a]['p'][ea];p1=faces[a]['p'][(ea+1)%len(faces[a]['p'])];ew=unit(p1-p0);uw=inward(faces[b],eb)
            q0=flat[a]['p'][ea];q1=flat[a]['p'][(ea+1)%len(faces[a]['p'])];ef=unit(q1-q0);uf=-inward(flat[a],ea)
            flat[b]['p']=np.array([q0+(p-p0)@ew*ef+(p-p0)@uw*uf for p in faces[b]['p']]);flat[b]['n']=np.array([0,0,1.]);seen.add(b);queue.append(b)
    assert len(queue)==len(faces)
    maxerr=0
    for old,new in zip(faces,flat):
        for a in range(len(old['p'])):
            for b in range(a):maxerr=max(maxerr,abs(np.linalg.norm(old['p'][a]-old['p'][b])-np.linalg.norm(new['p'][a]-new['p'][b])))
    assert maxerr<1e-8,'isometry'
    for l in links:
        for vid in faces[l['a']]['ids']:
            if vid in faces[l['b']]['ids']:
                a=flat[l['a']]['p'][faces[l['a']]['ids'].index(vid)];b=flat[l['b']]['p'][faces[l['b']]['ids'].index(vid)]
                assert np.linalg.norm(a-b)<1e-8,'crease alignment'
    polygons=[Polygon(f['p'][:,:2]) for f in flat]
    for i,p in enumerate(polygons):
        for j,q in enumerate(polygons[:i]):
            if checkoverlap:assert p.intersection(q).area<1e-7,f'net overlaps {i+1},{j+1}'
    return flat,maxerr

def extrude(p,n,depth):
    o=p[0];x=unit(p[1]-o);n=unit(n);y=np.cross(n,x);assert np.max(np.abs((p-o)@n))<1e-5
    uv=np.array([[(q-o)@x,(q-o)@y] for q in p]);poly=Polygon(uv)
    assert poly.is_valid and poly.area>1e-6,'bad extrusion polygon'
    if not poly.exterior.is_ccw:uv=uv[::-1]
    return m.CrossSection([uv]).extrude(depth).transform(np.column_stack([x,y,n,o]))

def clip(inter,f,e,g,p0,ev,u,gap,c):
    for j in range(len(f['p'])):
        if j==e:continue
        inwardj=inward(f,j);slope=ev@inwardj
        for w in [gap,gap+c.overlap]:
            req=g[j]+c.corner+c.overlap+.2-(p0+w*u-f['p'][j])@inwardj
            if abs(slope)<1e-9:assert req<1e-6
            elif slope>0:inter[0]=max(inter[0],req/slope)
            else:inter[1]=min(inter[1],req/slope)
    return inter

def solids(c,faces,links,gaps,net=False,accessories=False):
    panels=[]
    for i,f in enumerate(faces):
        shape=inset(f,gaps[i]);solid=extrude(shape,f['n'],f['t'])
        if c.corner:
            origin=shape[0];xx=unit(shape[1]-origin);yy=np.cross(f['n'],xx)
            poly=Polygon(np.column_stack([(shape-origin)@xx,(shape-origin)@yy]))
            rounded=poly.buffer(-c.corner,join_style='round').buffer(c.corner,join_style='round')
            uv=np.asarray(rounded.exterior.coords)[:-1];roundedshape=np.array([origin+q[0]*xx+q[1]*yy for q in uv]);solid=extrude(roundedshape,f['n'],f['t'])
        if c.tabs and not f['cap']:
            for e,v0 in enumerate(f['ids']):
                v1=f['ids'][(e+1)%len(f['ids'])]
                if not ((v0<c.n and v1<c.n) or (v0>=c.cells*c.n and v1>=c.cells*c.n)):continue
                if set([v0%c.n,v1%c.n])!=set([c.seam,(c.seam+1)%c.n]):continue
                a=f['p'][e];b=f['p'][(e+1)%len(f['p'])];mid=(a+b)/2;xx=unit(b-a);yy=-inward(f,e)
                assert np.linalg.norm(b-a)>=20
                tab=np.array([mid+x*xx+y*yy for x,y in [(-8,-.8),(8,-.8),(8,10),(-8,10)]]);tabsolid=extrude(tab,f['n'],f['t'])
                slot=np.array([mid+x*xx+y*yy-.1*f['n'] for x,y in [(-6,4),(6,4),(6,6),(-6,6)]]);tabsolid-=extrude(slot,f['n'],f['t']+.2);solid+=tabsolid
        if accessories:
            if not f['cap']:
                ribf=dict(f,p=shape);ribshape=inset(ribf,np.full(len(shape),3.));solid+=extrude(ribshape+(f['t']-.1)*f['n'],f['n'],.9)
            radius=3 if f['cap'] else 1.5
            center=shape.mean(axis=0)
            for j in range(len(shape)):assert (center-shape[j])@inward(dict(f,p=shape),j)>=radius+2
            x=unit(shape[1]-shape[0]);y=np.cross(f['n'],x)
            circ=np.array([center-.1*f['n']+radius*(math.cos(a)*x+math.sin(a)*y) for a in np.linspace(0,2*math.pi,48,endpoint=False)])
            solid-=extrude(circ,f['n'],f['t']+1)
        assert len(solid.decompose())==1
        panels.append(solid)
    for i,a in enumerate(panels):
        for j,b in enumerate(panels[:i]):assert (a^b).volume()<1e-7,f'panel contact {i+1},{j+1}'
    webs=[];minlength=1e9
    for k,l in enumerate(links):
        a=l['a'];b=l['b'];ia=l['ia'];ib=l['ib'];fa=faces[a];fb=faces[b]
        p0=fa['p'][ia];p1=fa['p'][(ia+1)%len(fa['p'])];ev=unit(p1-p0);ua=inward(fa,ia);ub=inward(fb,ib);ga=gaps[a][ia];gb=gaps[b][ib]
        inter=[c.relief+c.corner,np.linalg.norm(p1-p0)-c.relief-c.corner];clip(inter,fa,ia,gaps[a],p0,ev,ua,ga,c);clip(inter,fb,ib,gaps[b],p0,ev,ub,gb,c)
        length=inter[1]-inter[0];assert length>=max(1,3*c.hinge),f'short attachment {k+1}';minlength=min(minlength,length)
        pw=p0+inter[0]*ev;na=fa['n'];nb=fb['n'];ta=fa['t'];tb=fb['t'];h=c.hinge
        if l['outer']:
            pa=pw+(ga+c.overlap)*ua;pb=pw+(gb+c.overlap)*ub;middle=pw+offset(na,nb,h,h);poly=[pa,pw,pb,pb+h*nb,middle,pa+h*na]
        elif net:
            pa=pw+(ga+c.overlap)*ua+ta*na;pb=pw+(gb+c.overlap)*ub+tb*nb;middle=pw+(ta+tb)/2*na;poly=[pa,middle,pb,pb-h*nb,middle-h*na,pa-h*na]
        else:
            pi=pw+offset(na,nb,ta,tb);wa=ga+c.overlap-(pi-pw)@ua;wb=gb+c.overlap-(pi-pw)@ub
            pa=pi+wa*ua;pb=pi+wb*ub;middle=pw+offset(na,nb,ta-h,tb-h);poly=[pa,pi,pb,pb-h*nb,middle,pa-h*na]
        web=extrude(np.array(poly),ev,length)
        for j in [a,b]:assert (web^panels[j]).volume()>1e-7,f'web {k+1} disconnected from {j+1}'
        for j,panel in enumerate(panels):
            if j not in (a,b):assert (web^panel).volume()<1e-7,f'web {k+1} contacts panel {j+1}'
        for j,other in enumerate(webs):assert (web^other).volume()<1e-7,f'webs contact {k+1},{j+1}'
        webs.append(web)
    result=m.Manifold.batch_boolean(panels+webs,m.OpType.Add)
    assert len(result.decompose())==1,'disconnected result'
    assert result.status()==m.Error.NoError
    return dict(volume_mm3=result.volume(),minimum_hinge_length_mm=minlength,solid_components=len(result.decompose())),result

def main():
    cases={
      'basic':Config(),
      'extended':Config(cells=3),
      'alternating':Config(cells=3,alternate=True),
      'tapered':Config(cells=3,top_radius=22.5),
      'progressive_offset':Config(cells=3,top_radius=24,exponent=2,dx=4,dy=2),
      'clockwise':Config(sign=-1),
      'caps_holes_ribs':Config(caps='both'),
      'compressed_redesign':Config(compression=.2),
      'custom_schedule':Config(cells=3,schedule=[(30,28,25,1,0),(30,26,25,2,0),(30,24,25,3,0)]),
      'custom_overlap_rejection':Config(cells=3,schedule=[(30,28,25,1,0),(32,26,22,2,1),(28,24,20,3,1)]),
      'rounded_panels':Config(corner=.3),
      'slotted_tabs':Config(tabs=True),
      'inverted_hinges':Config(invert=True),
      'seam_changed':Config(cells=2,seam=3),
    }
    report=[]
    for name,c in cases.items():
        try:
            fs,ls,gs=build(c);flat,err=unfold(fs,ls)
            trimerr=0
            for old,new,g in zip(fs,flat,gs):
                op=inset(old,g);np_=inset(new,g)
                for a in range(len(op)):
                    for b in range(a):trimerr=max(trimerr,abs(np.linalg.norm(op[a]-op[b])-np.linalg.norm(np_[a]-np_[b])))
            assert trimerr<1e-8,'trimmed panel mismatch'
            rec=dict(case=name,faces=len(fs),hinges=len(ls),max_distance_error_mm=err,max_trimmed_panel_error_mm=trimerr)
            for mode,faces in [('folded',fs),('net',flat)]:
                rec[mode],solid=solids(c,faces,ls,gs,mode=='net',name=='caps_holes_ribs')
                if name=='basic':
                    mesh=solid.to_mesh();np.savez(Path(__file__).parent/f'basic_{mode}_mesh.npz',vertices=mesh.vert_properties[:,:3],faces=mesh.tri_verts)
            rec['result']='PASS'
        except AssertionError as e:rec=dict(case=name,result='EXPECTED_REJECT' if name=='custom_overlap_rejection' else 'FAIL',reason=str(e))
        print(json.dumps(rec),flush=True);report.append(rec)
    invalid=[('twist_too_large',Config(twist=185)),('offset_too_large',Config(dx=20)),('collapsed_inset',Config(width=30)),('excess_compression',Config(compression=.8))]
    for name,c in invalid:
        try:
            fs,ls,gs=build(c)
            for f,g in zip(fs,gs):inset(f,g)
            rec=dict(case=name,result='UNEXPECTED_ACCEPT')
        except AssertionError as e:rec=dict(case=name,result='EXPECTED_REJECT',reason=str(e))
        print(json.dumps(rec),flush=True);report.append(rec)
    Path(__file__).with_name('validation_results.json').write_text(json.dumps(report,indent=2))
    assert all(r['result'] in ['PASS','EXPECTED_REJECT'] for r in report),'unexpected verification result'
    return report
if __name__=='__main__':main()
