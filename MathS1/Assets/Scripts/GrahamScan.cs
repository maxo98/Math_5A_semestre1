using System.Collections.Generic;
using UnityEngine;

public class GrahamScan : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManagerScript;
    private GameObject _centerPoint;
    private List<GameObject> _pointsListSorted;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Transform meshTransform;

    private void Start()
    {
        _pointsListSorted = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Clear();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _pointsListSorted.Clear();
            InitCenterPoint();
            SortPointsByLowerAngle();
            DrawLineBetweenPoints(_pointsListSorted, Color.cyan);
            Destroy(_centerPoint);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            _pointsListSorted.Clear();
            InitCenterPoint();
            SortPointsByLowerAngle();
            ClearList(_pointsListSorted);
            DrawLineBetweenPoints(_pointsListSorted, Color.blue);

            Destroy(_centerPoint);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
                    LinkedList<Vector3> _LinkedPointsList = new LinkedList<Vector3>();

            foreach (var gameObject in _pointsListSorted)
            {
                _LinkedPointsList.AddLast(gameObject.transform.position);
            }

            Debug.Log(_pointsListSorted.Count);

            Vector3[] triangles;
            
            JoinPointsAlongPlane(_LinkedPointsList, out triangles);

            int[] indices = new int[triangles.Length];

            for(int i = 0; i < triangles.Length; i++)
            {
                indices[i] = i;
            }

            

            Delaunay.DoubleFaceIndices(ref indices);

            meshFilter.mesh.triangles = new int[0];
            meshFilter.mesh.vertices = triangles;
            meshFilter.mesh.triangles = indices;
        }
    }

    private float GetAngle(Vector3 from, Vector3 common, Vector3 to)
    {
        Vector3 side1 = to-common;
        Vector3 side2 = from-common;
        var angle = Vector3.SignedAngle(side2, side1, Vector3.back);
        return angle;
    }
    
    private void ClearList(List<GameObject> points)
    {
        var idx = 1;
        var lastPointChecked = points[0];

        while (idx < points.Count + 1)
        {
            var pointToControl = points[idx % points.Count];
            var pointNextToControl = points[(idx + 1) % points.Count];
            
            var testAngle = GetAngle(lastPointChecked.transform.position,pointToControl.transform.position, pointNextToControl.transform.position);

            if (testAngle >= 0)
            {
                points.Remove(pointToControl);
                lastPointChecked = points[0];
                idx = 1;
            }
            else
            {
                idx++;
                lastPointChecked = pointToControl;
            }
        }
        
    }

    private float AngleBetweenVectorsAndNormal(Vector3 from, Vector3 to, Vector3 normal)
    {
        float angle = Vector3.Angle(from,to);
        float sign = Mathf.Sign(Vector3.Dot(normal,Vector3.Cross(from,to)));
        float signed_angle = angle * sign;
        return signed_angle;
    }

    private void DrawLineBetweenPoints(List<GameObject> list, Color color)
    {
        var lineRendererComponent = sceneManagerScript.GetLineRenderer().GetComponent<LineRenderer>();
        lineRendererComponent.material.color = color;
        lineRendererComponent.positionCount = 0;
        lineRendererComponent.positionCount = list.Count + 1;
        
        for (int i = 0; i < list.Count + 1; i++)
        {
            lineRendererComponent.SetPosition(i, list[(i % list.Count)].transform.position);
        }
    }

    private void SortPointsByLowerAngle()
    {
        var pointsList = new List<GameObject>(sceneManagerScript.GetPointList());

        while (pointsList.Count > 0)
        {
            var lowerAngle = AngleBetweenVectorsAndNormal(_centerPoint.transform.position, pointsList[0].transform.position, Vector3.back);
            lowerAngle += 180;
            
            var pointWithLowerAngle = pointsList[0];
            
            foreach (var point in pointsList)
            {
                var newAngle = AngleBetweenVectorsAndNormal(_centerPoint.transform.position, point.transform.position,  Vector3.back);
                newAngle += 180;
                
                if (newAngle < lowerAngle)
                {
                    lowerAngle = newAngle;
                    pointWithLowerAngle = point;
                }
            }
            
            _pointsListSorted.Add(pointWithLowerAngle);
            pointsList.Remove(pointWithLowerAngle);
        }
    }

    private void InitCenterPoint()
    {
        var points = GetCenterPoint();
        CreateNewCenterPoint(points);
    }

    private void CreateNewCenterPoint(List<GameObject> points)
    {
        var positionZ = (points[0].transform.position.z + points[1].transform.position.z) / 2;
        var positionY = (points[2].transform.position.y + points[3].transform.position.y) / 2;

        _centerPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var rendererComponent = _centerPoint.GetComponent<Renderer>();
        rendererComponent.material.SetColor("_Color", Color.blue);
        _centerPoint.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        _centerPoint.transform.position = new Vector3(points[0].transform.position.x, positionY, positionZ);
    }
    
    private List<GameObject> GetCenterPoint()
    {
        var points = sceneManagerScript.GetPointList();
        var gameObjects = new List<GameObject>(4) { points[0], points[0], points[0], points[0] };
        points.ForEach(item => UpdateCenterBound(item, gameObjects));
        return gameObjects;
    }

    private void UpdateCenterBound(GameObject point, List<GameObject> pointsBounders)
    {
        // lower width
        if (point.transform.position.z < pointsBounders[0].transform.position.z)
            pointsBounders[0] = point;
        
        // higher width
        if (point.transform.position.z > pointsBounders[1].transform.position.z)
            pointsBounders[1] = point;
        
        // lower high
        if (point.transform.position.y < pointsBounders[2].transform.position.y)
            pointsBounders[2] = point;
        
        // higher high
        if (point.transform.position.y > pointsBounders[3].transform.position.y)
            pointsBounders[3] = point;
    }

    private void Clear()
    {
        sceneManagerScript.Clear();
        _pointsListSorted.Clear();
    }

    /// Join the points along the plane to the halfway point
    private void JoinPointsAlongPlane(in LinkedList<Vector3> face, out Vector3[] triangles)
    {
        //Get the 2d Vectors along the plane sorted in a clockwise polygon point list
        //And store the indexes of there 3d equivalent in a map
        Dictionary<Vector2, Vector3> vertexMap;

        LinkedList<Vector2> polygon;
        ProjectsOnPlane(in face, out polygon, out vertexMap);

        //Decompose the polygon into y-monotones polygons
        //O(n*Log(n))
        LinkedList<LinkedList<Vector2>> monotonePolygons;
        PolygonTriangulation.DecomposeToMonotone(polygon, out monotonePolygons);

        //triangulate the mononotone polygon 
        //O(n)
        LinkedList<Vector2> trianglesVec2 = new LinkedList<Vector2>();
        LinkedListNode<LinkedList<Vector2>> monotonePolygonsListNode;

        for(monotonePolygonsListNode = monotonePolygons.First; monotonePolygonsListNode != null; monotonePolygonsListNode = monotonePolygonsListNode.Next)
        {
            PolygonTriangulation.TriangulateMonotonePolygon(monotonePolygonsListNode.Value, ref trianglesVec2);
        }

        //Turn the 2d triangles into 3d triangles
        triangles = new Vector3[trianglesVec2.Count];
        LinkedListNode<Vector2> trianglesVec2Node;
        int cpt = 0;

        for(trianglesVec2Node = trianglesVec2.First; trianglesVec2Node != null; trianglesVec2Node = trianglesVec2Node.Next, cpt++)
        {
            triangles[cpt] = vertexMap[trianglesVec2Node.Value];
        }
    }

    //Creates a non crossing polygon from the points along the plane
    private void ProjectsOnPlane(in LinkedList<Vector3> face, out LinkedList<Vector2> polygon, out Dictionary<Vector2, Vector3> vertexMap)
    {
        if(face.Count < 3)
        {
            polygon = new LinkedList<Vector2>();
            vertexMap = new Dictionary<Vector2, Vector3>();

            return;
        }

        //Projects the 3d vectors on the 2d plane
        polygon = new LinkedList<Vector2>();
        Vector3 xAxis = Vector3.one;
        Vector3 yAxis = Vector3.one;
        Vector3 planeOrigin = Vector3.zero;//Any point on the plane will do.
        Vector3 zAxis = new Plane(face.First.Value, face.First.Next.Value, face.First.Next.Next.Value).normal;
        
        Vector3.OrthoNormalize(ref zAxis, ref xAxis, ref yAxis);

        vertexMap = new Dictionary<Vector2, Vector3>();

        LinkedListNode<Vector3> faceNode;
        Vector3 pointZero = face.First.Value;

        for(faceNode = face.First; faceNode != null; faceNode = faceNode.Next)
        {
            Vector3 planePos = faceNode.Value - pointZero;
            Vector2 vec2 = new Vector2(Vector3.Dot(planePos, xAxis), Vector3.Dot(planePos, yAxis));

            //If there's a point duplicate ignore it and everything will be alright, right ?
            //Probably happens because of the doublesided face, should be removed in the future
            if(vertexMap.ContainsKey(vec2) == false)
            {
                polygon.AddLast(vec2);
                vertexMap.Add(vec2, faceNode.Value);
            }
        }
    }
}
