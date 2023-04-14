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
            lineRenderer.startWidth = .01f;
            lineRenderer.endWidth = .01f;
        }

        public void Set(Vector3 start, Vector3 direction)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start + direction);
        }

    }
}
