using Bark;
using Bark.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRenderer : MonoBehaviour
{
    private Dictionary<Transform, BoxCollider> boxColliders;
    private Dictionary<Transform, SphereCollider> sphereColliders;
    public float refreshRate = 1;
    Transform sphere;

    void Start()
    {
        boxColliders = new Dictionary<Transform, BoxCollider>();
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            sphere.GetComponent<BoxCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            sphere.GetComponent<MeshRenderer>().material = material;
            sphere.SetParent(collider.transform);
            boxColliders.Add(sphere, collider);
        }
        sphereColliders = new Dictionary<Transform, SphereCollider>();
        foreach (SphereCollider collider in GetComponents<SphereCollider>())
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            sphere.GetComponent<SphereCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            sphere.GetComponent<MeshRenderer>().material = material;
            sphere.SetParent(collider.transform);
            sphereColliders.Add(sphere, collider);
        }
        Recalculate();
    }

    void FixedUpdate()
    {
        if (Time.frameCount % (60 * refreshRate) == 0)
        {
            Recalculate();
        }
    }

    public void Recalculate()
    {
        foreach (var entry in boxColliders)
        {
            Transform cube = entry.Key;
            BoxCollider collider = entry.Value;
            if (!collider) continue;
            cube.localPosition = collider.center;
            cube.localScale = new Vector3(
                collider.size.x,
                collider.size.y,
                collider.size.z
            );
        }
        foreach (var entry in sphereColliders)
        {
            Transform sphere = entry.Key;
            SphereCollider collider = entry.Value;
            if (!collider) continue;
            sphere.localPosition = collider.center;
            sphere.localScale = new Vector3(
                collider.radius * 2,
                collider.radius * 2,
                collider.radius * 2
            );
        }
    }
}
