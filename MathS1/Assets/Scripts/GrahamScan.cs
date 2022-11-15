using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GrahamScan : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManagerScript;
    private GameObject _centerPoint;
    private List<GameObject> _pointsListSorted;
    private GameObject _lineRenderer;
    

    private void Start()
    {
        _pointsListSorted = new List<GameObject>();
        _lineRenderer = new GameObject();
        var lineRendererComponent =_lineRenderer.AddComponent<LineRenderer>();
        lineRendererComponent.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererComponent.material.color = Color.cyan;
        lineRendererComponent.widthMultiplier = 0.1f;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Clear();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
            InitCenterPoint();
            SortPointsByLowerAngle();
            DrawLineBetweenPoints(_pointsListSorted);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Clear();
            InitCenterPoint();
            SortPointsByLowerAngle();
            ClearList(_pointsListSorted);
            DrawLineBetweenPoints(_pointsListSorted);
        }
    }

    public float test(Vector3 common, Vector3 to, Vector3 from)
    {
        Vector3 side1 = to-common;
        Vector3 side2 = from-common;
        return Vector3.Angle(side1, side2);
    }
    
    private void ClearList(List<GameObject> points)
    {
        var idx = 1;
        var lastPointChecked = points[0];
        var blockLoop = 0;

        while (idx < points.Count + 1 && blockLoop < 100)
        {
            var pointToControl = points[idx % points.Count];
            var pointNextToControl = points[(idx + 1) % points.Count];
            
            var testAngle = test(pointToControl.transform.position, lastPointChecked.transform.position, pointNextToControl.transform.position);

            Debug.Log("angle");
            Debug.Log(testAngle);
            
            /*Debug.Log("Count");
            Debug.Log(points.Count);
            Debug.Log("idx");
            Debug.Log(idx);*/
            Debug.Log("blockloop");
            Debug.Log(blockLoop);
            
            if (testAngle < 0)
            {
                points.Remove(pointToControl);
            }
            else
            {
                idx++;
                lastPointChecked = pointToControl;
            }
            
            blockLoop += 1;
        }
        
    }
    
    private void DrawLineBetweenPoints(List<GameObject> list)
    {
        var lineRendererComponent = _lineRenderer.GetComponent<LineRenderer>();
        lineRendererComponent.positionCount = list.Count;
        
        for (int i = 0; i < list.Count; i++)
        {
            lineRendererComponent.SetPosition(i, list[i].transform.position);
        }
    }
    
    private float AngleBetweenVectors(Vector3 from, Vector3 to, Vector3 normal)
    {
        /*return Vector3.Angle(from, to);*/

        float angle = Vector3.Angle(from,to);
        float sign = Mathf.Sign(Vector3.Dot(normal,Vector3.Cross(from,to)));

        // angle in [-179,180]
        float signed_angle = angle * sign;

        // angle in [0,360] (not used but included here for completeness)
        //float angle360 =  (signed_angle + 180) % 360;

        return signed_angle;
    }

    private void SortPointsByLowerAngle()
    {
        var pointsList = new List<GameObject>(sceneManagerScript.GetPointList());

        while (pointsList.Count > 0)
        {
            var lowerAngle = AngleBetweenVectors(_centerPoint.transform.position, pointsList[0].transform.position, Vector3.up);
            lowerAngle += 180;
            
            var pointWithLowerAngle = pointsList[0];
            
            foreach (var point in pointsList)
            {
                var newAngle = AngleBetweenVectors(_centerPoint.transform.position, point.transform.position,  Vector3.up);
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
        _lineRenderer.GetComponent<LineRenderer>().positionCount = 0;
        _pointsListSorted.Clear();
        Destroy(_centerPoint);
    }
}
