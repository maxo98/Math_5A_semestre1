using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Segmentation : MonoBehaviour
{
    public List<SkinnedMeshRenderer> skinMesh;
    Mesh mesh;
    public List<MeshFilter> meshFilter;
    public GameObject point;
    public List<Transform> meshTransform;

    private IntPtr _dataSetInstance;
    private IntPtr _genomeInstance;
    private IntPtr _networkInstance;

    private List<Tuple<Vector3, List<bool>>> _bones;

    // Start is called before the first frame update

    // Update is called once per frame
    void Start()
    {
        _dataSetInstance = InitDataSet(skinMesh[0].bones.Length);

        _bones = new List<Tuple<Vector3, List<bool>>>();

        for(int meshIndex = 0; meshIndex < skinMesh.Count; meshIndex++)
        {
            List<List<int>> bonesVertices = new List<List<int>>();
            
            for(int i = 0; i < skinMesh[meshIndex].bones.Length; i++)
            {
                bonesVertices.Add(new List<int>());
            }

            Mesh tmp = new Mesh();
            skinMesh[meshIndex].BakeMesh(tmp, true);
            mesh = skinMesh[meshIndex].sharedMesh;
            // Vector3[] vert = tmp.vertices;

            Vector3[] vert = meshFilter[meshIndex].mesh.vertices;
            
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

            Vector3 min = mesh.vertices[0];
            Vector3 max = mesh.vertices[0];


            for (int vertIndex = 1; vertIndex < mesh.vertexCount; vertIndex++)
            {
                if(min.x > mesh.vertices[vertIndex].x)
                {
                    min.x = mesh.vertices[vertIndex].x;
                }

                if(max.x < mesh.vertices[vertIndex].x)
                {
                    max.x = mesh.vertices[vertIndex].x;
                }

                if(min.y > mesh.vertices[vertIndex].y)
                {
                    min.y = mesh.vertices[vertIndex].y;
                }

                if(max.y < mesh.vertices[vertIndex].y)
                {
                    max.y = mesh.vertices[vertIndex].y;
                }

                if(min.z > mesh.vertices[vertIndex].z)
                {
                    min.z = mesh.vertices[vertIndex].z;
                }

                if(max.z < mesh.vertices[vertIndex].z)
                {
                    max.z = mesh.vertices[vertIndex].z;
                }
            }


            // Iterate over the vertices
            for (int vertIndex = 0; vertIndex < mesh.vertexCount; vertIndex++)
            {
                var newList = new List<bool>(skinMesh[meshIndex].bones.Length);

                for (int i = 0; i < skinMesh[meshIndex].bones.Length; i++)
                {
                    newList.Add(false);
                }

                int numberOfBonesForThisVertex = bonesPerVertex[vertIndex];

                // For each vertex, iterate over its BoneWeights
                for (int i = 0; i < numberOfBonesForThisVertex; i++)
                {
                    int index = boneWeights[boneWeightIndex].boneIndex;

                    if(boneWeights[boneWeightIndex].weight > 0.05)
                    {
                        bonesVertices[index].Add(vertIndex);
                        newList[index] = true;
                    }

                    boneWeightIndex++;
                }

                Vector3 vec = mesh.vertices[vertIndex];

                vec.x = -1 + (vec.x - min.x) * 2 / (max.x - min.x);
                vec.y = -1 + (vec.y - min.y) * 2 / (max.y - min.y);
                vec.y = -1 + (vec.z - min.z) * 2 / (max.z - min.z);
                
                var newTuple = Tuple.Create(vec, newList);

                _bones.Add(newTuple);
            }

            for (int idx = 0; idx < _bones.Count; idx++)
            {
                var pair = _bones[idx];
                float[] positionConvertedToArray = new float[3];

                for (int i = 0; i < 3; i++)
                {
                    positionConvertedToArray[i] = pair.Item1[i];
                }
                
                SetNewVertex(_dataSetInstance, positionConvertedToArray, 3, pair.Item2.ToArray(), pair.Item2.Count);
            }

        }

        _genomeInstance = CreateGenome(4, skinMesh[0].bones.Length, 1, 20);
        _networkInstance = CreateNeuralNetwork(_genomeInstance);
        var debugLog = Train(_dataSetInstance, _networkInstance, 50000000, 0.2f);
        Debug.Log(debugLog);

        ApplyBackProp(_genomeInstance, _networkInstance);
        
        int result = 0;
        
        for (int idx = 0; idx < _bones.Count; idx++)
        {
            bool correct = SetCompute(_dataSetInstance, _networkInstance, idx);

            if (correct)
            {
                result += 1;
            }
        }
        
        SaveGenome(_genomeInstance);

        float[] data = new float[31];

        Debug.Log("bones " + _bones.Count);
        Debug.Log("result : " + result);
    }

    void Update()
    {
        
    }

    void OnDestroy()
    {
        DeleteInstance(_dataSetInstance);
        DeleteInstance(_genomeInstance);
        DeleteInstance(_networkInstance);
    }

    [DllImport("machine learning algo")]
    static extern IntPtr InitDataSet(int boneSize);
    
    [DllImport("machine learning algo")]
    static extern void SetNewVertex(IntPtr dataset, float[] position, int lengthPosition, bool[] linkedBones, int lengthLink);

    [DllImport("machine learning algo")]
    static extern void DeleteInstance(IntPtr instance);
 
    [DllImport("machine learning algo")]
    static extern void DeleteArrayInstance(IntPtr arrayInstance);

    [DllImport("machine learning algo")]
    static extern bool Train(IntPtr dataset, IntPtr network, int epoch, float lr);

    [DllImport("machine learning algo")]
    static extern IntPtr CreateNeuralNetwork(IntPtr gen);

    [DllImport("machine learning algo")]
    static extern IntPtr CreateGenome(int input, int output, int layer, int node);

    [DllImport("machine learning algo")]
    static extern void SaveGenome(IntPtr gen);

    [DllImport("machine learning algo")]
    static extern void ApplyBackProp(IntPtr gen, IntPtr network);

    [DllImport("machine learning algo")]
    static extern bool SetCompute(IntPtr dataset, IntPtr network, int idx);

    [DllImport("machine learning algo")]
    static extern int GetOutputFromGenome(IntPtr genome);

    [DllImport("machine learning algo")]
    static extern IntPtr GetLinkedBones(IntPtr dataset, int idx);
}
