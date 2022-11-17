using System.Collections.Generic;
using UnityEngine;

public class JarvisScript : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManagerScript;
    [SerializeField] private GameObject Point;
    private List<GameObject> _listPointsRendering;
    private List<GameObject> _listPoints;

    private void Start()
    {
        _listPoints = new List<GameObject>();
        _listPointsRendering = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            BuildJarvis();
        }
    }
    
    private float GetAngle(Vector3 from, Vector3 common, Vector3 to)
    {
        Vector3 side1 = to-common;
        Vector3 side2 = from-common;
        var angle = Vector3.SignedAngle(side2, side1, Vector3.back);
        return angle;
    }

    private void BuildJarvis()
    {
        _listPoints = sceneManagerScript.GetPointList();
        var lefestpoint  = GetLeftestPoint();
        var vectorPoint = GetVectorPoint(lefestpoint);
        CreateJarvis(lefestpoint, vectorPoint);
        DrawLineBetweenPoints(_listPointsRendering);
        _listPointsRendering.Clear();
        Destroy(vectorPoint);
    }

    private void CreateJarvis(GameObject lefestpoint, GameObject vectorPoint)
    {
        _listPointsRendering.Add(lefestpoint);
        var fromPoint = lefestpoint;
        var commonPoint = vectorPoint;
        var PointToAdd = _listPoints[1];

        if (PointToAdd.GetInstanceID() == lefestpoint.GetInstanceID())
        {
            PointToAdd = _listPoints[0];
        }

        while (PointToAdd.GetInstanceID() != lefestpoint.GetInstanceID())
        {
            var lowestAngle = 360.0f;
            foreach(var point in _listPoints)
            {
                if (point.GetInstanceID() != fromPoint.GetInstanceID())
                {
                    var angle = GetAngle(fromPoint.transform.position, commonPoint.transform.position, point.transform.position);

                    if (angle < lowestAngle)
                    {
                        lowestAngle = angle;
                        PointToAdd = point;
                    }
                }
            }
            fromPoint = _listPointsRendering[_listPointsRendering.Count - 1];
            _listPointsRendering.Add(PointToAdd);
            commonPoint = PointToAdd;
        }
    }
    
    private GameObject GetLeftestPoint()
    {
        var lefestPoint = _listPoints[0];

        foreach (var point in _listPoints)
        {
            if (point.transform.position.z < lefestPoint.transform.position.z)
                lefestPoint = point;
            
            if (point.transform.position.z - lefestPoint.transform.position.z < 0.0001)
                if (point.transform.position.x > lefestPoint.transform.position.x)
                    lefestPoint = point;
        }

        return lefestPoint;
    }

    private GameObject GetVectorPoint(GameObject lefestPoint)
    {
        var vectorPoint = Instantiate(Point);
        vectorPoint.transform.position = new Vector3(lefestPoint.transform.position.x, lefestPoint.transform.position.y - 1, lefestPoint.transform.position.z);
        vectorPoint.SetActive(false);
        return vectorPoint;
    }
    
    private void DrawLineBetweenPoints(List<GameObject> list)
    {
        var lineRendererComponent = sceneManagerScript.GetLineRenderer().GetComponent<LineRenderer>();
        lineRendererComponent.material.color = Color.red;
        lineRendererComponent.positionCount = 0;
        lineRendererComponent.positionCount = list.Count + 1;

        for (int i = 0; i < list.Count + 1; i++)
        {
            lineRendererComponent.SetPosition(i, list[(i % list.Count)].transform.position);
        }
    }
}
