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
    private List<int> _triangles;
    private List<Vector3> _vertices;
    private Mesh _mesh;
    
    // Start is called before the first frame update
    void Start()
    {
        _pointsListSorted = new List<Vector3>();
        _mesh = meshFilter.mesh;
        _triangles = new List<int>();
        _vertices =  new List<Vector3>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.G)) return;
        SortPointList();
        Triangulate();
    }

    private void SortPointList()
    {
        var ordered = from v in sceneManagerScript.GetPointList()
            orderby v.transform.position.x, v.transform.position.y
            select v.transform.position;

        _pointsListSorted = ordered.ToList();
    }

    private void Triangulate()
    {
        var k = 0;
        for (var i = 0; i < 3; i++)
        {
            _vertices.Add(_pointsListSorted[i]);
            _triangles.Add(_vertices.Count - 1);
            k++;
        }

        
        for (var i = k; i < _pointsListSorted.Count; i++)
        {
            for (var h = 0; h < _triangles.Count; h += 3)
            {
                var verticesCanSee = new List<Vector3>();
                for (var l = 0; l < 3; l++)
                {
                    var vertex1 = 0;
                    var vertex2 = 0;
                    switch (l)
                    {
                        case 0:
                            vertex1 = 1;
                            vertex2 = 2;
                            break;
                        case 1:
                            vertex1 = 0;
                            vertex2 = 2;
                            break;
                        default:
                            vertex1 = 0;
                            vertex2 = 1;
                            break;
                    }

                    if (!DoIntersect(_vertices[h + l], _pointsListSorted[i], _vertices[h + vertex1],
                            _vertices[h + vertex2]))
                        verticesCanSee.Add(_vertices[h + l]);
                }

                switch (verticesCanSee.Count)
                {
                    case 2:
                        _vertices.Add(_pointsListSorted[i]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[0]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[1]);
                        _triangles.Add(_vertices.Count - 1);
                        break;
                    case 3:
                        _vertices.Add(_pointsListSorted[i]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[0]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[1]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(_pointsListSorted[i]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[1]);
                        _triangles.Add(_vertices.Count - 1);
                        _vertices.Add(verticesCanSee[2]);
                        _triangles.Add(_vertices.Count - 1);
                        break;
                }
            }
            k++;
        }

        var trianglesArray = _triangles.ToArray();
        DoubleFaceIndices(ref trianglesArray);
        _mesh.triangles = trianglesArray;
        _mesh.vertices = _vertices.ToArray();
        
        /*
         test affichage triangles
         _vertices = new[] { _pointsListSorted[0], _pointsListSorted[1], _pointsListSorted[2] };
        _triangles = new[] { 0, 2, 1 };
        
        */

    }
    
    private static bool OnSegment(Vector3 p, Vector3 q, Vector3 r)
    {
        return q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) &&
               q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y);
    }
    
    private static float Orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        var val = (q.y - p.y) * (r.x - q.x) -
                  (q.x - p.x) * (r.y - q.y);
  
        if (val == 0) return 0; // collinear
  
        return (val > 0)? 1: 2; // clock or counterclock wise
    }
    
    private static bool DoIntersect(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        // Find the four orientations needed for general and
        // special cases
        var o1 = Orientation(p1, q1, p2);
        var o2 = Orientation(p1, q1, q2);
        var o3 = Orientation(p2, q2, p1);
        var o4 = Orientation(p2, q2, q1);
  
        // General case
        if (!o1.Equals(o2) && !o3.Equals(o4))
            return true;
  
        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;
  
        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;
  
        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;
  
        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        return o4 == 0 && OnSegment(p2, q1, q2);
    }
    
}
