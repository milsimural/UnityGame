using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public Camera Camera;
    public SelectableObj Hovered;
    public SelectableObj Selected;
    public bool isHardcore = false;

    private bool _isDragging = false;
    private Vector3 _startPosition;
    private LineRenderer _lineRenderer;

    void Start()
    {
        
    }

    
    void Update()
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
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
                    Debug.Log("Сброс драгинга");
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
                Debug.Log("Select");
                //_lineRenderer = Selected.GetComponent<LineRenderer>();
                //_startPosition = Selected.transform.position;
                //_startPosition = hit.point;
                //_lineRenderer.SetPosition(0, _startPosition);
                //_lineRenderer.enabled = false;
        }

        if (Hovered && Selected && Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _lineRenderer = Selected.GetComponent<LineRenderer>();
            _startPosition = Selected.transform.position;
            _lineRenderer.SetPosition(0, _startPosition);
            Debug.Log("Selected and click");
        }

        if (_isDragging && Input.GetMouseButton(0))
        {
            Debug.Log("Dragging");
            Vector3 endPosition = hit.point;
            _lineRenderer.SetPosition(1, endPosition);
            if (GetLineTotalLength(_lineRenderer) > 0.7f) {
                _lineRenderer.enabled = true;
            } else {
                _lineRenderer.enabled = false;
            }
        }

        if (_isDragging && hit.collider.tag == "Sea" && Input.GetMouseButtonUp(0))
        {
            Debug.Log("MouseUp in dragging");
            _isDragging = false;
            _lineRenderer.enabled = false;
            Selected.WhenClickOnGround(hit.point);
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
}
