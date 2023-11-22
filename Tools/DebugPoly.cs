using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bark.Tools
{
    public class DebugPoly : MonoBehaviour
    {
        public Vector3[] vertices = new Vector3[0];
        public Mesh mesh;
        public Renderer renderer;

        void Awake()
        {
            try
            {
                // Create a new mesh and set its vertices and triangles
                mesh = new Mesh();

                // Create a new game object and add a mesh renderer component to it
                GameObject polygon = new GameObject("Debug Polygon");
                polygon.transform.parent = this.transform;
                renderer = polygon.AddComponent<MeshRenderer>();
                renderer.material = Plugin.assetBundle.LoadAsset<Material>("Cloud Material");
                renderer.material.color = new Color(1, 1, 1, .1f);
                // Add a mesh filter component and set the mesh to the one we just created
                MeshFilter filter = polygon.AddComponent<MeshFilter>();
                filter.mesh = mesh;
            }
            catch (Exception e) { Logging.Exception(e); }
        }

        void FixedUpdate()
        {
            try
            {
                if (vertices.Length != 3) return;
                SetColor((Time.frameCount / 1000f) % 1, 1, 1, .1f);

                mesh.Clear();
                mesh.vertices = vertices;
                mesh.uv = new Vector2[]
                {
                    new Vector2(vertices[0].x, vertices[0].y),
                    new Vector2(vertices[1].x, vertices[1].y),
                    new Vector2(vertices[2].x, vertices[2].y),
                };
                mesh.triangles = new int[] {
                    2, 1, 0,
                };
                mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2000);
            }
            catch (Exception e) { Logging.Exception(e); }
        }
        

        public void SetColor(float h, float s, float v, float a = .1f)
        {
            Color color = Color.HSVToRGB(h, s, v);
            color.a = a;
            this.renderer.material.color = color;
            this.renderer.material.SetColor("_EmissionColor", color);

        }
    }
}
