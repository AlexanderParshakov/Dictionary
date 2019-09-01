using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_designed_Dictionary.ViewModels
{
    public class SearchSettingsClass
    {
        public bool SearchWhileTyping { get; set; }

        public bool SearchMeaning { get; set; }
        public bool SearchSource { get; set; }
        public bool SearchTags { get; set; }
        public bool SearchExample { get; set; }
        public bool SearchNote { get; set; }


        public SearchSettingsClass(bool searchWhileTyping, bool searchMeaning, bool searchSource, bool searchTags, bool searchExample, bool searchNote)
        {
            SearchWhileTyping = searchWhileTyping;

            SearchMeaning = searchMeaning;
            SearchSource = searchSource;
            SearchTags = searchTags;
            SearchExample = searchExample;
            SearchNote = searchNote;
        }

        public SearchSettingsClass()
        {
            try
            {
                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                StreamReader srReader = new StreamReader(new IsolatedStorageFileStream("SearchSettings.txt", FileMode.OpenOrCreate, isolatedStorage));

                if (srReader == null)
                {

                }
                else
                {
                    while (!srReader.EndOfStream)
                    {
                        string line = srReader.ReadLine();
                        if (line.Contains("False") || line.Contains("True"))
                        {
                            string type = line.Split(':')[0];
                            switch (type)
                            {
                                case "SearchWhileTyping":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchWhileTyping = true;
                                        }
                                        else { SearchWhileTyping = false; }

                                        break;
                                    }
                                case "SearchMeaning":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchMeaning = true;
                                        }
                                        else { SearchMeaning = false; }

                                        break;
                                    }
                                case "SearchSource":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchSource = true;
                                        }
                                        else { SearchSource = false; }

                                        break;
                                    }
                                case "SearchTags":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchTags = true;
                                        }
                                        else { SearchTags = false; }

                                        break;
                                    }
                                case "SearchExample":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchExample = true;
                                        }
                                        else { SearchExample = false; }

                                        break;
                                    }
                                case "SearchNote":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            SearchNote = true;
                                        }
                                        else { SearchNote = false; }

                                        break;
                                    }
                            }
                        }
                    }
                }
                srReader.Close();
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                throw;
            }
        }
    }
}
