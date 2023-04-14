using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionObserver : MonoBehaviour
{
    public LayerMask layerMask = ~0;
    public Action<GameObject, Collision> OnCollisionEntered, OnCollisionStayed, OnCollisionExited;
    public Action<GameObject, Collider> OnTriggerEntered, OnTriggerStayed, OnTriggerExited;

    void OnCollisionEnter(Collision collision)
    {
        if (layerMask == (layerMask | (1 << collision.gameObject.layer)))
        {
            OnCollisionEntered?.Invoke(this.gameObject, collision);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (layerMask == (layerMask | (1 << collision.gameObject.layer)))
        {
            OnCollisionStayed?.Invoke(this.gameObject, collision);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (layerMask == (layerMask | (1 << collision.gameObject.layer)))
        {
            OnCollisionExited?.Invoke(this.gameObject, collision);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (layerMask == (layerMask | (1 << collider.gameObject.layer)))
        {
            OnTriggerEntered?.Invoke(this.gameObject, collider);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (layerMask == (layerMask | (1 << collider.gameObject.layer)))
        {
            OnTriggerStayed?.Invoke(this.gameObject, collider);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (layerMask == (layerMask | (1 << collider.gameObject.layer)))
        {
            OnTriggerExited?.Invoke(this.gameObject, collider);
        }
    }
}
