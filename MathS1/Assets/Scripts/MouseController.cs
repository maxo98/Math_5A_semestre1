using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private GameObject point;
    [SerializeField] private SceneManager sceneManagerScript;
    [SerializeField] private GameObject background;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            InstantiateSphereFromRayCasting();
    }

    private void InstantiateSphereFromRayCasting()
    {
        if (Camera.main == null) return;
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("Background");
        
        if (!Physics.Raycast(ray, out var hit, 100, layerMask)) return;

        var newPoint = GameObject.Instantiate(point);
        newPoint.transform.position = new Vector3(hit.point.x,background.transform.position.y,hit.point.z);
        sceneManagerScript.AddPoint(newPoint);
    }
}
