using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Delaunay;


public class TriangulationIncremental : MonoBehaviour
{
    private const int MaxTriangles = 10000000;
    private const int MaxVertices = 10000000;
    [SerializeField] private SceneManager sceneManagerScript;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    private List<Vector3> _pointsListSorted;
    private List<Vector3> _currentPoints;
    private int[] _triangles;
    private Vector3[] _vertices;
    private Mesh _mesh;
    
    // Start is called before the first frame update
    void Start()
    {
        _pointsListSorted = new List<Vector3>();
        _mesh = meshFilter.mesh;
        _triangles = new int[MaxTriangles];
        _vertices = new Vector3[MaxVertices];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SortPointList();
            Triangulate();
        }
    }

    private void SortPointList()
    {
        var ordered = from v in sceneManagerScript.GetPointList()
            orderby v.transform.position.y, v.transform.position.y
            select v.transform.position;

        _pointsListSorted = ordered.ToList();
    }

    private void Triangulate()
    {
        var k = 0;
        var baseY = _pointsListSorted[0].y;
        while (Math.Abs(_pointsListSorted[k].y - baseY) < 0.0001)
        {
            k++;
        }

        for (var i = 0; i < k; i++)
        {
            _vertices[i] = _pointsListSorted[i];
            _vertices[i + 1] = _pointsListSorted[i + 1];
            _vertices[k] = _pointsListSorted[k];
            _triangles[i] = i;
        }
        
        DoubleFaceIndices(ref _triangles);
        _mesh.triangles = _triangles;
        _mesh.vertices = _vertices;
        
        /*
         test affichage triangles
         _vertices = new[] { _pointsListSorted[0], _pointsListSorted[1], _pointsListSorted[2] };
        _triangles = new[] { 0, 2, 1 };
        
        */

    }
}
