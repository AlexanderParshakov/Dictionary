using New_designed_Dictionary.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_designed_Dictionary.ViewModels
{
    public partial class VMLanguage : Language
    {
        public string LanguageFullName { get; set; }

        public void AcquireFullName()
        {
            LanguageFullName = LanguageName + " (" + Location + ")";
        }
    }
}
