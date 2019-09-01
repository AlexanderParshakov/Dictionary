using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
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

namespace New_designed_Dictionary.Modals
{
    /// <summary>
    /// Interaction logic for SearchSettings.xaml
    /// </summary>
    public partial class SearchSettings : Window
    {
        public SearchSettings()
        {
            InitializeComponent();
            ReadSettings();
        }


        #region Settings
        private void ReadSettings()
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
                                            chbSearchWhileTyping.IsChecked = true;
                                        }
                                        else { chbSearchWhileTyping.IsChecked = false; }

                                        break;
                                    }
                                case "SearchMeaning":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            chbSearchMeaning.IsChecked = true;
                                        }
                                        else { chbSearchMeaning.IsChecked = false; }

                                        break;
                                    }
                                case "SearchSource":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            chbSearchSource.IsChecked = true;
                                        }
                                        else { chbSearchSource.IsChecked = false; }

                                        break;
                                    }
                                case "SearchTags":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            chbSearchTags.IsChecked = true;
                                        }
                                        else { chbSearchTags.IsChecked = false; }

                                        break;
                                    }
                                case "SearchExample":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            chbSearchExample.IsChecked = true;
                                        }
                                        else { chbSearchExample.IsChecked = false; }

                                        break;
                                    }
                                case "SearchNote":
                                    {
                                        string value = line.Split(':')[1];
                                        if (value.Contains("True"))
                                        {
                                            chbSearchNote.IsChecked = true;
                                        }
                                        else { chbSearchNote.IsChecked = false; }

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
        private void SaveSettings()
        {
            try
            {
                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                StreamWriter srWriter = new StreamWriter(new IsolatedStorageFileStream("SearchSettings.txt", FileMode.Create, isolatedStorage));


                SearchSettingsClass searchSettings = new SearchSettingsClass(
                    (bool)chbSearchWhileTyping.IsChecked,
                    (bool)chbSearchMeaning.IsChecked,
                    (bool)chbSearchSource.IsChecked,
                    (bool)chbSearchTags.IsChecked,
                    (bool)chbSearchExample.IsChecked,
                    (bool)chbSearchNote.IsChecked);
                
                if (searchSettings != null)
                {
                    srWriter.WriteLine("SearchWhileTyping: " + searchSettings.GetType().GetProperty("SearchWhileTyping").GetValue(searchSettings, null));
                    srWriter.WriteLine("SearchMeaning: " + searchSettings.GetType().GetProperty("SearchMeaning").GetValue(searchSettings, null));
                    srWriter.WriteLine("SearchSource: " + searchSettings.GetType().GetProperty("SearchSource").GetValue(searchSettings, null));
                    srWriter.WriteLine("SearchTags: " + searchSettings.GetType().GetProperty("SearchTags").GetValue(searchSettings, null));
                    srWriter.WriteLine("SearchExample: " + searchSettings.GetType().GetProperty("SearchExample").GetValue(searchSettings, null));
                    srWriter.WriteLine("SearchNote: " + searchSettings.GetType().GetProperty("SearchNote").GetValue(searchSettings, null));
                }

                srWriter.Flush();

                srWriter.Close();
            }

            catch (System.Security.SecurityException sx)
            {
                string exception = sx.Message;
                throw;
            }

        }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.Close();
        }
    }
}
