using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class PhysicsSegmentation : MonoBehaviour
{
    //public MeshFilter meshFilter;
    public SkinnedMeshRenderer skinMesh;
    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        List<List<int>> bonesVertices = new List<List<int>>();
        
        for(int i = 0; i < skinMesh.bones.Length; i++)
        {
            bonesVertices.Add(new List<int>());
        }

        Mesh tmp = new Mesh();
        skinMesh.BakeMesh(tmp);
        mesh = skinMesh.sharedMesh;
        Vector3[] vert = tmp.vertices;

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
            float totalWeight = 0f;
            int numberOfBonesForThisVertex = bonesPerVertex[vertIndex];

            // For each vertex, iterate over its BoneWeights
            for (int i = 0; i < numberOfBonesForThisVertex; i++)
            {
                int index = boneWeights[boneWeightIndex].boneIndex;
                bonesVertices[index].Add(vertIndex);

                boneWeightIndex++;
            }
        }

        for(int i = 0; i < bonesVertices.Count; i++)
        {
            if(bonesVertices[i].Count == 0) continue;

            //Get barycenter
            Vector3 center = Vector3.zero;

            for(int cpt = 0; cpt < bonesVertices[i].Count; cpt++)
            {
                center += vert[bonesVertices[i][cpt]];
            }

            center = new Vector3(center.x/bonesVertices[i].Count, center.y/bonesVertices[i].Count, center.z/bonesVertices[i].Count);

            //Sphere
            float maxDist = 0;

            for(int cpt = 0; cpt < bonesVertices[i].Count; cpt++)
            {
                float dist = (vert[bonesVertices[i][cpt]] - center).sqrMagnitude;
                
                if(dist > maxDist)
                {
                    maxDist = dist;
                }
            }

            maxDist = Mathf.Sqrt(maxDist) * 2;

            SphereCollider sc = skinMesh.bones[i].gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;

            center = skinMesh.bones[i].InverseTransformPoint(center);

            sc.center = new Vector3(center.x/skinMesh.bones[i].lossyScale.x, center.y/skinMesh.bones[i].lossyScale.y, center.z/skinMesh.bones[i].lossyScale.z);
            sc.radius = (maxDist/skinMesh.bones[i].lossyScale.x) /2;

            //Cylinder

        }
    }
}
