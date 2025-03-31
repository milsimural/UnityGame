using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ZoomToMouseCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("Current zoom distance")]
    public float currentZoom = 20f;
    [Tooltip("Minimum zoom distance")]
    public float minZoom = 5f;
    [Tooltip("Maximum zoom distance")]
    public float maxZoom = 50f;
    [Tooltip("Zoom speed")]
    public float zoomSpeed = 10f;
    [Tooltip("Zoom acceleration")]
    public float zoomAcceleration = 2f;
    [Tooltip("How fast zoom returns to normal speed after input")]
    public float zoomDeceleration = 4f;

    [Header("Advanced")]
    [Tooltip("Should zoom be smoothed?")]
    public bool smoothZoom = true;
    [Tooltip("Smoothing amount if enabled")]
    [Range(0.1f, 1f)] public float smoothFactor = 0.3f;

    private float targetZoom;
    private float currentZoomVelocity;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetZoom = currentZoom;
    }

    void Update()
    {
        HandleZoomInput();
        ApplyZoom();
    }

    void HandleZoomInput()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // ”скорение при активном зуме
            float speedMultiplier = 1f + Mathf.Abs(scrollInput) * zoomAcceleration;
            targetZoom -= scrollInput * zoomSpeed * speedMultiplier;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            // —брасываем скорость дл€ плавности
            if (smoothZoom) currentZoomVelocity = 0;
        }
    }

    void ApplyZoom()
    {
        if (smoothZoom)
        {
            // ѕлавное приближение/удаление
            currentZoom = Mathf.SmoothDamp(
                currentZoom,
                targetZoom,
                ref currentZoomVelocity,
                smoothFactor
            );
        }
        else
        {
            currentZoom = targetZoom;
        }

        // ѕолучаем точку под курсором в мировых координатах
        Ray ray2 = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray2, out RaycastHit hit))
        {
            // ¬ычисл€ем новую позицию камеры относительно точки под курсором
            Vector3 zoomDirection = (transform.position - hit.point).normalized;
            transform.position = hit.point + zoomDirection * currentZoom;
        }
        else
        {
            // ≈сли луч никуда не попал, просто измен€ем рассто€ние
            transform.position += transform.forward * (currentZoom - cam.transform.position.magnitude);
        }
    }

    void OnValidate()
    {
        // ќбновл€ем значени€ при изменении в инспекторе
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    // ¬изуализаци€ зума в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, currentZoom * 0.1f);
    }
}