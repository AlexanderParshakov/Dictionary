using New_designed_Dictionary.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_designed_Dictionary.ViewModels
{
    public partial class VMWordUnit : WordUnit
    {
        public int SourceId { get; set; }
        public List<string> AllSources { get; set; }
        public string TagsString { get; set; }

        public void TagsToString()
        {
            TagsString = "";
            this.Tags = DBComm.Context.WordUnits.SingleOrDefault(x => x.Id == this.Id).Tags;

            for (int i = 0; i < this.Tags.Count; i++)
            {
                if (i < this.Tags.Count - 1)
                {
                    TagsString += this.Tags.ToList()[i].Name + Environment.NewLine;
                }
                else
                {
                    TagsString += this.Tags.ToList()[i].Name;
                }
            }
        }
    }
}
