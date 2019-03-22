using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace New_designed_Dictionary.MessageClasses
{
    public class MyShortNotification
    {
        public static string Show()
        {
            ShortNotification shortNotification = new ShortNotification();
            shortNotification.Show();
            return "1";
        }

        public static string ShowDialog()
        {
            ShowBlurEffectAllWindow();
            ShortNotification shortNotification = new ShortNotification();
            shortNotification.ShowDialog();
            shortNotification.Close();
            StopBlurEffectAllWindow();
            return "1";
        }

        static BlurEffect MyBlur = new BlurEffect();
        public static void ShowBlurEffectAllWindow()
        {
            MyBlur.Radius = 20;
        }

        public static void StopBlurEffectAllWindow()
        {
            MyBlur.Radius = 0;
        }
    }
}
