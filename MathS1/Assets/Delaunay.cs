using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay : MonoBehaviour
{

    public MeshFilter meshFilter;
    public Transform meshTransform;

    public Transform cube;
    public int i;

    public Transform p1;
    public Transform p2;
    public Transform p3;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Mesh mesh = meshFilter.mesh;

        p1.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[0+3*i]]);
        p2.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[1+3*i]]);
        p3.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[2+3*i]]);

        cube.position = meshTransform.TransformPoint(GetCircleCenter(mesh.vertices[mesh.triangles[0+3*i]], 
        mesh.vertices[mesh.triangles[1+3*i]], 
        mesh.vertices[mesh.triangles[2+3*i]]));
        //Debug.Log(cube.position);
    }

    Vector3 GetCircleCenter(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 pos = Vector3.zero;

        float x12 = p1.x - p2.x;
        float x13 = p1.x - p3.x;
    
        float y12 = p1.y - p2.y;
        float y13 = p1.y - p3.y;
    
        float y31 = p3.y - p1.y;
        float y21 = p2.y - p1.y;
    
        float x31 = p3.x - p1.x;
        float x21 = p2.x - p1.x;
    
        float sx13 = p1.x*p1.x - p3.x*p3.x;
    
        float sy13 = p1.y*p1.y - p3.y*p3.y;
    
        float sx21 = p2.x*p2.x - p1.x*p1.x;
        float sy21 = p2.y*p2.y - p1.y*p1.y;
    
        pos.y = -((sx13) * (x12) + (sy13) * (x12) + (sx21) * (x13) + (sy21) * (x13)) / (2 * ((y31) * (x12) - (y21) * (x13)));
        pos.x = -((sx13) * (y12) + (sy13) * (y12) + (sx21) * (y13) + (sy21) * (y13)) / (2 * ((x31) * (y12) - (x21) * (y13)));

        return pos;
    }
}
