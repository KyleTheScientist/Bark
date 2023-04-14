using System;
using UnityEngine;

namespace Bark.Tools
{
    public class DebugLine : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public Transform a, b;
        void Start()
        {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = .1f;
            lineRenderer.endWidth = .1f;
        }

        public void Connect(Transform a, Transform b)
        {
            if(a == null || b == null)
                throw new NullReferenceException($"Transform(s) null: {a?.name}, {b?.name}");
            this.a = a;
            this.b = b;
            Connect(a.position, b.position);
        }

        public void Connect(Vector3 a, Vector3 b)
        {
            lineRenderer.SetPositions(new Vector3[] { a, b });
        }

        void FixedUpdate()
        {
            if (a && b)
                Connect(a.position, b.position);
        }
    }
}
