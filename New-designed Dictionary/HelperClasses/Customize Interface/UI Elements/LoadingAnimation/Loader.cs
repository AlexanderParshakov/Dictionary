using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace New_designed_Dictionary.HelperClasses.Customize_Interface.UI_Elements.LoadingAnimation
{
    public static class Loader
    {
        public static void StartLoadingWords(DataGrid dgWords, LoadingAnimation loadingAnimation)
        {
            dgWords.Visibility = Visibility.Collapsed;
            loadingAnimation.Visibility = Visibility.Visible;
        }
        public static void StopLoadingWords(DataGrid dgWords, LoadingAnimation loadingAnimation)
        {
            dgWords.Visibility = Visibility.Visible;
            loadingAnimation.Visibility = Visibility.Collapsed;
        }
        public static void StartLoadingSources(ListView lvSources, LoadingAnimation loadingAnimation)
        {
            lvSources.Visibility = Visibility.Collapsed;
            loadingAnimation.Visibility = Visibility.Visible;
        }
        public static void StopLoadingSources(ListView lvSources, LoadingAnimation loadingAnimation)
        {
            lvSources.Visibility = Visibility.Visible;
            loadingAnimation.Visibility = Visibility.Collapsed;
        }

    }
}
