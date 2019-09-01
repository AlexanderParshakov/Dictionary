using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace New_designed_Dictionary.Views.Modals
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window, IDisposable
    {
        public Action Worker { get; set; }
        public static Brush LoaderBrightColor = (Brush)new BrushConverter().ConvertFrom("#FF2A83E5");
        public static Brush LoaderMidColor = (Brush)new BrushConverter().ConvertFrom("#FF3781D5");
        public static Brush LoaderPallidColor = (Brush)new BrushConverter().ConvertFrom("#4a76a8");
        public LoadingWindow(Action worker)
        {
            InitializeComponent();
            Worker = worker ?? throw new ArgumentNullException();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Worker).ContinueWith(t => { Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Dispose()
        {
            
        }
    }
}
