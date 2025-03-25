using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public Camera Camera;
    public SelectableObj Hovered;
    public List<SelectableObj> ListOfSelected = new List<SelectableObj>();

    void Start()
    {
        
    }

    
    void Update()
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
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
                }
            }
        } else
        {
            UnHoverCurrent();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Hovered)
            {
                if(Input.GetKey(KeyCode.LeftControl) == false)
                {
                    UnselectAll();
                }
                Select(Hovered);
            } else
            {
                if(ListOfSelected.Count > 0 && hit.collider.tag == "Sea")
                {
                    for (int i = 0; i < ListOfSelected.Count; i++)
                    {
                        ListOfSelected[i].WhenClickOnGround(hit.point);
                    }
                }
            }
        }

        if(Input.GetMouseButtonUp(1))
        {
            if(ListOfSelected.Count > 0)
            {
                UnselectAll();
            }
        }

    }

    private void UnHoverCurrent()
    {
        Hovered?.OnUnHover();
        Hovered = null;
    }

    private void UnselectAll()
    {
        for (int i = 0; i < ListOfSelected.Count; i++)
        {
            ListOfSelected[i].Unselect();
        }
        ListOfSelected.Clear();
    }

    private void Select(SelectableObj selectableObj)
    {
        if (ListOfSelected.Contains(selectableObj) == false)
        {
            ListOfSelected.Add(selectableObj);
            selectableObj.Select();
        }
    }
    
}
