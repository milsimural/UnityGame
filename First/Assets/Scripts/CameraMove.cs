using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

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
    private Camera mainCamera;
    [SerializeField] private float _yPos = 15f;
    [SerializeField] private float _fov = 60f;
    [SerializeField] private int _zoomPreset = 3;

    // ��������� ���������� ��� ����������� �� ������
    private Vector3 currentVelocity;
    private bool isMoving;

    // ��������� ��������� ��� ����
    private bool _isBlockedZoom = false;
    [SerializeField] private float _transitionSpeed = 20f;
    [SerializeField] private float _threshold = 0.3f;
    

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if(isScriptOn) {
            HandleEdgeMovement();
            ApplyBoundaries();
        }
        HandleZoom();
       
    }

    void HandleEdgeMovement()
    {
        Vector3 inputDirection = GetEdgeInputDirection();

        if (inputDirection != Vector3.zero)
        {
            // ���������
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                inputDirection * maxSpeed,
                acceleration * Time.deltaTime
            );
            isMoving = true;
        }
        else if (isMoving)
        {
            // ����������
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

        // �������� ������ ������
        if (mousePos.x < Screen.width * edgeThreshold)
            direction.x = 1; // ����� �������
        else if (mousePos.x > Screen.width * (1 - edgeThreshold))
            direction.x = -1; // ������ �������

        if (mousePos.y < Screen.height * edgeThreshold)
            direction.z = 1; // ������ �������
        else if (mousePos.y > Screen.height * (1 - edgeThreshold))
            direction.z = -1; // ������� �������

        return direction.normalized;
    }

    void ApplyBoundaries()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    // ������ ������
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // ������������ ���� ������ ���� �� � �������� ��������
        if (!_isBlockedZoom && scroll != 0)
        {
            _isBlockedZoom = true;

            // ���������� ����������� �������
            int scrollDirection = scroll > 0 ? -1 : 1;
            _zoomPreset = Mathf.Clamp(_zoomPreset + scrollDirection, 0, 6);

            switch (_zoomPreset)
            {
                case 0: _yPos = 0; _fov = 60; break;
                case 1: _yPos = 05; _fov = 60; break;
                case 2: _yPos = 05; _fov = 60; break;
                case 3: _yPos = 15; _fov = 60; break;
                case 4: _yPos = 25; _fov = 60; break;
                case 5: _yPos = 35; _fov = 60; break;
                case 6: _yPos = 45; _fov = 60; break;
            }
        }

        // ������ ������������ ��������, ���� ��� �������
        if (_isBlockedZoom)
        {
            // ������� ��������� FOV
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, _fov, Time.deltaTime * _transitionSpeed);

            // ������� ��������� �������
            Vector3 currentCameraPos = transform.position;
            currentCameraPos.y = Mathf.Lerp(currentCameraPos.y, _yPos, Time.deltaTime * _transitionSpeed);
            transform.position = currentCameraPos;

            // ��������� ���������� �������� � ��������
            if (Mathf.Abs(mainCamera.fieldOfView - _fov) < _threshold &&
                Mathf.Abs(currentCameraPos.y - _yPos) < _threshold)
            {
                // ������������ ��������
                mainCamera.fieldOfView = _fov;
                currentCameraPos.y = _yPos;
                transform.position = currentCameraPos;

                _isBlockedZoom = false;
            }
        }
    }

    Vector3 GetMousePositionWithDepth()
    {
        Vector3 mousePos = Input.mousePosition;
            mousePos.z = transform.position.z * -1;

        return mousePos;
    }

    void MoveCameraLocalTowardsMouse()
    {
        // 1. �������� ������� ���� � ����� ������
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

        // 2. ������ �� ������ � �������
        Vector3 direction = mouseScreenPos - screenCenter;

        // 3. ���� ����� ������� ������ � ������ - �� �������
        if (direction.magnitude < 10f) return;

        // 4. ����������� ����������� (�������� � ��������� [-1, 1])
        Vector2 normalizedDirection = direction.normalized;

        // 5. �������� ��������� ��� ������ (��� ������� �� Y)
        Vector3 cameraRight = transform.right;
        Vector3 cameraForward = transform.forward;
        cameraRight.y = 0;
        cameraForward.y = 0;
        cameraRight.Normalize();
        cameraForward.Normalize();

        // 6. ��������� ����������� �������� (�� ��������� ����)
        Vector3 moveDirection = (cameraRight * normalizedDirection.x) + (cameraForward * normalizedDirection.y);

        // 7. ��������� �������� (�������� ���������� �� ������ �� ����)
        float moveDistance = direction.magnitude * 0.5f;

        // 8. ��������� �������� ������ (��� Time.deltaTime)
        transform.position += moveDirection * moveDistance;
    }
}