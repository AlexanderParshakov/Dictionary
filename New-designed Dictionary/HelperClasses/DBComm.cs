using New_designed_Dictionary.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace New_designed_Dictionary.HelperClasses
{
    public class DBComm
    {
        private static MyOwnDictionaryContext GetCurrentContext()
        {
            return new MyOwnDictionaryContext();
        }
        public static MyOwnDictionaryContext Context = GetCurrentContext();
        public static User GlobalUser = Context.Users.Single(u => u.Login == MainWindow.UserName);
        #region Data Retrieval
        public static ObservableCollection<VMWordUnit> GetVMWordUnits(int Language = 1)
        {
            List<WordUnit> WordUnits = new List<WordUnit>();

            WordUnits = Context.WordUnits
                .AsNoTracking()
                .Where(l => l.Users.Select(c => c.Login).Contains(GlobalUser.Login))
                .Where(x => x.Languages.Select(s => s.Id).Contains(Language)).ToList();
            return ConvertToVMWordUnits(new List<WordUnit>(WordUnits.Take(5)));
        }
        public static ObservableCollection<VMSource> GetVMSources(int Language = 1)
        {
            GlobalUser = Context.Users.Single(u => u.Login == MainWindow.UserName);
            ObservableCollection<Source> sources = new ObservableCollection<Source>();
            sources = new ObservableCollection<Source>(GlobalUser.Sources);
            return ConvertToVMSources(sources);
        }
        public static ObservableCollection<Source> GetSources()
        {
            ObservableCollection<Source> sources = new ObservableCollection<Source>();
            return new ObservableCollection<Source>(Context.Sources.Where(l => l.Users.Select(c => c.Login).Contains(GlobalUser.Login)).ToList().OrderBy(s => s.Name));
        }
        public static ObservableCollection<Tag> GetTags(bool AllCheckedNecessary = true)
        {
            if (AllCheckedNecessary == true) // if all tags should be checked on the view (for instance, on the main form)
            {
                return GetCheckedTags(Context.Tags.Where(l => l.Users.Select(c => c.Login).Contains(GlobalUser.Login)).ToList());
            }
            else
            {
                return new ObservableCollection<Tag>(Context.Tags.Where(l => l.Users.Select(c => c.Login).Contains(GlobalUser.Login)).OrderBy(t => t.Name));
            }
        }
        public static ObservableCollection<UnitType> GetUnitTypes()
        {
            return new ObservableCollection<UnitType>(Context.UnitTypes.OrderBy(u => u.Name));
        }

        public static VMLanguage FromLanguageToVMLanguage(Language lang)
        {
            VMLanguage vmLang = new VMLanguage { Id = lang.Id, LanguageName = lang.LanguageName, Location = lang.Location};
            vmLang.AcquireFullName();

            return vmLang;
        }
        public static ObservableCollection<VMLanguage> GetVMLanguages()
        {
            var list = new ObservableCollection<Language>(Context.Languages.Where(l => l.Users.Select(c => c.Login).Contains(GlobalUser.Login)).OrderBy(t => t.LanguageName));
            var VMlist = new ObservableCollection<VMLanguage>();
            foreach (var lang in list)
            {
                VMlist.Add(FromLanguageToVMLanguage(lang));
            }
            return VMlist;
        }


        #region Modifying Lists
        public static ObservableCollection<VMWordUnit> ConvertToVMWordUnits(List<WordUnit> WordUnits)
        {
            ObservableCollection<VMWordUnit> VMWordUnits = new ObservableCollection<VMWordUnit>();

            for (int i = 0; i < WordUnits.Count; i++)
            {
                VMWordUnits.Add(FromWordUnitToVMWordUnit(WordUnits[i]));
            }
            return VMWordUnits;
        }
        private static ObservableCollection<VMSource> ConvertToVMSources(ObservableCollection<Source> Sources)
        {
            ObservableCollection<VMSource> VMSources = new ObservableCollection<VMSource>();

            foreach (var source in Sources)
            {
                VMSources.Add(FromSourceToVMSource(source));
            }
            VMSources = new ObservableCollection<VMSource>(VMSources.OrderBy(x => x.Name));
            return VMSources;
        }
        private static ObservableCollection<Tag> GetCheckedTags(List<Tag> tags)
        {
            foreach (var tag in tags)
            {
                tag.IsChecked = true;
            }
            return new ObservableCollection<Tag>(tags.OrderBy(t => t.Name));
        }
        #endregion

        #region Modifying Objects
        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
        public static VMSource FromSourceToVMSource(Source source)
        {
            return new VMSource { Id = source.Id, Name = source.Name, ImageData = ToImage(source.Picture) };
        }
        public static VMWordUnit FromWordUnitToVMWordUnit(WordUnit wu)
        {
            VMWordUnit VMwu = new VMWordUnit();
            if (wu.Sources != null)
            {
                foreach (Source source in wu.Sources)
                {
                    VMwu.SourceId = source.Id;
                }
            }

            VMwu.Id = wu.Id;
            VMwu.ContentOfUnit = wu.ContentOfUnit;
            VMwu.Meaning = wu.Meaning;
            VMwu.Example = wu.Example;
            VMwu.Note = wu.Note;
            VMwu.Datetime = wu.Datetime;
            VMwu.Sources = wu.Sources;
            VMwu.TagsToString();
            return VMwu;
        }
        public static WordUnit FromVMWordUnitToWordUnit(VMWordUnit VMwu)
        {
            WordUnit wu = new WordUnit
            {
                Id = VMwu.Id,
                ContentOfUnit = VMwu.ContentOfUnit,
                Meaning = VMwu.Meaning,
                Example = VMwu.Example,
                Note = VMwu.Note,
            };
            return wu;
        }
        #endregion

        #endregion
        #region Data Editing
        public static void ChangeUnitSource(int UnitId, int SourceId)
        {
            WordUnit wu = Context.WordUnits.SingleOrDefault(w => w.Id == UnitId); // getting the WordUnit instance from DB
            wu.Sources.Clear();

            wu.Sources.Add(Context.Sources.SingleOrDefault(s => s.Id == SourceId)); // adding the source with the given Id
            Context.SaveChanges();
        }
        public static void UpdateWordUnit(VMWordUnit VMWu)
        {
            WordUnit newWu = Context.WordUnits.SingleOrDefault(x => x.Id == VMWu.Id);
            newWu.Id = VMWu.Id;
            newWu.ContentOfUnit = VMWu.ContentOfUnit;
            newWu.Meaning = VMWu.Meaning;
            newWu.Example = VMWu.Example;
            newWu.Note = VMWu.Note;
            newWu.Sources = VMWu.Sources;
            newWu.Tags = VMWu.Tags;
            newWu.UnitTypes = VMWu.UnitTypes;
            newWu.Languages = VMWu.Languages;
            Context.SaveChanges();
        }
        public static void UpdateLastUsedLanguage(int langId)
        {
            GlobalUser.LastUsedLanguage = langId;
            Context.SaveChanges();
        }

        #endregion
        #region Data Adding
        public static void AddWordUnit(WordUnit wu)
        {
            wu.Datetime = DateTime.Now;
            Context.WordUnits.Add(wu);
            Context.SaveChanges();
        }
        public static void AddTag(string name)
        {
            Tag tag = new Tag { Name = name };
            tag.Users.Add(GlobalUser);
            Context.Tags.Add(tag);
            Context.SaveChanges();
        }
        #endregion
        #region Data Deleting
        public static void DeleteWordUnit(WordUnit wu)
        {
            wu = Context.WordUnits.SingleOrDefault(x => x.Id == wu.Id);
            wu.Sources.Clear();
            wu.Tags.Clear();
            wu.UnitTypes.Clear();
            wu.Users.Clear();
            wu.Languages.Clear();
            Context.WordUnits.Remove(wu);
            Context.SaveChanges();
        }

        #endregion
    }
}
