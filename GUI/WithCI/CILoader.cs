using Bark.GUI.ComputerInterface;
using Bepinject;

namespace Bark.GUI
{
    public static class CI
    {
        public static void Init() { Zenjector.Install<BarkCI>().OnProject(); }
    }
}
