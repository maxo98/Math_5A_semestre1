using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pointList;
    
    private void Start()
    {
        pointList = new List<GameObject>();
    }
    
    public void AddPoint(GameObject newPoint)
    {
        pointList.Add(newPoint);
    }
}
