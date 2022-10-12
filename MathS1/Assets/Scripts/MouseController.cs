using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private GameObject point;
    [SerializeField] private SceneManager sceneManagerScript;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CastRayFromMouseClick();
    }

    private void CastRayFromMouseClick()
    {
        if (Camera.main == null) return;
        
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var layerMask = LayerMask.GetMask("Background");
        
        if (!Physics.Raycast(ray, out var hit, 100, layerMask)) return;

        var newPoint = GameObject.Instantiate(point);
        newPoint.transform.position = hit.point;
        sceneManagerScript.AddPoint(newPoint);
    }
}
