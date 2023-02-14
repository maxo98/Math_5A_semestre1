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
    void Start()
    {
        _dataSetInstance = InitDataSet(skinMesh[0].bones.Length);

        for(int meshIndex = 0; meshIndex < skinMesh.Count; meshIndex++)
        {
            _bones = new List<Tuple<Vector3, List<bool>>>();
            
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
                    newList[index] = true;
                    
                    if(boneWeights[boneWeightIndex].weight > 0.05)
                    {
                        bonesVertices[index].Add(vertIndex);
                    }

                    boneWeightIndex++;
                }
                
                var newTuple = Tuple.Create(mesh.vertices[vertIndex], newList);
                _bones.Add(newTuple);
            }

            for (int idx = 0; idx < _bones.Count; idx++)
            {
                /*for (int i = 0; i < 3; i++)
                {
                    GameObject obj = Instantiate(point, meshTransform[meshIndex].TransformPoint(vert[0][bonesVertices[cpt]]), Quaternion.identity);
                    obj.transform.localScale = new Vector3(20f, 20f, 20f);
                }*/

                    var pair = _bones[idx];
                    float[] positionConvertedToArray = new float[3];

                    for (int i = 0; i < 3; i++)
                    {
                        positionConvertedToArray[i] = pair.Item1[i];
                    }

                    SetNewVertex(_dataSetInstance, positionConvertedToArray, 3, pair.Item2.ToArray(), pair.Item2.Count);
            }

        }

        Debug.Log(skinMesh.Count);

        _genomeInstance = CreateGenome(4, skinMesh[0].bones.Length, 2, 8);
        _networkInstance = CreateNeuralNetwork(_genomeInstance);

        Train(_dataSetInstance, _networkInstance, 10, 0.1f);

        ApplyBackProp(_genomeInstance, _networkInstance);

        SaveGenome(_genomeInstance);
        
        Debug.Log("Eval " + Evaluate(_genomeInstance, _networkInstance));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        DeleteInstance(_dataSetInstance);
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
    static extern void Train(IntPtr dataset, IntPtr network, int epoch, float lr);

    [DllImport("machine learning algo")]
    static extern IntPtr CreateNeuralNetwork(IntPtr gen);

    [DllImport("machine learning algo")]
    static extern IntPtr CreateGenome(int input, int output, int layer, int node);

    [DllImport("machine learning algo")]
    static extern void SaveGenome(IntPtr gen);

    [DllImport("machine learning algo")]
    static extern void ApplyBackProp(IntPtr gen, IntPtr network);

    [DllImport("machine learning algo")]
    static extern int Evaluate(IntPtr dataset, IntPtr network);
}
