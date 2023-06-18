using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bark.Extensions
{
    public static class MathExtensions
    {
        public static int Wrap(int x, int min, int max)
        {
            if (x < min) x = max - (min - x) + 1;
            if (x > max) x = min + (max - x) + 1;
            return x;
        }
    }
}
