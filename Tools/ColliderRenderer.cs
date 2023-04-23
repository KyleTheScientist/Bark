using Bark;
using Bark.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRenderer : MonoBehaviour
{
    private Dictionary<Transform, BoxCollider> colliders;
    public float refreshRate = 1;
    Transform cube;

    void Start()
    {
        colliders = new Dictionary<Transform, BoxCollider>();
        foreach (BoxCollider collider in GetComponents<BoxCollider>())
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            cube.GetComponent<BoxCollider>().Obliterate();
            Material material = Instantiate(Plugin.assetBundle.LoadAsset<GameObject>("Cloud").GetComponent<Renderer>().material);
            var color = Random.ColorHSV();
            color.a = .25f;
            material.color = color;
            material.SetColor("_EmissionColor", color);
            cube.GetComponent<MeshRenderer>().material = material;
            cube.SetParent(collider.transform);
            colliders.Add(cube, collider);
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
        foreach (var entry in colliders)
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
    }
}
