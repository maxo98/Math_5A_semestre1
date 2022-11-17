using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pointList;
    
    private GameObject _lineRenderer;
    private void Start()
    {
        pointList = new List<GameObject>();
        _lineRenderer = new GameObject();
        var lineRendererComponent =_lineRenderer.AddComponent<LineRenderer>();
        lineRendererComponent.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererComponent.widthMultiplier = 0.1f;
    }
    
    public void AddPoint(GameObject newPoint)
    {
        pointList.Add(newPoint);
    }

    public List<GameObject> GetPointList()
    {
        return pointList;
    }

    public GameObject GetLineRenderer()
    {
        return _lineRenderer;
    }

    public void Clear()
    {
        ClearLineRenderer();
        
        foreach (var point in pointList)
        {
            Destroy(point);
        }
        
        pointList.Clear();
    }

    public void ClearLineRenderer()
    {
        _lineRenderer.GetComponent<LineRenderer>().positionCount = 0;
    }
}
