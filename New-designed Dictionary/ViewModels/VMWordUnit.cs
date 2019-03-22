using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_designed_Dictionary.ViewModels
{
    public partial class VMWordUnit : WordUnit
    {
        public string SourceName { get; set; }
        public List<string> AllSources { get; set; }
    }
}
