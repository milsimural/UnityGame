using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMove : MonoBehaviour
{
    [Header("Admin")]
    [Tooltip("On/Off camera movement")][SerializeField] private bool isScriptOn = false;

    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed")]
    public float maxSpeed = 30f;
    [Tooltip("How quickly camera reaches max speed")]
    public float acceleration = 20f;
    [Tooltip("How quickly camera stops")]
    public float deceleration = 20f;
    [Tooltip("Screen edge trigger zone (0-1)")]
    [Range(0.01f, 0.1f)] public float edgeThreshold = 0.05f;

    [Header("Movement Boundaries")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    [Header("Height Settings")]
    [Tooltip("Fixed camera height")]
    public float cameraHeight = 15f;
    [Tooltip("Should camera stay at fixed height?")]
    public bool fixedHeight = true;

    [Header("Camera Height Settings")]
    public float zoomSpeed = 10f;
    public float minZoom = 40f;
    public float maxZoom = 80f;
    public float currentZoom = 60f;
    public bool isHeightScriptOn = true;

    private Camera mainCamera;

    private Vector3 currentVelocity;
    private bool isMoving;


    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mainCamera.fieldOfView = currentZoom;
    }

    void Update()
    {
        if(isScriptOn) {
            HandleEdgeMovement();
            ApplyBoundaries();
            MaintainHeight();
        }
        if (isHeightScriptOn)
        {
            HandleZoom();
        }
       
    }

    void HandleEdgeMovement()
    {
        Vector3 inputDirection = GetEdgeInputDirection();

        if (inputDirection != Vector3.zero)
        {
            // Ускорение
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                inputDirection * maxSpeed,
                acceleration * Time.deltaTime
            );
            isMoving = true;
        }
        else if (isMoving)
        {
            // Замедление
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );

            if (currentVelocity.magnitude < 0.1f)
            {
                currentVelocity = Vector3.zero;
                isMoving = false;
            }
        }

        transform.Translate(currentVelocity * Time.deltaTime, Space.World);
    }

    Vector3 GetEdgeInputDirection()
    {
        Vector3 direction = Vector3.zero;
        Vector2 mousePos = Input.mousePosition;

        // Проверка границ экрана
        if (mousePos.x < Screen.width * edgeThreshold)
            direction.x = 1; // Левая граница
        else if (mousePos.x > Screen.width * (1 - edgeThreshold))
            direction.x = -1; // Правая граница

        if (mousePos.y < Screen.height * edgeThreshold)
            direction.z = 1; // Нижняя граница
        else if (mousePos.y > Screen.height * (1 - edgeThreshold))
            direction.z = -1; // Верхняя граница

        return direction.normalized;
    }

    void ApplyBoundaries()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    void MaintainHeight()
    {
        if (fixedHeight && !Mathf.Approximately(transform.position.y, cameraHeight))
        {
            transform.position = new Vector3(
                transform.position.x,
                cameraHeight,
                transform.position.z
            );
        }
    }

    // Визуализация границ в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (minX + maxX) * 0.5f,
            cameraHeight,
            (minZ + maxZ) * 0.5f
        );
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }

    // Высота камеры
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        // Определяем точку под курсором мыши в мировых координатах
        Vector3 mouseWorldPosBeforeZoom = mainCamera.ScreenToWorldPoint(GetMousePositionWithDepth());

        // Изменяем зум

            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            mainCamera.fieldOfView = currentZoom;

        // Определяем новую позицию камеры, чтобы точка под курсором осталась той же
        Vector3 mouseWorldPosAfterZoom = mainCamera.ScreenToWorldPoint(GetMousePositionWithDepth());
        Vector3 posDiff = mouseWorldPosBeforeZoom - mouseWorldPosAfterZoom;
        transform.position += posDiff;
    }

    Vector3 GetMousePositionWithDepth()
    {
        Vector3 mousePos = Input.mousePosition;
            mousePos.z = transform.position.z * -1;

        return mousePos;
    }
}