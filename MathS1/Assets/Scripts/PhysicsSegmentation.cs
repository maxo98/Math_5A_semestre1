using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;

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
        skinMesh.BakeMesh(tmp, true);
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
            int numberOfBonesForThisVertex = bonesPerVertex[vertIndex];

            // For each vertex, iterate over its BoneWeights
            for (int i = 0; i < numberOfBonesForThisVertex; i++)
            {
                int index = boneWeights[boneWeightIndex].boneIndex;
                
                //if(boneWeights[boneWeightIndex].weight > 0.3)
                //{
                    bonesVertices[index].Add(vertIndex);
                //}

                boneWeightIndex++;
            }
        }

        for(int i = 0; i < bonesVertices.Count; i++)
        {
            if(bonesVertices[i].Count == 0) continue;

            //Get barycenter
            Vector3 center = Vector3.zero;
            List<Vector3> localVert = new List<Vector3>();
            Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;

            for(int cpt = 0; cpt < bonesVertices[i].Count; cpt++)
            {

                localVert.Add(skinMesh.bones[i].InverseTransformPoint(transform.TransformPoint(vert[bonesVertices[i][cpt]])));

                if(localVert[localVert.Count-1].x < min.x)
                {
                    min.x = localVert[localVert.Count-1].x;
                }else if(localVert[localVert.Count-1].x > max.x)
                {
                    max.x = localVert[localVert.Count-1].x;
                }

                if(localVert[localVert.Count-1].y < min.y)
                {
                    min.y = localVert[localVert.Count-1].y;
                }else if(localVert[localVert.Count-1].y > max.y)
                {
                    max.y = localVert[localVert.Count-1].y;
                }

                if(localVert[localVert.Count-1].z < min.z)
                {
                    min.z = localVert[localVert.Count-1].z;
                }else if(localVert[localVert.Count-1].z > max.z)
                {
                    max.z = localVert[localVert.Count-1].z;
                }
            }

            center = new Vector3((max.x+min.x)/2, (max.y+min.y)/2, (max.z+min.z)/2);
            Vector3 size = new Vector3(max.x-min.x, max.y-min.y, max.z-min.z);

            // BoxCollider boxColl = skinMesh.bones[i].gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;

            // boxColl.center = center;
            // boxColl.size = size;

            //Sphere
            float maxDist = 0;

            for(int cpt = 0; cpt < localVert.Count; cpt++)
            {
                float dist = (localVert[cpt] - center).sqrMagnitude;
                
                if(dist > maxDist)
                {
                    maxDist = dist;
                }
            }

            maxDist = Mathf.Sqrt(maxDist);

            // SphereCollider sc = skinMesh.bones[i].gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;

            // sc.center = center;
            // sc.radius = maxDist;

            //Capsule
            float cylMaxY = 0;
            float cylMinY = 0;
            float radius = 0.001f;

            for(int cpt = 0; cpt < bonesVertices[i].Count; cpt++)
            {
                float newRadius = Mathf.Sqrt(localVert[cpt].x * localVert[cpt].x + localVert[cpt].z * localVert[cpt].z);

                if(radius < newRadius)
                {
                    radius = newRadius;
                }
            }

            for(int cpt = 0; cpt < bonesVertices[i].Count; cpt++)
            {
                if(cylMaxY < localVert[cpt].y)
                {
                    float newRadius = (localVert[cpt] - new Vector3(0, cylMaxY, 0)).magnitude;

                    if(newRadius > radius)
                    {
                        cylMaxY += (newRadius - radius);
                    }

                }else if(cylMinY > localVert[cpt].y)
                {
                    float newRadius = (localVert[cpt] - new Vector3(0, cylMinY, 0)).magnitude;

                    if(newRadius > radius)
                    {
                        cylMinY -= (newRadius - radius);
                    }
                }
            }

            //radius = Mathf.Sqrt(radius);

            float height = (cylMaxY - cylMinY) + radius;
            Debug.Log(cylMinY);

            CapsuleCollider capsule = skinMesh.bones[i].gameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;

            capsule.height = height;
            capsule.center = new Vector3(0, (cylMaxY + cylMinY)/2, 0);
            capsule.direction = 1;//y
            
            capsule.radius = radius;

        }
    }
}
