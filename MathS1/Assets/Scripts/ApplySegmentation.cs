using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ApplySegmentation : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] public GameObject point;
    private Mesh mesh;
    private IntPtr _genomeInstance;
    private IntPtr _networkInstance;
    private Vector3 min;
    private Vector3 max;
    private List<List<GameObject>> vertices;
    private int boneIdxShow = 0;
    private int nbBones;
    public Transform meshTransform;

    void Start()
    {
        _genomeInstance = LoadGenome();
        _networkInstance = CreateNeuralNetwork(_genomeInstance);
        nbBones = GetOutputFromGenome(_genomeInstance);
        Debug.Log(nbBones);
        vertices = new List<List<GameObject>>();

        for (int i = 0; i < nbBones; i++)
        {
            vertices.Add(new List<GameObject>());
        }

        mesh = meshFilter.mesh;
        min = mesh.vertices[0];
        max = mesh.vertices[0];
        float[] input = new float[4];
        float[] output = new float[nbBones];
        
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

        for (int vertIndex = 0; vertIndex < mesh.vertexCount; vertIndex++)
        {
            input[0] = -1 + (mesh.vertices[vertIndex][0] - min.x) * 2 / (max.x - min.x);
            input[1] = -1 + (mesh.vertices[vertIndex][1] - min.y) * 2 / (max.y - min.y);
            input[2] = -1 + (mesh.vertices[vertIndex][2] - min.z) * 2 / (max.z - min.z);
            // input[0] = mesh.vertices[vertIndex][0];
            // input[1] = mesh.vertices[vertIndex][1];
            // input[2] = mesh.vertices[vertIndex][2];
            input[3] = 0.5f;
            
            IntPtr outputPtr = GetComputeResult(_networkInstance, input, 4, nbBones);
            Marshal.Copy(outputPtr, output, 0, nbBones);
            DeleteInstance(outputPtr);

            for (int i = 0; i < nbBones; i++)
            {
                Debug.Log(output[i]);

                if (output[i] > 0)
                {
                    
                    GameObject obj = Instantiate(point, meshTransform.TransformPoint(mesh.vertices[vertIndex]), Quaternion.identity);
                    obj.SetActive(false);
                    vertices[i].Add(obj);
                }
            }
        }

        for (int idxVertix = 0 ; idxVertix < vertices[boneIdxShow].Count; idxVertix++)
        {
            vertices[boneIdxShow][idxVertix].SetActive(true);
        }
    }

    public void OnClickLeft()
    {
        for (int idxVertix = 0 ; idxVertix < vertices[boneIdxShow].Count; idxVertix++)
        {
            vertices[boneIdxShow][idxVertix].SetActive(false);
        }

        boneIdxShow -= 1;
        if (boneIdxShow < 0)
            boneIdxShow = nbBones - 1;
        
        for (int idxVertix = 0 ; idxVertix < vertices[boneIdxShow].Count; idxVertix++)
        {
            vertices[boneIdxShow][idxVertix].SetActive(true);
        }
    }

    public void OnClickRight()
    {
        for (int idxVertix = 0 ; idxVertix < vertices[boneIdxShow].Count; idxVertix++)
        {
            vertices[boneIdxShow][idxVertix].SetActive(false);
        }

        boneIdxShow += 1;
        if (boneIdxShow >= nbBones)
            boneIdxShow = 0;
        
        for (int idxVertix = 0 ; idxVertix < vertices[boneIdxShow].Count; idxVertix++)
        {
            vertices[boneIdxShow][idxVertix].SetActive(true);
        }
    }
    
    private void OnDestroy()
    {
        DeleteInstance(_genomeInstance);
        DeleteInstance(_networkInstance);
    }

    [DllImport("machine learning algo")]
    static extern IntPtr LoadGenome();
    
    [DllImport("machine learning algo")]
    static extern IntPtr CreateNeuralNetwork(IntPtr gen);
    
    [DllImport("machine learning algo")]
    static extern void DeleteInstance(IntPtr instance);
    
    [DllImport("machine learning algo")]
    static extern IntPtr GetComputeResult(IntPtr network, float[] positions, int length, int nbBones);

    [DllImport("machine learning algo")]
    static extern int GetOutputFromGenome(IntPtr genome);
}
