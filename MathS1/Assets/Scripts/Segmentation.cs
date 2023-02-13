using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

public class Segmentation : MonoBehaviour
{
    public SkinnedMeshRenderer skinMesh;
    Mesh mesh;
    public MeshFilter meshFilter;
    public GameObject point;
    public Transform meshTransform;

    

    // Start is called before the first frame update
    void Start()
    {
        List<List<int>> bonesVertices = new List<List<int>>();
        
        for(int i = 0; i < skinMesh.bones.Length; i++)
        {
            bonesVertices.Add(new List<int>());
        }

        Mesh tmp = new Mesh();
        skinMesh.BakeMesh(tmp, true);
        mesh = skinMesh.sharedMesh;
        // Vector3[] vert = tmp.vertices;

        Vector3[] vert = meshFilter.mesh.vertices;

        // Get the number of bone weights per vertex
        NativeArray<byte> bonesPerVertex = mesh.GetBonesPerVertex();
        if (bonesPerVertex.Length == 0)
        {
            return;
        }

        // Get all the bone weights, in vertex index order
        NativeArray<BoneWeight1> boneWeights = mesh.GetAllBoneWeights();

        // Keep track of where we are in the array of BoneWeights, as we iterate over the vertices
        int boneWeightIndex = 0;

        // Iterate over the vertices
        for (int vertIndex = 0; vertIndex < mesh.vertexCount; vertIndex++)
        {
            int numberOfBonesForThisVertex = bonesPerVertex[vertIndex];

            // For each vertex, iterate over its BoneWeights
            for (int i = 0; i < numberOfBonesForThisVertex; i++)
            {
                int index = boneWeights[boneWeightIndex].boneIndex;
                
                if(boneWeights[boneWeightIndex].weight > 0.05)
                {
                    bonesVertices[index].Add(vertIndex);
                }

                boneWeightIndex++;
            }
        }

        Debug.Log(bonesVertices[0].Count);

        for(int cpt = 0; cpt < bonesVertices[0].Count; cpt++)
        {
            GameObject obj = Instantiate(point, meshTransform.TransformPoint(vert[bonesVertices[0][cpt]]), Quaternion.identity);
            obj.transform.localScale = new Vector3(20f, 20f, 20f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
