using System.Collections.Generic;
using UnityEngine;

public class GrahamScan : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManagerScript;
    private GameObject _centerPoint;
    public List<GameObject> _pointsListSorted;

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
            UseScan();
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
    }

    public void UseScan()
    {
        _pointsListSorted.Clear();
        InitCenterPoint();
        SortPointsByLowerAngle();
        DrawLineBetweenPoints(_pointsListSorted, Color.cyan);
        Destroy(_centerPoint);
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
}
