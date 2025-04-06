using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using RangeAttribute = UnityEngine.RangeAttribute;

public class Manager : MonoBehaviour
{
    public SelectableObj Hovered;
    public SelectableObj Selected;
    private bool _isDragging = false;
    private Vector3 _startPosition;
    private LineRenderer _lineRenderer;
    //[SerializeField] private float dragSpeed = 75f; 

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
    public float minX = -500f;
    public float maxX = 500f;
    public float minZ = -500f;
    public float maxZ = 500f;

    [Header("Height Settings")]
    private Camera mainCamera;
    // Служебные переменные для перемещения от границ
    private Vector3 currentVelocity;
    private bool isMoving;

    void Start()
    {
        mainCamera = Camera.main;
    }

    
    void Update()
    {
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
        //if (!Hovered && !_isDragging && Input.GetMouseButtonDown(0))
        //     {
        //    Debug.Log("Start");
        //    dragOrigin = Input.mousePosition;
        //         isDraggingCam = true;
        //     }

        //        // Завершение перетаскивания (отпустили ЛКМ)
        //if (!_isDragging && Input.GetMouseButtonUp(0))
        //     {
        //    Debug.Log("End");     
        //    isDraggingCam = false;
        //     }


        if (isScriptOn)
        {
            HandleEdgeMovement();
            ApplyBoundaries();
        }
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
}
