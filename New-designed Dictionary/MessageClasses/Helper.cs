using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace New_designed_Dictionary.MessageClasses
{
    public class Helper
    {
        static BlurEffect myBlur;

        public static void ShowBlurEffectNotification()
        {
            myBlur.Radius = 20;
            foreach (Window window in Application.Current.Windows)
            {
                window.Effect = myBlur;
            }
        }

        public static void StopBlueEffect()
        {
            myBlur.Radius = 0;
        }
    }
}
