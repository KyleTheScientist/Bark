using Bark.Tools;
using UnityEngine;

namespace SpectatorGUI
{
    public static class Extensions
    {
        public static void Log(this GameObject obj)
        {

            Logging.Log($"\"{obj.name}\": {{");

            // Log the components on the object
            Logging.Log("\"Components\": [");
            var comps = obj.GetComponents<Component>();
            int i = 0;
            foreach (var comp in obj.GetComponents<Component>())
            {
                Logging.Log($"\"{comp.GetType()}\"");
                i++;
                if (i < comps.Length)
                    Logging.Log(",");
            }
            Logging.Log("],");

            Logging.Log("\"Children\": {");
            // Log the children
            i = 0;
            foreach (Transform transform in obj.transform)
            {
                transform.gameObject.Log();
                i++;
                if (i < obj.transform.childCount)
                    Logging.Log(",");
            }
            Logging.Log("}");
            Logging.Log("}");
        }

    }
}
