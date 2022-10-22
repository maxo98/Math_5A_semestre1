using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Delaunay : MonoBehaviour
{

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform meshTransform;

    public bool voronoi;

    public Material voronoiMat;

    public GameObject point;

    // Start is called before the first frame update
    void Start()
    {
        DelaunayTriangulation();
        if(voronoi == true) VoronoiFromDelaunay();
    }

    // Update is called once per frame
    void Update()
    {
        /*Mesh mesh = meshFilter.mesh;

        p1.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[0+3*i]]);
        p2.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[1+3*i]]);
        p3.position = meshTransform.TransformPoint(mesh.vertices[mesh.triangles[2+3*i]]);

        cube.position = meshTransform.TransformPoint(GetCircleCenter(mesh.vertices[mesh.triangles[0+3*i]], 
        mesh.vertices[mesh.triangles[1+3*i]], 
        mesh.vertices[mesh.triangles[2+3*i]]));*/
        //Debug.Log(cube.position);
    }

    void DelaunayTriangulation()
    {
        int[] triangles = meshFilter.mesh.triangles;
        Vector3[] vertices = meshFilter.mesh.vertices;

        //for each triangles
        for(int i = 0; i < triangles.Length; i+=3)
        {
            
            Vector3 circleCenter = GetCircleCenter(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);
            float sqrRadius = (circleCenter - vertices[triangles[i]]).sqrMagnitude;

            int cpt = 0;
            bool found = false;

            //check delaunay condition for each vertex
            while(cpt < vertices.Length && found == false)
            {
                if(sqrRadius > (circleCenter - vertices[cpt]).sqrMagnitude)
                {
                    //Check that it's not a point of the current triangle
                    if(vertices[triangles[i]] != vertices[cpt] && vertices[triangles[i+1]] != vertices[cpt] && vertices[triangles[i+2]] != vertices[cpt])
                    {
                        //Debug.Log("Found 1");
                        found = true;
                    }else{
                        cpt++;
                    }
                }else{
                    cpt++;
                }
            }

            //If we need to flip it
            if(found == true)
            {
                bool reset = false;

                //Search to which triangle the vertex belongs
                for(int i2 = i; i2 < triangles.Length && reset == false; i2++)
                {
                    if(triangles[i2] == cpt)
                    {
                        //Search the common edge
                        int triangleIndex = i2 - (i2 % 3);

                        int p1 = -1;
                        int p2 = -1;
                        int pA3 = -1;

                        for(int i3 = 0; i3 < 3; i3++)
                        {
                            bool found2 = false;

                            for(int i4 = 0; i4 < 3 && found2 == false; i4++)
                            {
                                //Debug.Log(i + " " + triangleIndex);
                                //Debug.Log(i + " " + triangleIndex + " " + triangles[i + i3] + " " + triangles[triangleIndex + i4]);

                                if(triangles[i + i3] == triangles[triangleIndex + i4])
                                {
                                    if(p1 == -1)
                                    {
                                        p1 = triangles[i + i3];
                                        found2 = true;
                                    }else{
                                        p2 = triangles[i + i3];
                                        found2 = true;
                                    }
                                }
                            }
            
                            //Get isolated point of triangle A
                            if(found2 == false)
                            {
                                pA3 = triangles[i + i3];
                            }
                        }

                        //If the triangles do have a common egde
                        if(p2 != -1)
                        {
                            //Debug.Log("Found 2");

                            //Search for isolated point of triangle B
                            int pB3 = -1;
                            bool found2 = false;

                            for(int i3 = 0; i3 < 3 && found2 == false; i3++)
                            {
                                //Debug.Log(p1 + " " + p2 + " " + triangles[triangleIndex + i3] + " " + triangleIndex);
                                if(triangles[triangleIndex + i3] != p1 && triangles[triangleIndex + i3] != p2)
                                {
                                    pB3 = triangles[triangleIndex + i3];
                                    found2 = true;
                                }
                            }

                            //Debug.Log("result " + p1 + " " + p2 + " " + pA3 + " " + pB3);

                            //Do the backflip !
                            triangles[i] = pA3;
                            triangles[i+1] = pB3;
                            triangles[i+2] = p1;

                            triangles[triangleIndex] = p2;
                            triangles[triangleIndex+1] = pA3;
                            triangles[triangleIndex+2] = pB3;

                            //Start the search again
                            reset = true;
                            i = -3;
                        }
                    }
                }
            }
        }

        DoubleFaceIndices(ref triangles);

        meshFilter.mesh.triangles = triangles;
    }

    void VoronoiFromDelaunay()
    {
        foreach (Transform child in meshTransform.transform) {
            GameObject.Destroy(child.gameObject);
        }

        List<int> temp = new List<int>(meshFilter.mesh.triangles);
        temp.RemoveRange(meshFilter.mesh.triangles.Length/2, meshFilter.mesh.triangles.Length/2);
        int[] triangles = temp.ToArray();
        Vector3[] vertices = meshFilter.mesh.vertices;

        List<Vector3> lineVertices = new List<Vector3>();
        List<int> lineIndices = new List<int>();
        
        for(int i = 0; i < triangles.Length; i+=3)
        {
            Vector3 center = GetCircleCenter(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]]);
            center.z = vertices[triangles[i]].z;
            
            int centerIndex = lineVertices.Count;
            lineVertices.Add(center);

            List<Vector3> triangle = new List<Vector3>();
            triangle.Add(vertices[triangles[i]]);
            triangle.Add(vertices[triangles[i+1]]);
            triangle.Add(vertices[triangles[i+2]]);

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i]], vertices[triangles[i+1]], centerIndex);

            VoronoiFlip(center, triangle, 0, 1, ref lineIndices, ref lineVertices);

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i+1]], vertices[triangles[i+2]], centerIndex);

            VoronoiFlip(center, triangle, 1, 2, ref lineIndices, ref lineVertices);

            AddVoronoiLine(ref lineVertices, ref lineIndices, vertices[triangles[i]], vertices[triangles[i+2]], centerIndex);

            VoronoiFlip(center, triangle, 2, 0, ref lineIndices, ref lineVertices);
        }

        int offset = lineVertices.Count;

        lineVertices.InsertRange(lineVertices.Count, vertices);

        List<int> cellCoreIndices = new List<int>();

        for(int i = 0; i < vertices.Length; i++)
        {
            Instantiate(point, meshTransform.TransformPoint(vertices[i]), new Quaternion(), meshTransform);

            cellCoreIndices.Add(offset + i);
        }

        meshFilter.mesh.Clear();
        meshFilter.mesh.subMeshCount = 2;
        meshFilter.mesh.SetVertices(lineVertices);
        meshFilter.mesh.SetIndices(lineIndices, MeshTopology.Lines, 0);
        meshFilter.mesh.SetIndices(cellCoreIndices, MeshTopology.Points, 1);

        meshRenderer.material = voronoiMat;
    }

    void AddVoronoiLine(ref List<Vector3> lineVertices, ref List<int> lineIndices, in Vector3 vertA, in Vector3 vertB, int centerIndex)
    {
        Vector3 lineMiddle = (vertA + vertB);
        lineMiddle.x /= 2;
        lineMiddle.y /= 2;
        lineMiddle.z = vertA.z;

        int lineIndex = lineVertices.FindIndex(0, lineVertices.Count-1, d => d == lineMiddle);

        if(lineIndex == -1)
        {
            lineIndex = lineVertices.Count;
            lineVertices.Add(lineMiddle);
        }

        lineIndices.Add(centerIndex);
        lineIndices.Add(lineIndex);
    }

    //Returns the number of time the edge appears in the mesh
    int FindEdge((int, int) edge, in int[] triangles)
    {
        int i = 0;
        int found = 0;

        while(i < triangles.Length)
        {
            if((triangles[i], triangles[i+1]) == edge || (triangles[i+1], triangles[i]) == edge)
            {
                found++;
            }else if((triangles[i+1], triangles[i+2]) == edge || (triangles[i+2], triangles[i+1]) == edge)
            {
                found++;
            }else if((triangles[i], triangles[i+2]) == edge || (triangles[i+2], triangles[i]) == edge)
            {
                found++;
            }
                
            i+=3;
        }

        return found;
    }

    void VoronoiFlip(in Vector3 center, in List<Vector3> triangle, int a, int b, ref List<int> lineIndices, ref List<Vector3> lineVertices)
    {
        //Debug.Log(IsInsideTriangle(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]], center));
        Vector3 lineMiddle = (center + lineVertices[lineIndices[lineIndices.Count-1]]);
        lineMiddle.x /= 2;
        lineMiddle.y /= 2;
        lineMiddle.z = center.z;

        //if(IsInsideTriangle(vertices[triangles[i]], vertices[triangles[i+1]], vertices[triangles[i+2]], lineMiddle) == false)
        bool clockwise = isClockwise(triangle);
        int orientation = Orientation(triangle[a], triangle[b], center);

        if((clockwise == true && orientation == 1) || (clockwise == false && orientation == 2))
        {
            lineVertices.Add((center - lineVertices[lineVertices.Count-1]) + center);
            lineIndices[lineIndices.Count-1] = lineVertices.Count - 1;
        }
    }

    static public bool isClockwise(in List<Vector3> polygon)
    {
        float sum = 0;

        for(int i = 0; i < polygon.Count; i++)
        {
            Vector3 nextVec = (i+1) < polygon.Count ? polygon[i+1] : polygon[0];

            sum += (nextVec.x - polygon[i].x) * (nextVec.y + polygon[i].y);
        }

        //Clockwise if sum > 0
        //CounterClockwise if sum < 0
        return (sum > 0);
    }

    // 0 --> a, b and c are colinear
    // 1 --> c is on the right
    // 2 --> c is on the left
    static public byte Orientation(Vector3 a, Vector3 b, Vector3 c)
    {
        double ABx = b.x - a.x;
        double ABy = b.y - a.y;

        double ACx = c.x - a.x;
        double ACy = c.y - a.y;

        double val = ACx * ABy - ACy * ABx;

        if (val == 0) return 0; // collinéire 
        return (byte)((val < 0) ? 1 : 2); // sens horaire ou sens anti-hoaraire
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

    public void DoubleFaceIndices(ref int[] indices)
    {
        int n = 3;

        if((indices.Length % n) != 0)
        {
            return;
        }

        int indicesLength = indices.Length;

        Array.Resize(ref indices, indices.Length*2);

        for(int i = 0; i < indicesLength; i += n)
        {
            for(int cpt = 0; cpt < n; cpt++)
            {
                indices[indicesLength+i+cpt] = indices[i+n-1-cpt];
            }
        }
    }
}
