using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using RangeAttribute = UnityEngine.RangeAttribute;

public class ManagerBackup : MonoBehaviour
{
    public SelectableObj Hovered;
    public SelectableObj Selected;
    public bool isHardcore = false;

    private bool _isDragging = false;
    private Vector3 _startPosition;
    private LineRenderer _lineRenderer;

    [SerializeField] private float dragSpeed = 75f; // Скорость перемещения
    private Vector3 dragOrigin; // Точка, от которой начали тянуть
    private bool isDraggingCam = false;

    [Header("Camera")]
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
    [SerializeField] private float _xPos = 0;
    [SerializeField] private float _yPos = 15f;
    [SerializeField] private float _zPos = 0;
    [SerializeField] private float _fov = 60f;
    [SerializeField] private int _zoomPreset = 2;
    // Служебные переменные для перемещения от границ
    private Vector3 currentVelocity;
    private bool isMoving;

    // Служебные переменны для зума
    private bool _isBlockedZoom = false;
    [SerializeField] private float _transitionSpeed = 20f;
    [SerializeField] private float _threshold = 0.3f;

    [Header("Drag Settings")]
    [SerializeField] private float dragPlaneHeight = 0f; // Высота плоскости перетаскивания
    [SerializeField] private float dragSpeedMultiplier = 2f; // Множитель скорости при увеличении FOV
    private Plane dragPlane; // Плоскость для рейкастинга при перетаскивании

    void Start()
    {
        mainCamera = Camera.main;
        mainCamera.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        Vector3 cameraPos = mainCamera.transform.position;
        cameraPos.x = _xPos;
        cameraPos.y = _yPos;
        cameraPos.z = _zPos;
        mainCamera.transform.position = cameraPos;

        // Инициализируем плоскость для перетаскивания
        dragPlane = new Plane(Vector3.up, new Vector3(0, dragPlaneHeight, 0));
    }

    
    void Update()
    {
        #region Select
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Логика выделения обьекта куда мышкой навелись
        if (Physics.Raycast(ray, out hit) )
        {
            if(hit.collider.GetComponent<SelectableCol>())
            {
                SelectableObj hitSelectable = hit.collider.GetComponent<SelectableCol>().MySelectableObj;
                if(Hovered)
                {
                    if(Hovered != hitSelectable)
                    {
                        Hovered.OnUnHover();
                        Hovered = hitSelectable;
                        Hovered.OnHover();
                    } 
                } else
                {
                    Hovered = hitSelectable;
                    Hovered.OnHover();
                }

            } else
            {
                if(Hovered)
                {
                    UnHoverCurrent();
                    if(_isDragging && !Input.GetMouseButton(0)) _isDragging = false;
                }
            }
        } else
        {
            UnHoverCurrent();
        }

        if (Hovered && Input.GetMouseButtonDown(0))
        {
                if(Selected) UnselectAll();
                Select(Hovered);
                _isDragging = false;
        }

        if (Hovered && Selected && Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lineRenderer = Selected.GetComponent<LineRenderer>();
            _startPosition = Selected.transform.position;
            _lineRenderer.SetPosition(0, _startPosition);
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            if(hit.collider)
            {
                Vector3 endPosition = hit.point;
                _lineRenderer.SetPosition(1, endPosition);
                if (GetLineTotalLength(_lineRenderer) > 0.7f)
                {
                    _lineRenderer.enabled = true;
                }
                else
                {
                    _lineRenderer.enabled = false;
                }
            }
            
        }

        if (_isDragging && Input.GetMouseButtonUp(0))
        {
            if(!hit.collider) {
                _isDragging = false;
                _lineRenderer.enabled = false;
            }
            else if (hit.collider.tag == "Sea") {
                _isDragging = false;
                _lineRenderer.enabled = false;
                Selected.WhenClickOnGround(hit.point);
            }       
        } 


        // Начало перетаскивания (зажали ЛКМ)
        if (!Hovered && !_isDragging && Input.GetMouseButtonDown(0))
             {
            Debug.Log("Start");
            dragOrigin = Input.mousePosition;
                 isDraggingCam = true;
             }

                // Завершение перетаскивания (отпустили ЛКМ)
        if (!_isDragging && Input.GetMouseButtonUp(0))
             {
            Debug.Log("End");     
            isDraggingCam = false;
             }

        //if (isDraggingCam)
        //{
        //    // Разница между текущей позицией и начальной точкой
        //    Vector3 difference = dragOrigin - Input.mousePosition;

        //    // Создаем вектор движения только по XZ плоскости
        //    Vector3 movement = new Vector3(difference.x, 0, difference.y) * dragSpeed * Time.deltaTime;

        //    // Перемещаем камеру в локальных координатах (учитывая ее поворот)
        //    Vector3 localMovement = mainCamera.transform.TransformDirection(movement);
        //    localMovement.y = 0; // Обнуляем вертикальную составляющую

        //    // Вычисляем новую позицию с фиксированной высотой
        //    Vector3 newPosition = mainCamera.transform.position + localMovement;
        //    newPosition.y = _yPos; // Фиксируем высоту

        //    // Применяем ограничения границ
        //    newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        //    newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        //    mainCamera.transform.position = newPosition;

        //    // Обновляем точку отсчёта для плавности
        //    dragOrigin = Input.mousePosition;
        //}

        if (isDraggingCam)
        {
            // Рассчитываем динамический множитель скорости на основе FOV
            float fovFactor = Mathf.Lerp(0.7f, 1.3f, Mathf.InverseLerp(22, 82, mainCamera.fieldOfView));
            float currentDragSpeed = dragSpeed * fovFactor * dragSpeedMultiplier;

            // Создаем луч из камеры в точку мыши
            Ray rayD = mainCamera.ScreenPointToRay(Input.mousePosition);
            Ray prevRay = mainCamera.ScreenPointToRay(dragOrigin);

            float enter, prevEnter;

            if (dragPlane.Raycast(rayD, out enter) && dragPlane.Raycast(prevRay, out prevEnter))
            {
                // Получаем точки на плоскости
                Vector3 currentPoint = rayD.GetPoint(enter);
                Vector3 prevPoint = prevRay.GetPoint(prevEnter);

                // Вычисляем разницу
                Vector3 difference = prevPoint - currentPoint;
                difference.y = 0; // Игнорируем вертикальное движение

                // Вычисляем новую позицию с фиксированной высотой
                Vector3 newPosition = mainCamera.transform.position + difference;
                newPosition.y = _yPos;

                // Применяем ограничения границ
                newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

                // Плавное перемещение
                mainCamera.transform.position = Vector3.Lerp(
                    mainCamera.transform.position,
                    newPosition,
                    currentDragSpeed * Time.deltaTime
                );
            }

            // Обновляем точку отсчета
            dragOrigin = Input.mousePosition;
        }
        #endregion

        if (isScriptOn)
        {
            HandleEdgeMovement();
            ApplyBoundaries();
        }
        HandleZoom();
    }

    private void UnHoverCurrent()
    {
        Hovered?.OnUnHover();
        Hovered = null;
    }

    private void UnselectAll()
    {
        Selected.Unselect();
        Selected = null;
    }

    private void Select(SelectableObj selectableObj)
    {
        if (Selected != selectableObj)
        {
            Selected = selectableObj;
            selectableObj.Select();
        }
    }
    
    private float GetLineTotalLength(LineRenderer lineRenderer)
    {
        if (lineRenderer.positionCount < 2)
            return 0f;

        float totalLength = 0f;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            Vector3 point1 = lineRenderer.GetPosition(i);
            Vector3 point2 = lineRenderer.GetPosition(i + 1);
            totalLength += Vector3.Distance(point1, point2);
        }

        return totalLength;
    }

    // Camera Group
    #region Camera

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

        mainCamera.transform.Translate(currentVelocity * Time.deltaTime, Space.World);
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
        Vector3 pos = mainCamera.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    // Высота камеры
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Обрабатываем ввод только если не в процессе анимации
        if (!_isBlockedZoom && scroll != 0)
        {
            _isBlockedZoom = true;

            // Определяем направление скролла
            int scrollDirection = scroll > 0 ? -1 : 1;
            _zoomPreset = Mathf.Clamp(_zoomPreset + scrollDirection, 0, 3);

            switch (_zoomPreset)
            {
                case 0: _yPos = 15; _fov = 22; break;
                case 1: _yPos = 15; _fov = 42; break;
                case 2: _yPos = 15; _fov = 62; break;
                case 3: _yPos = 15; _fov = 82; break;
            }
        }

        // Всегда обрабатываем анимацию, если она активна
        if (_isBlockedZoom)
        {
            // Плавное изменение FOV
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, _fov, Time.deltaTime * _transitionSpeed);

            // Плавное изменение позиции
            Vector3 currentCameraPos = mainCamera.transform.position;
            currentCameraPos.y = Mathf.Lerp(currentCameraPos.y, _yPos, Time.deltaTime * _transitionSpeed);
            mainCamera.transform.position = currentCameraPos;

            // Проверяем завершение анимации с допуском
            if (Mathf.Abs(mainCamera.fieldOfView - _fov) < _threshold &&
                Mathf.Abs(currentCameraPos.y - _yPos) < _threshold)
            {
                // Финализируем значения
                mainCamera.fieldOfView = _fov;
                currentCameraPos.y = _yPos;
                mainCamera.transform.position = currentCameraPos;

                _isBlockedZoom = false;
            }
        }
    }

    Vector3 GetMousePositionWithDepth()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.transform.position.z * -1;

        return mousePos;
    }

    void MoveCameraLocalTowardsMouse()
    {
        // 1. Получаем позицию мыши и центр экрана
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

        // 2. Вектор от центра к курсору
        Vector3 direction = mouseScreenPos - screenCenter;

        // 3. Если мышка слишком близко к центру - не двигаем
        if (direction.magnitude < 10f) return;

        // 4. Нормализуем направление (приводим к диапазону [-1, 1])
        Vector2 normalizedDirection = direction.normalized;

        // 5. Получаем локальные оси камеры (без наклона по Y)
        Vector3 cameraRight = mainCamera.transform.right;
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraRight.y = 0;
        cameraForward.y = 0;
        cameraRight.Normalize();
        cameraForward.Normalize();

        // 6. Вычисляем направление движения (по локальным осям)
        Vector3 moveDirection = (cameraRight * normalizedDirection.x) + (cameraForward * normalizedDirection.y);

        // 7. Вычисляем смещение (половина расстояния от центра до мыши)
        float moveDistance = direction.magnitude * 0.5f;

        // 8. Мгновенно сдвигаем камеру (без Time.deltaTime)
        mainCamera.transform.position += moveDirection * moveDistance;
    }
    #endregion
}
