using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    public new CapsuleCollider collider { get; protected set; }

    public float radius => collider.radius;

    public Vector3 center => transform.position;

    public Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

    public Vector3 GetDirectionToPole(Transform other, out float distance)
    {
        var target = new Vector3(center.x, other.position.y, center.z) - other.position;
        distance = target.magnitude;
        return target / distance;
    }

    public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
    {
        var minHeight = collider.bounds.min.y + offset;
        var maxHeight = collider.bounds.max.y - offset;
        var clampedHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
        return new Vector3(point.x, clampedHeight, point.z);
    }

    protected virtual void Awake()
    {
        tag = GameTags.Pole;
        collider = GetComponent<CapsuleCollider>();
    }
}
