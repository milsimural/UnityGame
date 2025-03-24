using UnityEngine;

public class Manager : MonoBehaviour
{
    public Camera Camera;
    public SelectableObj Hovered;

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
                Hovered = hit.collider.GetComponent<SelectableCol>().MySelectableObj;
            }
        }
    }
}
