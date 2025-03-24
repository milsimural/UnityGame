using UnityEngine;

public class SelectableObj : MonoBehaviour
{
    public GameObject Marker;

    public void Start()
    {
        Marker.SetActive(false);
    }
    public void OnHover()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnUnHover()
    {
        transform.localScale = Vector3.one;
    }

    public void Select()
    {
        Marker.SetActive(true);
    }

    public void Unselect()
    {
        Marker.SetActive(false);
    }
}
