using Bark.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bark.Tools
{
    public class DebugRay : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Color color = Color.red;

        public void Awake()
        {
            Logging.Debug(this.name);
            lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
            lineRenderer.startColor = color;
            lineRenderer.startWidth = .01f;
            lineRenderer.endWidth = .01f;
            lineRenderer.material = Plugin.assetBundle.LoadAsset<Material>("X-Ray Material");
            //Destroy(sphere);
        }

        public void Set(Vector3 start, Vector3 direction)
        {
            try
            {
                lineRenderer.material.color = color;
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, start + direction);
            }
            catch (Exception e)
            {
                Logging.Exception(e);
            }
        }

        public void Set(Ray ray)
        {
            Set(ray.origin, ray.direction);
        }

        public static Dictionary<string, DebugRay> rays = new Dictionary<string, DebugRay>();
        public static DebugRay Get(string name)
        {
            if (rays.ContainsKey(name)) return rays[name];
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
