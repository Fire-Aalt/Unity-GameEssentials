using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace RenderDream.GameEssentials
{
    public static class HDRColorManager
    {
        public static List<HDRColorComponent> HDRControllers = new();

        public static void ChangeColorIntensity(HDRColorComponent.FinalIntensity finalIntensity, float duration)
        {
            foreach (HDRColorComponent controller in HDRControllers)
            {
                controller.LerpIntensityValue(TimescaleModes.Unscaled, finalIntensity, duration).Forget();
            }
        }
    }
}
