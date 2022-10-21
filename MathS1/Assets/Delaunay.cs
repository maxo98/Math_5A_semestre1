using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        DelaunayTriangulation();
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
                //Search to which triangle the vertex belongs
                for(int i2 = 0; i2 < triangles.Length; i2++)
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

                            //Do the backflip !
                            triangles[i] = pA3;
                            triangles[i+1] = pB3;
                            triangles[i+2] = p1;

                            triangles[triangleIndex] = pA3;
                            triangles[triangleIndex+1] = pB3;
                            triangles[triangleIndex+2] = p2;

                            //Start the search again
                            i = 0;
                        }
                    }
                }
            }
        }

        DoubleFaceIndices(ref triangles);

        meshFilter.mesh.triangles = triangles;
    }

    bool FindEdge((int, int) edge, in int[] triangles, out int i)
    {
        i = 0;
        bool found = false;

        while(i < triangles.Length && found == false)
        {
            if((triangles[i], triangles[i+1]) == edge || (triangles[i+1], triangles[i]) == edge)
            {
                found = true;
            }else if((triangles[i+1], triangles[i+2]) == edge || (triangles[i+2], triangles[i+1]) == edge)
            {
                found = true;
            }else if((triangles[i], triangles[i+2]) == edge || (triangles[i+2], triangles[i]) == edge)
            {
                found = true;
            }else{
                i+=3;
            }
        }

        return found;
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

    public void DoubleFaceVertices(ref Vector3[] vertices)
    {
        int verticesLength = vertices.Length;

        Array.Resize(ref vertices, vertices.Length*2);
        Array.Copy(vertices, 0, vertices, verticesLength, verticesLength);
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
