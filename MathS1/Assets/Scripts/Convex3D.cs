using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Convex3D : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform meshTransform;

    public Transform pointToAdd;

    public Transform sphere;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 p;

        LineLineIntersection(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 2, 0), out p);

        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[3*4]{0,1,2, 1,3,2, 0,2,3, 0,3,1};//Need to always swap between clockwise and counterclockwise index

        vertices[0] = new Vector3(1, 0, 0);
        vertices[1] = new Vector3(0, 1, 0);
        vertices[2] = new Vector3(1, 1, 0);
        vertices[3] = new Vector3(1, 0, 1);
        
        meshFilter.sharedMesh.SetVertices(vertices);
        meshFilter.sharedMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        // meshFilter.mesh.SetVertices(vertices);
        // meshFilter.mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        Vector3 center;

        Debug.Log(GetSphereCenter(vertices[0], vertices[1], vertices[2], vertices[3], out center));

        sphere.localPosition = center;
        float radius = Vector3.Distance(vertices[0], center) * 2;
        sphere.localScale = new Vector3(radius, radius, radius);
    }

    public void AddPoint()
    {
        Vector3 point = meshTransform.InverseTransformPoint(pointToAdd.position);
        //Vector3 point = pointToAdd.position;

        Dictionary<(int, int), int> segementColors = new Dictionary<(int, int), int>();
        
        List<int> triangles = new List<int>(meshFilter.sharedMesh.triangles);
        List<Vector3> vertices = new List<Vector3>(meshFilter.sharedMesh.vertices);

        // List<int> triangles = new List<int>(meshFilter.mesh.triangles);
        // List<Vector3> vertices = new List<Vector3>(meshFilter.mesh.vertices);

        for(int i = 0; i < triangles.Count; i+=3)
        {
            Vector3 normal = GetNormal(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);

            Vector3 test = point - vertices[triangles[i]];

            if(Vector3.Dot(normal, test) > 0)
            {
                for(int cpt = 0; cpt < 3; cpt++)
                {
                    int second = (cpt + 1) == 3 ? 0 : (cpt+1);

                    if(segementColors.ContainsKey((triangles[i+cpt], triangles[i+second])) == true)
                    {
                        segementColors[(triangles[i+cpt], triangles[i+second])]++;
                    }else{
                        segementColors.Add((triangles[i+second], triangles[i+cpt]), 1);
                    }
                }

                triangles.RemoveRange(i, 3);
                i-=3;
            }
        }

        int trianglesCount = triangles.Count;

        foreach(KeyValuePair<(int, int), int> entry in segementColors)
        {
            if(entry.Value == 1)
            {
                triangles.Add(entry.Key.Item2);
                triangles.Add(entry.Key.Item1);
                triangles.Add(vertices.Count);
            }
        }

        vertices.Add(point);

        int[] triangleArray = triangles.ToArray();
        
        meshFilter.sharedMesh.SetVertices(vertices.ToArray());
        meshFilter.sharedMesh.SetIndices(triangleArray, MeshTopology.Triangles, 0);
        // meshFilter.mesh.SetVertices(vertices.ToArray());
        // meshFilter.mesh.SetIndices(triangleArray, MeshTopology.Triangles, 0);
    }


//     Is the sphere well defined?
//     (1) Check that A and B are not coincident (=> failure).
//     (2) Find the line AB and check that C does not lie on it (=> failure).
//     (3) Find the plane ABC and check that D does not lie in it (=> failure).
// Yes. Find its centre.
//     (1) Find the perpendicular bisectors of AB and AC.
//     (2) Find their point of intersection (P).
//     (3) Find the normal to the plane ABC passing through P (line N).
//     (4) Find the plane containing N and D; find the point E on the
// 	ABC circle in this plane (if D lies on N, take E as A).
//     (4) Find the perpendicular bisector of ED (line L)
//     (5) Find the point of intersection of N and L (Q).
    
    public bool GetSphereCenter(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 e)
    {
        e = Vector3.zero;

        //If they are colinear we can't
        if(IsColinear(a, b, c) == true)
        {
            return false;
        } 

        if(IsCoplanar(a, b, c, d) == true) return false;//Check that they a not coplanar

        //Find the intersection of the triangles normal originating from the circumcenter of the triangles
        Vector3 p1 = GetCircumCenter(a, b, c);
        Vector3 p2 = p1 + GetNormal(a, b, c);

        Vector3 p3 = GetCircumCenter(b, d, c);
        Vector3 p4 = p3 + GetNormal(b, d, c);

        return LineLineIntersection(p1, p2, p3, p4, out e);
    }

    Vector3 GetCircumCenter(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ac = c - a ;
        Vector3 ab = b - a ;
        Vector3 abXac = Vector3.Cross(ab, ac) ;

        // this is the vector from a TO the circumsphere center
        Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab )*ac.sqrMagnitude + Vector3.Cross(ac, abXac)*ab.sqrMagnitude) / (2.0f*abXac.sqrMagnitude) ;

        // The 3 space coords of the circumsphere center then:
        return (a  +  toCircumsphereCenter); // now this is the actual 3space location
    }

    bool IsColinear(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = a - b;
        Vector3 ac = a - c;

        return !(((ab.x == 0 && ac.x != 0) || (ab.x != 0 && ac.x == 0)
        || (ab.y == 0 && ac.y != 0) || (ab.y != 0 && ac.y == 0) 
        || (ab.z == 0 && ac.z != 0) || (ab.z != 0 && ac.z == 0)) 
        || (ab.x / ac.x) != (ab.y / ac.y) 
        || (ab.x / ac.x) != (ab.z / ac.z));
    }

    //Explanations here:
    // http://mathworld.wolfram.com/Line-LineIntersection.html
    bool LineLineIntersection(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 p5)
    {
        Vector3 a = p2 - p1;
        Vector3 b = p4 - p3;
        Vector3 c = p3 - p1;

        Vector3 d = Vector3.Cross(a, b);
        float e = Vector3.Dot( Vector3.Cross(c, b), d);

        if(d.sqrMagnitude == 0)
        {
            p5 = Vector3.zero;
            return false;
        }else{
            p5 = (p1 + a * ( e / d.sqrMagnitude ));
            return true;
        }
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;

        return Vector3.Cross(ab, ac);
    }

    public Vector3 GetTriangleCenter(Vector3 a, Vector3 b, Vector3 c)
    {
        return (a + b + c)/3;
    }

    public bool IsCoplanar(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return ((a.x * (b.y*c.z + b.z*d.y + c.y* d.z - c.z*d.y - b.z*c.y - b.y*d.z)
        - b.x * (a.y*c.z + a.z*d.y + c.y* d.z - c.z*d.y - a.z*c.y - a.y*d.z)
        + c.x * (a.y*b.z + a.z*d.y + b.y* d.z - b.z*d.y - a.z*b.y - a.y*d.z)
        - d.x * (a.y*b.z + a.z*c.y + b.y* c.z - b.z*c.y - a.z*b.y - a.y*c.z)) == 0);
    }
}
