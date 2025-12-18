using UnityEngine;

public class PrefabCameraAssigner : MonoBehaviour
{
    private Camera _mainCamera;
    private Canvas _canvas;

    private bool _isCanvasAssigned = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        if (_isCanvasAssigned) return;


        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("[PrefabCameraAssigner] Main camera not found!");
            return;
        }
        if (_canvas == null)
        {
            Debug.LogError("[PrefabCameraAssigner] Canvas component not found!");
            return;
        }

        // Assign the main camera to the canvas
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = _mainCamera;
        _isCanvasAssigned = true;
    }
}
