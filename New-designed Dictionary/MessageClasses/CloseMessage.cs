using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace New_designed_Dictionary.MessageClasses
{
    public class CloseMessage
    {
        public static void AllShortNotification()
        {
            foreach(ShortNotification window in Application.Current.Windows.OfType<ShortNotification>())
            {
                window.FastClose();
            }
        }
    }
}
