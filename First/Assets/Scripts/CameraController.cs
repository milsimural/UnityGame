using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Камера")]
    public Transform cameraTransform;
    
    public float movementSpeed;
    public float movementTime;
    private Vector3 newPosition;
    public float rotationAmount;
    private Quaternion newRotation;
    public Vector3 zoomAmount;
    private Vector3 newZoom;

    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPosition;

    [Header("Edge Scrolling при перетаскивании")]
    [Range(0.01f, 0.2f)] public float edgeThreshold = 0.05f; 
    public float edgeScrollSpeed = 25f; 

    [Header("Менеджер")]
    private SelectableObj Hovered;
    private SelectableObj Selected;
    private bool _isDragging = false;
    private Vector3 _startPosition;
    private LineRenderer _lineRenderer;

    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;


    }

    
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Логика выделения обьекта куда мышкой навелись
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.GetComponent<SelectableCol>())
            {
                SelectableObj hitSelectable = hit.collider.GetComponent<SelectableCol>().MySelectableObj;
                if (Hovered)
                {
                    if (Hovered != hitSelectable)
                    {
                        Hovered.OnUnHover();
                        Hovered = hitSelectable;
                        Hovered.OnHover();
                    }
                }
                else
                {
                    Hovered = hitSelectable;
                    Hovered.OnHover();
                }

            }
            else
            {
                if (Hovered)
                {
                    UnHoverCurrent();
                    if (_isDragging && !Input.GetMouseButton(0)) _isDragging = false;
                }
            }
        }
        else
        {
            UnHoverCurrent();
        }

        if (Hovered && Input.GetMouseButtonDown(0))
        {
            if (Selected) UnselectAll();
            Select(Hovered);
            _isDragging = false;
        }

        if (Hovered && Selected && Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lineRenderer = Selected.GetComponent<LineRenderer>();
            _startPosition = Selected.transform.position;
            _lineRenderer.SetPosition(0, _startPosition);

            // Основные параметры
            _lineRenderer.startWidth = 0.1f;  // Толщина в начале
            _lineRenderer.endWidth = 0.1f;    // Толщина в конце
            _lineRenderer.positionCount = 2;   // Две точки - начало и конец

            // Материал и цвет
            _lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
            _lineRenderer.material.color = Color.white;

            // Плавность линии
            _lineRenderer.numCapVertices = 5;  // Сглаживание концов
            _lineRenderer.numCornerVertices = 5; // Сглаживание углов
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            if (hit.collider)
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
            if (!hit.collider)
            {
                _isDragging = false;
                _lineRenderer.enabled = false;
            }
            else if (hit.collider.tag == "Sea")
            {
                _isDragging = false;
                _lineRenderer.enabled = false;
                Selected.WhenClickOnGround(hit.point);
            }
        }

        if (_isDragging)
        {
            HandleEdgeScrollingWhileDragging();
        }

        HandleMouseInput();
        HandleMovementInput();

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

    void HandleEdgeScrollingWhileDragging()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        // Проверяем выход за границы экрана
        if (mousePos.x < Screen.width * edgeThreshold)
            moveDirection.x = -1; // Левый край
        else if (mousePos.x > Screen.width * (1 - edgeThreshold))
            moveDirection.x = 1; // Правый край

        if (mousePos.y < Screen.height * edgeThreshold)
            moveDirection.z = -1; // Нижний край
        else if (mousePos.y > Screen.height * (1 - edgeThreshold))
            moveDirection.z = 1; // Верхний край

        // Если мышка у края - двигаем камеру
        if (moveDirection != Vector3.zero)
        {
            // Преобразуем направление с учетом поворота камеры
            Vector3 worldDirection = transform.TransformDirection(moveDirection);
            worldDirection.y = 0;

            // Плавное перемещение
            newPosition += worldDirection.normalized * edgeScrollSpeed * Time.deltaTime;
        }
    }

    void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q)) 
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if (Input.GetKey(KeyCode.E)) 
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }

        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomAmount;
        }
        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomAmount;
        }


        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y !=0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }
        
        if (!Hovered && !_isDragging && Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (!_isDragging && Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }

        if(Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }

        if(Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }
}
