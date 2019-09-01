using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_designed_Dictionary.HelperClasses
{
    public class EqualityComparer : IEqualityComparer<VMWordUnit>
    {
        public bool Equals(VMWordUnit x, VMWordUnit y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(VMWordUnit obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
