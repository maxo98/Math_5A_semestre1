using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoneScript : MonoBehaviour
{
    [SerializeField] private List<Vector3> vertex;
    [SerializeField] private GameObject bone;
    [SerializeField] private Material vertexMat;
    [SerializeField] private Material boneMat;
    
    private List<Vector3> _projections;
    private List<float> _projectionsValue;
    private Vector3 _barycenter;
    private Matrix4x4 _covarianceMatrix;
    private Vector3 _principalVector;
    private float _principalValue;
    public Vector3 minimumExtreme;
    public Vector3 maximumExtreme;

    private void Start()
    {
        vertex = new List<Vector3>();
        for (var i = 0; i < 20; i++)
        {
            vertex.Add(new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f)));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GenerateBone();
        }
    }

    private void GenerateBone()
    {
        GetBaryCenter();
        VertexToOrigin();
        GetCovarianceMatrix();
        GetPrincipalComponents();
        GetProjections();
        GetExtremities();
        ExtremitiesReplacement();
        DrawBone();
    }
    
    private void GetBaryCenter()
    {
        var barycenter = new Vector3(0, 0, 0);
        foreach (var vert in vertex)
        {
            barycenter.x += vert.x;
            barycenter.y += vert.y;
            barycenter.z += vert.z;
        }

        barycenter /= vertex.Count;
        _barycenter = barycenter;
    }

    private void VertexToOrigin()
    {
        var og = new Vector3(0, 0, 0) - _barycenter;
        for (var i = 0; i < vertex.Count; i++)
        {
            vertex[i] += og;
        }
    }

    private void GetCovarianceMatrix()
    {
        _covarianceMatrix = new Matrix4x4();
        foreach (var ver in vertex)
        {
            _covarianceMatrix[0, 0] += ver.x * ver.x;
            _covarianceMatrix[0, 1] += ver.x * ver.y;
            _covarianceMatrix[0, 2] += ver.x * ver.z;
            _covarianceMatrix[1, 1] += ver.y * ver.y;
            _covarianceMatrix[1, 2] += ver.y * ver.z;
            _covarianceMatrix[2, 2] += ver.z * ver.z;
        }

        _covarianceMatrix[1, 0] = _covarianceMatrix[0, 1];
        _covarianceMatrix[0, 2] = _covarianceMatrix[2, 0];
        _covarianceMatrix[2, 1] = _covarianceMatrix[1, 2];
    }

    private void GetPrincipalComponents()
    {
        var v0 = new Vector3(0, 0, 1);
        var vectorMk = _covarianceMatrix.MultiplyVector(v0);
        var lambda = 0f; 
        for (var k = 1; k < 100; k++)
        {
            lambda = Math.Max(Math.Abs(vectorMk.x), Math.Max(Math.Abs(vectorMk.y), Math.Abs(vectorMk.z)));
            vectorMk = (1 / lambda) * _covarianceMatrix.MultiplyVector(vectorMk);
        }

        _principalVector = vectorMk.normalized;
        _principalValue = lambda;
    }

    private void GetProjections()
    {
        _projections = new List<Vector3>();
        _projectionsValue = new List<float>();
        maximumExtreme = new Vector3();
        minimumExtreme = new Vector3();
        for (var i = 0; i < vertex.Count; i++)
        {
            var oAi = new Vector3() + vertex[i];
            _projections.Add((Vector3.Dot(oAi, _principalVector) / _principalVector.magnitude) * _principalVector);
            _projectionsValue.Add(Vector3.Dot(oAi, _principalVector) / _principalVector.magnitude);
        }
    }

    private void GetExtremities()
    {
        for (var i = 0; i < _projections.Count; i++)
        {
            if (_projectionsValue[i] < 0)
            {
                if (_projections[i].magnitude > minimumExtreme.magnitude)
                {
                    minimumExtreme = _projections[i];
                }
            }
            else if (_projectionsValue[i] > 0)
            {
                if (_projections[i].magnitude > maximumExtreme.magnitude)
                {
                    maximumExtreme = _projections[i];
                }
            }
        }
    }

    private void ExtremitiesReplacement()
    {
        maximumExtreme += _barycenter;
        minimumExtreme += _barycenter;
    }

    private void DrawBone()
    {
        // foreach (var variable in vertex)
        // {
        //     var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //     go.GetComponent<MeshRenderer>().material = vertexMat;
        //     go.transform.position = variable;
        // }
        var min = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var max = GameObject.CreatePrimitive(PrimitiveType.Cube);
        min.transform.position = minimumExtreme;
        max.transform.position = maximumExtreme;
        var instantiatedBone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        instantiatedBone.transform.position = new Vector3(minimumExtreme.x + maximumExtreme.x, minimumExtreme.y + maximumExtreme.y, minimumExtreme.z + maximumExtreme.z) / 2;
        var scaleY = (minimumExtreme - maximumExtreme).magnitude / 2;
        instantiatedBone.transform.localScale = new Vector3(1, scaleY, 1);
        instantiatedBone.transform.LookAt((minimumExtreme - maximumExtreme));
        instantiatedBone.transform.forward = instantiatedBone.transform.up;
        instantiatedBone.GetComponent<MeshRenderer>().material = boneMat;
    }
}
