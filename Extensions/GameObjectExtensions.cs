using Bark.Tools;
using UnityEngine;

namespace Bark.Extensions
{
    public static class GameObjectExtensions
    {
        public static void Log(this GameObject self)
        {

            Logging.LogDebug($"\"{self.name}\": {{");

            // Log the components on the object
            Logging.LogDebug("\"Components\": [");
            var comps = self.GetComponents<Component>();
            int i = 0;
            foreach (var comp in self.GetComponents<Component>())
            {
                Logging.LogDebug($"\"{comp.GetType()}\"");
                i++;
                if (i < comps.Length)
                    Logging.LogDebug(",");
            }
            Logging.LogDebug("],");

            Logging.LogDebug("\"Children\": {");
            // Log the children
            i = 0;
            foreach (Transform transform in self.transform)
            {
                transform.gameObject.Log();
                i++;
                if (i < self.transform.childCount)
                    Logging.LogDebug(",");
            }
            Logging.LogDebug("}");
            Logging.LogDebug("}");
        }

        public static void Obliterate(this GameObject self)
        {
            Object.Destroy(self);
        }

        public static void Obliterate(this Component self)
        {
            Object.Destroy(self);
        }
    }
}
