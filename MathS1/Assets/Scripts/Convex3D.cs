using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Convex3D : MonoBehaviour
{

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform meshTransform;

    public Transform pointToAdd;

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] vertices = new Vector3[4];
        int[] indices = new int[3*4]{0,1,2, 1,3,2, 0,2,3, 0,3,1};//Need to always swap between clockwise and counterclockwise index

        vertices[0] = new Vector3(1, 0, 0);
        vertices[1] = new Vector3(0, 1, 0);
        vertices[2] = new Vector3(1, 1, 0);
        vertices[3] = new Vector3(1, 0, 1);

        meshFilter.mesh.SetVertices(vertices);
        meshFilter.mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

    public void AddPoint()
    {
        Vector3 point = meshTransform.InverseTransformPoint(pointToAdd.position);
        //Vector3 point = pointToAdd.position;

        Dictionary<(int, int), int> segementColors = new Dictionary<(int, int), int>();

        List<int> triangles = new List<int>(meshFilter.mesh.triangles);
        //triangles.RemoveRange(meshFilter.mesh.triangles.Length/2, meshFilter.mesh.triangles.Length/2);
        List<Vector3> vertices = new List<Vector3>(meshFilter.mesh.vertices);

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

                Debug.Log(triangles[entry.Key.Item2]);
                Debug.Log(triangles[entry.Key.Item1]);
            }
        }

        vertices.Add(point);

        int[] triangleArray = triangles.ToArray();

        meshFilter.mesh.SetVertices(vertices.ToArray());
        meshFilter.mesh.SetIndices(triangleArray, MeshTopology.Triangles, 0);
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;

        return Vector3.Cross(ab, ac);
    }
}
