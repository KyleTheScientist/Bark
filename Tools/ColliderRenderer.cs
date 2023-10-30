using Bark;
using Bark.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRenderer : MonoBehaviour
{
    private Dictionary<Transform, BoxCollider> boxColliders;
    private Dictionary<Transform, SphereCollider> sphereColliders;
    private Dictionary<Transform, MeshCollider> meshColliders;
    public float refreshRate = 10;
    Transform obj;
    int refreshOffset;

    void Start()
    {
        refreshOffset = Random.Range(0, 60 * (int)refreshRate);
        boxColliders = new Dictionary<Transform, BoxCollider>();
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            obj.GetComponent<BoxCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            obj.GetComponent<MeshRenderer>().material = material;
            obj.SetParent(collider.transform);
            boxColliders.Add(obj, collider);
        }
        sphereColliders = new Dictionary<Transform, SphereCollider>();
        foreach (SphereCollider collider in GetComponents<SphereCollider>())
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            obj.GetComponent<SphereCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            obj.GetComponent<MeshRenderer>().material = material;
            obj.SetParent(collider.transform);
            sphereColliders.Add(obj, collider);
        }

        foreach (MeshCollider collider in GetComponents<MeshCollider>())
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            obj.GetComponent<BoxCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            obj.GetComponent<MeshRenderer>().material = material;
            obj.SetParent(collider.transform);
            meshColliders.Add(obj, collider);
        }
        Recalculate();
    }
    void FixedUpdate()
    {
        if ((refreshOffset + Time.frameCount) % (60 * refreshRate) == 0)
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
        foreach (var entry in meshColliders)
        {
            Transform cube = entry.Key;
            MeshCollider collider = entry.Value;
            if (!collider) continue;
            cube.localPosition = collider.bounds.center;
            cube.localScale = new Vector3(
                collider.bounds.extents.x,
                collider.bounds.extents.y,
                collider.bounds.extents.z
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

    void OnDestroy()
    {
        obj?.Obliterate();
    }
}
