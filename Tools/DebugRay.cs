using System.Collections.Generic;
using UnityEngine;

namespace Bark.Tools
{
    public class DebugRay : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Color color = Color.red;

        public void Start ()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startColor = color;
            lineRenderer.startWidth = .01f;
            lineRenderer.endWidth = .01f;
            
            lineRenderer.material =
                GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<Renderer>().material;
        }

        public void Set(Vector3 start, Vector3 direction)
        {
            lineRenderer.material.color = color;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start + direction);
        }

        public static Dictionary<string, DebugRay> rays = new Dictionary<string, DebugRay>();
        public static DebugRay Get(string name)
        {
            if(rays.ContainsKey(name)) return rays[name];
            DebugRay ray = new GameObject($"{name} (Debug Ray)").AddComponent<DebugRay>();
            rays.Add(name, ray);
            return ray;
        }

        public DebugRay SetColor(Color c)
        {
            this.color = c;
            return this;
        }

    }
}
