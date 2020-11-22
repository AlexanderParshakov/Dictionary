using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace New_designed_Dictionary.HelperClasses.Customize_Interface
{
    public class UIActions
    {
        public static Brush LoaderBrightColor = (Brush) new BrushConverter().ConvertFrom("#FF2A83E5");
        public static Brush LoaderMidColor = (Brush) new BrushConverter().ConvertFrom("#FF3781D5");
        public static Brush LoaderPallidColor = (Brush)new BrushConverter().ConvertFrom("#4a76a8");
        public static void AnimateOpacity(double from, double to, double timespan, object window)
        {
            DoubleAnimation animation = new DoubleAnimation(from, to,
                                   (Duration)TimeSpan.FromSeconds(timespan));
            if (window is Window)
            {
                ((Window)window).BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }
        public static void OpenWindowWithAnimation(dynamic currentWindow, dynamic nextWindow)
        {
            if (currentWindow is Window && nextWindow is Window)
            {
                UIActions.AnimateOpacity(1, 0.5, 1.5, currentWindow);
                ((Window)nextWindow).Owner = (Window)currentWindow;
                nextWindow.ShowInTaskbar = false;
                nextWindow.ShowDialog();
                ((Window)currentWindow).Show();
                UIActions.AnimateOpacity(0.5, 1, 0.5, currentWindow);
            }
        }
        public static byte [] GetReducedImage (byte [] myBytes, int newWidth = 700, int newHeight = 400) // width 250 recommended, height 220
        {
            System.IO.MemoryStream myMemStream = new System.IO.MemoryStream(myBytes);
            System.Drawing.Image fullsizeImage = System.Drawing.Image.FromStream(myMemStream);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Jpeg);  //Or whatever format you want.
            return myResult.ToArray();  //Returns a new byte array.
        }
    }
}
