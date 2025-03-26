using UnityEngine;
using UnityEngine.AI;

public class SelectableObj : MonoBehaviour
{
    public GameObject Marker;
    public NavMeshAgent Agent;

    protected virtual void Start()
    {
        Marker.SetActive(false);
    }
    public virtual void OnHover()
    {
        transform.localScale = Vector3.one * 1.1f;
    }

    public virtual void OnUnHover()
    {
        transform.localScale = Vector3.one;
    }

    public virtual void Select()
    {
        Marker.SetActive(true);
    }

    public virtual void Unselect()
    {
        Marker.SetActive(false);
    }

    public virtual void WhenClickOnGround(Vector3 point)
    {
        Agent.SetDestination(point);
    }
}
