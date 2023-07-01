using Bark.Tools;
using UnityEngine;

namespace Bark.Extensions
{
    public static class GameObjectExtensions
    {
        public static void Log(this GameObject self)
        {

            Logging.Debug($"\"{self.name}\": {{");

            // Log the components on the object
            Logging.Debug("\"Components\": [");
            var comps = self.GetComponents<Component>();
            int i = 0;
            foreach (var comp in self.GetComponents<Component>())
            {
                Logging.Debug($"\"{comp.GetType()}\"");
                i++;
                if (i < comps.Length)
                    Logging.Debug(",");
            }
            Logging.Debug("],");

            Logging.Debug("\"Children\": {");
            // Log the children
            i = 0;
            foreach (Transform transform in self.transform)
            {
                transform.gameObject.Log();
                i++;
                if (i < self.transform.childCount)
                    Logging.Debug(",");
            }
            Logging.Debug("}");
            Logging.Debug("}");
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
