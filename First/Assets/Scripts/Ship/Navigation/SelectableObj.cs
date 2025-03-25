using UnityEngine;
using UnityEngine.AI;

public class SelectableObj : MonoBehaviour
{
    public GameObject Marker;
    public NavMeshAgent Agent;

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

    public void WhenClickOnGround(Vector3 point)
    {
        Agent.SetDestination(point);
    }
}
