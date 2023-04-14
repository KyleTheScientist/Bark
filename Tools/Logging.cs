using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bark.Tools
{
    public static class Logging
    {

        public static void Log(params object[] content)
        {
            Console.WriteLine(string.Join(" ", content));
            Debug.Log(string.Join(" ", content));
        }
    }
}
