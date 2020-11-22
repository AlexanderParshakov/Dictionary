using New_designed_Dictionary.Customize_Interface;
using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.HelperClasses.Customize_Interface;
using New_designed_Dictionary.HelperClasses.Customize_Interface.UI_Elements.LoadingAnimation;
using New_designed_Dictionary.Import_and_Export;
using New_designed_Dictionary.Modals;
using New_designed_Dictionary.ViewModels;
using Syn.WordNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Word2vec.Tools;

namespace New_designed_Dictionary
{
    public partial class MainWindow : Window
    {
        #region Global Declarations

        static Vocabulary vectorVocabulary = null;
        static SearchSettingsClass searchSettings = new SearchSettingsClass();
        static int CurrentSourceFromList = 0;
        static VMSource CurrentSourceForContextMenu = new VMSource();
        static string SourceWrapping = "Wrap";
        public static string UserName = "";
        static ObservableCollection<VMWordUnit> DictionaryUnits = new ObservableCollection<VMWordUnit>();

        #endregion 
        #region Animation

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
        private void DisableFindButtons()
        {
            btnFindSeeAlso.IsEnabled = false;
            btnFindSynonyms.IsEnabled = false;
        }

        #endregion
        #region CheckBoxes

        private int CheckBoxUidSummary()
        {
            int Total = 0;
            if ((bool)chbAllParts.IsChecked)
            {
                Total += Convert.ToInt32(chbAllParts.Uid);
            }
            if ((bool)chbAdjectives.IsChecked)
            {
                Total += Convert.ToInt32(chbAdjectives.Uid);
            }
            if ((bool)chbCollocations.IsChecked)
            {
                Total += Convert.ToInt32(chbCollocations.Uid);
            }
            if ((bool)chbInterjections.IsChecked)
            {
                Total += Convert.ToInt32(chbInterjections.Uid);
            }
            if ((bool)chbNouns.IsChecked)
            {
                Total += Convert.ToInt32(chbNouns.Uid);
            }
            if ((bool)chbContractions.IsChecked)
            {
                Total += Convert.ToInt32(chbContractions.Uid);
            }
            if ((bool)chbVerbs.IsChecked)
            {
                Total += Convert.ToInt32(chbVerbs.Uid);
            }
            if ((bool)chbSentences.IsChecked)
            {
                Total += Convert.ToInt32(chbSentences.Uid);
            }
            if ((bool)chbMiscellaneous.IsChecked)
            {
                Total += Convert.ToInt32(chbMiscellaneous.Uid);
            }
            return Total;
        }
        private void SetCheck(bool AllParts)
        {
            RemoveEventsFromUnitTypes();
            int Total = CheckBoxUidSummary();
            if (AllParts == true)
            {
                chbAdjectives.IsChecked = true;
                chbCollocations.IsChecked = true;
                chbInterjections.IsChecked = true;
                chbNouns.IsChecked = true;
                chbContractions.IsChecked = true;
                chbVerbs.IsChecked = true;
                chbSentences.IsChecked = true;
                chbMiscellaneous.IsChecked = true;
            }
            if (AllParts == false && Total == 44)
            {
                chbAdjectives.IsChecked = false;
                chbCollocations.IsChecked = false;
                chbInterjections.IsChecked = false;
                chbNouns.IsChecked = false;
                chbContractions.IsChecked = false;
                chbVerbs.IsChecked = false;
                chbSentences.IsChecked = false;
                chbMiscellaneous.IsChecked = false;
            }
            AddEventsToUnitTypes();
        }
        private void CheckOtherPartsOfSpeech()
        {
            int Total = CheckBoxUidSummary();
            if (Total >= 44)
            {
                chbAllParts.IsChecked = true;
            }
            if (Total < 44)
            {
                chbAllParts.IsChecked = false;
            }
        }

        #endregion
        #region WordNet

        private void ShowSynonyms(string word)
        {
            List<VMWordUnit> wordUnits = new List<VMWordUnit>();

            var directory = Directory.GetCurrentDirectory();
            var wordNet = new WordNetEngine();
            wordNet.LoadFromDirectory(directory);

            var synSetList = wordNet.GetSynSets(word);

            foreach (var synSet in synSetList)
            {
                foreach (string syn in synSet.Words)
                {
                    int index = syn.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);
                    if (index != 0)
                    {
                        VMWordUnit wu = new VMWordUnit { ContentOfUnit = syn.Replace("_", " ") };
                        wordUnits.Add(wu);
                    }
                }
            }
            if (wordUnits.Count == 0)
            {
                lbNoSynonymsFound.Visibility = Visibility.Visible;
            }
            else
            {
                lbNoSynonymsFound.Visibility = Visibility.Collapsed;
            }
            itemsControlSynonyms.ItemsSource = wordUnits.Distinct();
        }

        #endregion
        #region Word2Vec 
        private void ShowSimilars(string word)
        {
            try
            {
                List<string> list = GetSimilars(word);
                List<VMWordUnit> wordUnits = new List<VMWordUnit>();

                foreach (var item in list)
                {
                    VMWordUnit wu = new VMWordUnit { ContentOfUnit = item };
                    wordUnits.Add(wu);
                }
                itemsControlSimilars.ItemsSource = wordUnits.Distinct();
            }
            catch (Exception) { MessageBox.Show("Cannot find a similar word for a phrase"); }
        }
        private List<string> GetSimilars(string word)
        {
            List<string> similars = new List<string>();
            var closest = vectorVocabulary[word].GetClosestFrom(vectorVocabulary.Words.Where(w => w != vectorVocabulary[word]), 5);
            foreach (var neighbour in closest)
            {
                similars.Add(neighbour.Representation.WordOrNull);
            }
            return CutSimilars(word, similars).Distinct().ToList();
        }
        private List<string> CutSimilars(string originalWord, List<string> similars)
        {
            List<string> cutSimilars = new List<string>();
            foreach (var word in similars)
            {
                int index = word.IndexOf(originalWord, StringComparison.CurrentCultureIgnoreCase);
                if (index != 0 && word != originalWord + "ed")
                {
                    cutSimilars.Add(word);
                }
            }
            return cutSimilars;
        }
        private List<string> GetAnalogies(string pairStartWord, string pairEndWord, string baseWord)
        {
            List<string> listAnalogies = new List<string>();
            var analogies = vectorVocabulary.Analogy(pairStartWord, pairEndWord, baseWord, 5);
            foreach (var neightboor in analogies)
            {
                listAnalogies.Add(neightboor.Representation.WordOrNull);
            }
            return listAnalogies;
        }

        #endregion
        #region Load of Program
        private void Initiar()
        {
            DefineItemSources();
            if (searchSettings.SearchWhileTyping == true)
            {
                tbSearchUnits.TextChanged += tbSearchUnits_TextChanged;
            }
            AddEventsToUnitTypes();
            cbTypeOfTagFilter.SelectionChanged += CbTypeOfTagFilter_SelectionChanged;
            cbLanguages.SelectionChanged += CbLanguages_SelectionChanged;
            //vectorVocabulary = new Word2VecBinaryReader().Read(New_designed_Dictionary.Resources.Paths.Word2Vec_bin_file);
            Storyboard sb = this.FindResource("MenuClose") as Storyboard;
            //this.Dispatcher.Invoke(() =>
            //{
            //    sb.Completed += Sb_Completed;
            //    sb.Changed += Sb_Changed;
            //});
        }

        private void Sb_Changed(object sender, EventArgs e)
        {
            SourceWrapping = "NoWrap";
            if (lvSources.Items.Count == 0)
            {
                Loader.StopLoadingSources(lvSources, loaderSources);
            }
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            SourceWrapping = "Wrap";
            CollapseSourceMenu();
        }

        public MainWindow(string userName)
        {
            InitializeComponent();
            UserName = userName;

            var tsk = Task.Factory.StartNew(Initiar);
            tsk.ContinueWith(t => { MessageBox.Show(t.Exception.InnerException.Message); },
       CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted,
       TaskScheduler.FromCurrentSynchronizationContext());
            //Initiar();
            //Properties.Resources.Culture = new CultureInfo(ConfigurationManager.AppSettings["Culture"]);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            UIActions.AnimateOpacity(0, 1, 0.5, this);
        }
        private void AddEventsToUnitTypes()
        {
            chbAllParts.Checked += chbAllParts_Checked;
            chbAllParts.Unchecked += chbAllParts_Unchecked;

            chbNouns.Checked += chbNouns_Checked;
            chbNouns.Unchecked += chbNouns_Unchecked;

            chbVerbs.Checked += chbVerbs_Checked;
            chbVerbs.Unchecked += chbVerbs_Unchecked;

            chbAdjectives.Checked += chbAdjectives_Checked;
            chbAdjectives.Unchecked += chbAdjectives_Unchecked;

            chbContractions.Checked += chbContractions_Checked;
            chbContractions.Unchecked += chbContractions_Unchecked;

            chbCollocations.Checked += chbCollocations_Checked;
            chbCollocations.Unchecked += chbCollocations_Unchecked;

            chbInterjections.Checked += chbInterjections_Checked;
            chbInterjections.Unchecked += chbInterjections_Unchecked;

            chbSentences.Checked += chbSentence_Checked;
            chbSentences.Unchecked += chbSentence_Unchecked;

            chbMiscellaneous.Checked += chbMiscellaneous_Checked;
            chbMiscellaneous.Unchecked += chbMiscellaneous_Unchecked;
        }
        private void RemoveEventsFromUnitTypes()
        {
            chbAllParts.Checked -= chbAllParts_Checked;
            chbAllParts.Unchecked -= chbAllParts_Unchecked;

            chbNouns.Checked -= chbNouns_Checked;
            chbNouns.Unchecked -= chbNouns_Unchecked;

            chbVerbs.Checked -= chbVerbs_Checked;
            chbVerbs.Unchecked -= chbVerbs_Unchecked;

            chbAdjectives.Checked -= chbAdjectives_Checked;
            chbAdjectives.Unchecked -= chbAdjectives_Unchecked;

            chbContractions.Checked -= chbContractions_Checked;
            chbContractions.Unchecked -= chbContractions_Unchecked;

            chbCollocations.Checked -= chbCollocations_Checked;
            chbCollocations.Unchecked -= chbCollocations_Unchecked;

            chbInterjections.Checked -= chbInterjections_Checked;
            chbInterjections.Unchecked -= chbInterjections_Unchecked;

            chbSentences.Checked -= chbSentence_Checked;
            chbSentences.Unchecked -= chbSentence_Unchecked;

            chbMiscellaneous.Checked -= chbMiscellaneous_Checked;
            chbMiscellaneous.Unchecked -= chbMiscellaneous_Unchecked;
        }

        #endregion
        #region Events
        private void gridAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception) { }
        }

        private void btnOpenList_Click(object sender, RoutedEventArgs e)
        {
            OpenSourceMenu();
            if (lvSources.Items.Count == 0)
            {
                Loader.StartLoadingSources(lvSources, loaderSources);
            }
        }
        private void btnCloseList_Click(object sender, RoutedEventArgs e)
        {
            CollapseSourceMenu();
            if (lvSources.Items.Count == 0)
            {
                Loader.StopLoadingSources(lvSources, loaderSources);
            }
        }
        private void btnSaveExample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((VMWordUnit)dgWordUnits.SelectedItem).Example = tbExample.Text;
                DBComm.UpdateWordUnit((VMWordUnit)dgWordUnits.SelectedItem);
                lbSavedExample.Text = "Saved successfully!";
            }
            catch (Exception ex) { lbSavedExample.Text = ex.ToString(); }
        }
        private void btnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((VMWordUnit)dgWordUnits.SelectedItem).Note = tbNote.Text;
                DBComm.UpdateWordUnit((VMWordUnit)dgWordUnits.SelectedItem);
                lbSavedNote.Text = "Saved successfully!";
            }
            catch (Exception ex) { lbSavedNote.Text = ex.ToString(); }

        }
        private void btnAddWordUnit_Click(object sender, RoutedEventArgs e)
        {
            AddWordUnit editDictionaryUnit = new AddWordUnit();
            WordUnit wu = new WordUnit();
            editDictionaryUnit.Unit += value => wu = value;
            editDictionaryUnit.ShowDialog();
            DisableFindButtons();
            if (itemsControlTags.Items.Count != DBComm.GetTags().Count)
            {
                itemsControlTags.ItemsSource = DBComm.GetTags();
            }
            if (wu.Id != 0)
            {
                DictionaryUnits.Add(DBComm.FromWordUnitToVMWordUnit(wu));
                dgWordUnits.ItemsSource = null;
                DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.OrderByDescending(x => x.Datetime));
                dgWordUnits.ItemsSource = DictionaryUnits;
            }
        }
        private void btnSearchUnits_Click(object sender, RoutedEventArgs e)
        {
            Loader.StartLoadingWords(dgWordUnits, loaderWordUnits);
            var tsk = Task.Factory.StartNew(RefreshDictionaryList);
            tsk.ContinueWith(t => { MessageBox.Show(t.Exception.InnerException.Message); },
       CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted,
       TaskScheduler.FromCurrentSynchronizationContext());
            //RefreshDictionaryList();
        }
        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            UIActions.OpenWindowWithAnimation(this, new SearchSettings());
            DisableFindButtons();
            searchSettings = new SearchSettingsClass();
            if (searchSettings.SearchWhileTyping == true)
            {
                tbSearchUnits.TextChanged += tbSearchUnits_TextChanged;
            }
            else
            {
                tbSearchUnits.TextChanged -= tbSearchUnits_TextChanged;
            }
        }
        private void btnFindSynonyms_Click(object sender, RoutedEventArgs e)
        {
            if (dgWordUnits.SelectedItems.Count != 0)
            {
                ShowSynonyms(((VMWordUnit)dgWordUnits.SelectedItem).ContentOfUnit);
                btnFindSynonyms.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("You should select a unit in the list to do this");
            }
        }
        private void btnFindSeeAlso_Click(object sender, RoutedEventArgs e)
        {
            //List<string> list = GetAnalogies("Strong", "Strength", "Intelligent");
            if (dgWordUnits.SelectedItems.Count != 0)
            {
                if (vectorVocabulary != null)
                {
                    ShowSimilars(((VMWordUnit)dgWordUnits.SelectedItem).ContentOfUnit);
                    btnFindSeeAlso.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show("Word2Vec is disabled!");
                }
            }
            else
            {
                MessageBox.Show("You should select a unit in the list to do this");
            }
        }
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportPreparation importPrep = new ImportPreparation();
            UIActions.AnimateOpacity(1, 0.5, 1.5, this);
            importPrep.Owner = this;
            importPrep.ShowInTaskbar = false;
            importPrep.ShowDialog();
            DisableFindButtons();
            if (importPrep.ImportDone == true)
            {
                RefreshDictionaryList();
            }
            Show();
            UIActions.AnimateOpacity(0.5, 1, 0.5, this);
            RefreshDictionaryList();
        }
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            UIActions.OpenWindowWithAnimation(this, new ExportPreparation());
            DisableFindButtons();
        }
        private void btnSelectAllTags_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                checkBox.Checked -= chbTags_Checked;
                checkBox.Unchecked -= chbTags_Unchecked;
                checkBox.IsChecked = true;
            }

            var items = itemsControlTags.Items;
            RefreshDictionaryList();
            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                checkBox.Checked += chbTags_Checked;
                checkBox.Unchecked += chbTags_Unchecked;
            }
        }
        private void btnDeselectAllTags_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                checkBox.Checked -= chbTags_Checked;
                checkBox.Unchecked -= chbTags_Unchecked;
                checkBox.IsChecked = false;
            }

            dgWordUnits.ItemsSource = null;
            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                checkBox.Checked += chbTags_Checked;
                checkBox.Unchecked += chbTags_Unchecked;
            }
            DictionaryUnits.Clear();
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            VMWordUnit wu = (VMWordUnit)dgWordUnits.SelectedItem;
            VMWordUnit oldWu = wu;
            wu.Tags = DBComm.Context.WordUnits.SingleOrDefault(x => x.Id == wu.Id).Tags;
            wu.UnitTypes = DBComm.Context.WordUnits.SingleOrDefault(x => x.Id == wu.Id).UnitTypes;
            EditDictionaryUnit editDictionaryUnit = new EditDictionaryUnit(wu, true);
            int index = DictionaryUnits.IndexOf(wu);
            editDictionaryUnit.Unit += value => wu = value;
            editDictionaryUnit.ShowDialog();
            if (wu.ContentOfUnit != null && wu.Meaning != null)
            {
                wu.TagsToString();
                DictionaryUnits[index] = wu;
                dgWordUnits.ItemsSource = null;
                DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.OrderByDescending(x => x.Datetime));
                dgWordUnits.ItemsSource = DictionaryUnits;
                DisableFindButtons();
            }
            if (itemsControlTags.Items.Count != DBComm.GetTags().Count)
            {
                itemsControlTags.ItemsSource = DBComm.GetTags();
            }
        }
        private void btnDeleteUnit_Click(object sender, RoutedEventArgs e)
        {
            DBComm.DeleteWordUnit((VMWordUnit)dgWordUnits.SelectedItem);
            DictionaryUnits.Remove((VMWordUnit)dgWordUnits.SelectedItem);
            dgWordUnits.Items.Refresh();
        }

        #region CheckBoxes for Parts of Speech

        private void chbAllParts_Checked(object sender, RoutedEventArgs e)
        {
            SetCheck((bool)chbAllParts.IsChecked);
            RefreshDictionaryList();
        }
        private void chbAllParts_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheck((bool)chbAllParts.IsChecked);
            dgWordUnits.ItemsSource = null;
        }
        private void chbNouns_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbVerbs_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbAdjectives_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbContractions_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbCollocations_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbInterjections_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbNouns_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbVerbs_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbAdjectives_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbContractions_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbCollocations_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbInterjections_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbSentence_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbSentence_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbMiscellaneous_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbMiscellaneous_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbTags_Checked(object sender, RoutedEventArgs e)
        {
            //var chb = (FrameworkElement)sender;
            //var tag = (Tag)chb.DataContext;
            //DictionaryUnits = AddByTag(DictionaryUnits, tag);
            //dgWordUnits.ItemsSource = null;
            //dgWordUnits.ItemsSource = DictionaryUnits;
            RefreshDictionaryList();
        }
        private void chbTags_Unchecked(object sender, RoutedEventArgs e)
        {
            //var chb = (FrameworkElement)sender;
            //var tag = (Tag)chb.DataContext;
            //DictionaryUnits = RemoveByTag(DictionaryUnits, tag);
            //dgWordUnits.ItemsSource = null;
            //dgWordUnits.ItemsSource = DictionaryUnits;
            RefreshDictionaryList();
        }
        #endregion

        private void lvSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomView.ChangeCheckVisibility(lvSources, "iconChecked");
            if (iconAllChecked.Visibility == Visibility.Visible)
            {
                lvInitializedItems.UnselectAll();
            }
            iconAllChecked.Visibility = Visibility.Hidden;
            RefreshDictionaryList();
            //var source = (Source)lvSources.SelectedItem;
            //itemsControlTags.ItemsSource = null;
            //itemsControlTags.ItemsSource = DBComm.GetTagsBySource(source);
        }
        private void lvInitializedItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItems.Count != 0)
            {
                switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
                {
                    case "AddItem":
                        {
                            CaseAddItem();
                            break;
                        }
                    case "SelectAll":
                        {
                            try
                            {
                                lvSources.UnselectAll();
                                CaseSelectAll();
                            }
                            catch (Exception) { }
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        private void cbWordSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int UnitId = ((WordUnit)dgWordUnits.SelectedItem).Id;
                ComboBox combobox = (ComboBox)sender;
                int SourceId = ((Source)combobox.SelectedItem).Id;
                DBComm.ChangeUnitSource(UnitId, SourceId);
            }
            catch (Exception exc) { string str = exc.ToString(); }
        }
        private void CbWordSources_DropDownOpened(object sender, EventArgs e)
        {
            ((ComboBox)sender).ItemsSource = DBComm.GetSources();
        }
        private void CbWordSources_DropDownClosed(object sender, EventArgs e)
        {
            ((ComboBox)sender).ItemsSource = null;
        }

        private void CbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDictionaryList();
            DBComm.UpdateLastUsedLanguage((cbLanguages.SelectedItem as Language).Id);
        }
        private void dgWordUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgWordUnits.SelectedItems.Count != 0)
            {
                btnFindSynonyms.IsEnabled = true;
                btnFindSeeAlso.IsEnabled = true;
                btnFindSynonyms.Visibility = Visibility.Visible;
                btnFindSeeAlso.Visibility = Visibility.Visible;
                itemsControlSynonyms.ItemsSource = null;
                itemsControlSimilars.ItemsSource = null;
                lbNoSynonymsFound.Visibility = Visibility.Collapsed;
                lbNoSimilarsFound.Visibility = Visibility.Collapsed;
                OperateOnWordUnitSelectionChanged();
            }
        }

        private void tbExample_GotFocus(object sender, RoutedEventArgs e)
        {
            tbExample.SelectionStart = tbExample.Text.Length; // add some logic if length is 0
            tbExample.SelectionLength = 0;
        }
        private void tbNote_GotFocus(object sender, RoutedEventArgs e)
        {
            tbNote.SelectionStart = tbNote.Text.Length; // add some logic if length is 0
            tbNote.SelectionLength = 0;
        }
        private void tbSearchUnits_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }
        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            //CheckSearchInput(tbSearch, New_designed_Dictionary.Resources.Literals.Placeholder_SearchSources);
        }

        private void tbSearchUnits_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }
        private void tbSearchUnits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RefreshDictionaryList();
            }
        }

        private void tbSearchUnits_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDictionaryList();
        }

        private void dgWordUnits_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBlock t = e.EditingElement as TextBlock;  // Assumes columns are all TextBoxes
            DataGridColumn dgc = e.Column;
            DataGridRow dgr = e.Row;
            VMWordUnit wu = (VMWordUnit)dgr.Item;

            var textbox = dgWordUnits.FindAllVisualDescendants()
    .Where(elt => elt.Name == "tbMeaning")
    .OfType<TextBox>()
    .FirstOrDefault();

            string NewMeaning = textbox.Text.ToString();
            string Word = wu.ContentOfUnit;
            //OntologyProcessor.UpdateGraph(Word, "HasMeaning", NewMeaning.Replace(" ", "_"), "IsMeaningOf");
            dgWordUnits.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));

        }
        #endregion
        #region SourceMenu

        private void CollapseSourceMenu()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                btnOpenList.Visibility = Visibility.Visible;
                btnCloseList.Visibility = Visibility.Collapsed;
                btnCloseList.Width = 50;
                lbHideSourceMenu.Visibility = Visibility.Collapsed;
                lbOpenSourceMenu.Visibility = Visibility.Visible;
                gridSearch.Width = 50;
                //tbSearch.Visibility = Visibility.Collapsed;
                lvSources.Visibility = Visibility.Collapsed;
                lvInitializedItems.Visibility = Visibility.Collapsed;
            }));
        }
        private void OpenSourceMenu()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                btnOpenList.Visibility = Visibility.Collapsed;
                btnCloseList.Visibility = Visibility.Visible;
                btnCloseList.Width = 210;
                lbHideSourceMenu.Visibility = Visibility.Visible;
                lbOpenSourceMenu.Visibility = Visibility.Collapsed;
                gridSearch.Width = 210;
                //tbSearch.Visibility = Visibility.Visible;
                lvSources.Visibility = Visibility.Visible;
                lvInitializedItems.Visibility = Visibility.Visible;
            }));
        }

        #endregion SourceMenu
        #region Bind Items to Controls
        private void RefreshDictionaryList()
        {
            Language lang = cbLanguages.SelectedItem as Language;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                DictionaryUnits = GetFilteredByEverything(tbSearchUnits.Text, lang);
            }));
            DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.OrderByDescending(x => x.Datetime));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                dgWordUnits.ItemsSource = DictionaryUnits;
                lbNumberOfUnits.Text = New_designed_Dictionary.Resources.Literals.Label_NumberOfUnits + dgWordUnits.Items.Count;
                Loader.StopLoadingWords(dgWordUnits, loaderWordUnits);
            }));
        }
        private void DefineItemSources()
        {
            DictionaryUnits = DBComm.GetVMWordUnits();
            DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.OrderByDescending(x => x.Datetime));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                dgWordUnits.ItemsSource = DictionaryUnits;
                lbNumberOfUnits.Text = New_designed_Dictionary.Resources.Literals.Label_NumberOfUnits + DictionaryUnits.Count;
                loaderWordUnits.Visibility = Visibility.Collapsed;
            }));
            var sourcesRes = DBComm.GetVMSources();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                lvSources.ItemsSource = sourcesRes;
                loaderSources.Visibility = Visibility.Collapsed;
            }));
            var langsRes = DBComm.GetVMLanguages();
            var langRes = DBComm.Context.Languages.SingleOrDefault(l => l.Id == DBComm.GlobalUser.LastUsedLanguage);
            var vmLangRes = DBComm.FromLanguageToVMLanguage(langRes);
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                cbLanguages.ItemsSource = langsRes;
                Language lang = langRes;
                VMLanguage vmLang = vmLangRes;
                cbLanguages.SelectedValue = vmLang.Id;
            }));
            var tagsRes = DBComm.GetTags();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                itemsControlTags.ItemsSource = tagsRes;
                loaderTags.Visibility = Visibility.Collapsed;
            }));



            //this.Dispatcher.Invoke(() =>
            //{
            //    cbTypeOfTagFilter.ItemsSource = new List<string> { "Excluding filter", "Including filter" };
            //    cbTypeOfTagFilter.SelectedValue = "Excluding filter";
            //});
        }
        #endregion
        #region Filtering
        private ObservableCollection<VMWordUnit> GetFilteredByTypesOfUnit(ObservableCollection<VMWordUnit> initialUnits)
        {
            ObservableCollection<VMWordUnit> finalList = new ObservableCollection<VMWordUnit>();

            if (chbAllParts.IsChecked == true)
            {
                return initialUnits;
            }
            else
            {
                ObservableCollection<VMWordUnit> adverbs = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> collocations = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> sentences = new ObservableCollection<VMWordUnit>();

                foreach (var item in initialUnits)
                {
                    item.UnitTypes = DBComm.Context.WordUnits.SingleOrDefault(x => x.Id == item.Id).UnitTypes;
                    if (chbNouns.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Noun"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbAdjectives.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Adjective"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbVerbs.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Verb"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbInterjections.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Interjection"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbContractions.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Contraction"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbCollocations.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Collocation"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbSentences.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Sentence"))
                        {
                            finalList.Add(item);
                        }
                    }
                    if (chbMiscellaneous.IsChecked == true)
                    {
                        if (item.UnitTypes.Select(x => x.Name).Contains("Miscellaneous"))
                        {
                            finalList.Add(item);
                        }
                    }
                }
                finalList = new ObservableCollection<VMWordUnit>(finalList.Intersect(initialUnits));
                return finalList;
            }
        }
        private ObservableCollection<VMWordUnit> GetFilteredByText(string textInput, ObservableCollection<VMWordUnit> initialUnits)
        {
            if (textInput == "" || textInput == New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits)
            {
                return initialUnits;
            }
            else
            {
                List<VMWordUnit> finalList = new List<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByUnit = new ObservableCollection<VMWordUnit>();
                List<VMWordUnit> listByMeaning = new List<VMWordUnit>();
                List<VMWordUnit> listBySourceName = new List<VMWordUnit>();
                List<VMWordUnit> listByExample = new List<VMWordUnit>();
                List<VMWordUnit> listByNote = new List<VMWordUnit>();
                List<VMWordUnit> listByTags = new List<VMWordUnit>();
                bool containsSourceName = false;
                //for (int i = 0; i < initialUnits.Count; i++)
                //{
                //    WordUnit wu = DBComm.FromVMWordUnitToWordUnit(initialUnits[i]);
                //    DBComm.Context.Entry(wu).Collection(x => x.Tags);
                //}

                if (!searchSettings.SearchMeaning && !searchSettings.SearchSource && !searchSettings.SearchExample && !searchSettings.SearchNote)
                {
                    return listByUnit;
                }
                Parallel.For(0, initialUnits.Count(), i =>
                {
                    if (initialUnits[i].ContentOfUnit == null)
                    {
                        initialUnits[i].ContentOfUnit = "";
                    }
                    if (initialUnits[i].Meaning == null)
                    {
                        initialUnits[i].Meaning = "";
                    }
                    if (initialUnits[i].Example == null)
                    {
                        initialUnits[i].Example = "";
                    }
                    if (initialUnits[i].Note == null)
                    {
                        initialUnits[i].Note = "";
                    }
                    int sourceId = initialUnits[i].SourceId;
                    bool containsUnit = initialUnits[i].ContentOfUnit.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool containsMeaning = initialUnits[i].Meaning.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                    using (var context = new MyOwnDictionaryContext())
                        containsSourceName = context.Sources.SingleOrDefault(x => x.Id == sourceId).Name.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool containsExample = initialUnits[i].Example.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool containsNote = initialUnits[i].Note.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool containsTag = false;

                    foreach (var tag in initialUnits[i].Tags)
                    {
                        if (containsTag == false)
                        {
                            containsTag = tag.Name.IndexOf(textInput, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                    }

                    if (containsUnit) // obligatory element of search
                    {
                        listByUnit.Add(initialUnits[i]);
                    }

                    if (searchSettings.SearchMeaning == true)
                    {
                        if (containsMeaning)
                        {
                            listByMeaning.Add(initialUnits[i]);
                        }
                    }

                    if (searchSettings.SearchSource == true)
                    {
                        if (containsSourceName)
                        {
                            listBySourceName.Add(initialUnits[i]);
                        }
                    }

                    if (searchSettings.SearchExample == true)
                    {
                        if (containsExample)
                        {
                            listByExample.Add(initialUnits[i]);
                        }
                    }

                    if (searchSettings.SearchNote == true)
                    {
                        if (containsNote)
                        {
                            listByNote.Add(initialUnits[i]);
                        }
                    }

                    if (searchSettings.SearchTags == true)
                    {
                        if (containsTag)
                        {
                            listByTags.Add(initialUnits[i]);
                        }
                    }

                }
                );


                if (searchSettings.SearchMeaning == true)
                {
                    foreach (var p in listByUnit.Union(listByMeaning))
                    {
                        finalList.Add(p);
                    }
                }
                if (searchSettings.SearchSource == true)
                {
                    foreach (var p in listByUnit.Union(listBySourceName))
                    {
                        finalList.Add(p);
                    }
                }
                if (searchSettings.SearchExample == true)
                {
                    foreach (var p in listByUnit.Union(listByExample))
                    {
                        finalList.Add(p);
                    }
                }
                if (searchSettings.SearchNote == true)
                {
                    foreach (var p in listByUnit.Union(listByNote))
                    {
                        finalList.Add(p);
                    }
                }
                if (searchSettings.SearchTags == true)
                {
                    foreach (var p in listByTags.Union(listByNote))
                    {
                        finalList.Add(p);
                    }
                }

                return new ObservableCollection<VMWordUnit>(finalList.Distinct());
            }
        }
        private ObservableCollection<VMWordUnit> GetFilteredByTags(ObservableCollection<VMWordUnit> initialUnits)
        {
            if (AllTagsChecked())
            {
                return initialUnits;
            }
            else
            {
                List<VMWordUnit> finalList = new List<VMWordUnit>();
                EqualityComparer ec = new EqualityComparer();
                for (int i = 0; i < itemsControlTags.Items.Count; i++)
                {
                    var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(itemsControlTags.Items[i]) as FrameworkElement;
                    var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                    Tag tag = (Tag)checkBox.DataContext;
                    if (checkBox.IsChecked == true)
                    {
                        if (cbTypeOfTagFilter.SelectedValue.ToString().Contains("Including"))
                        {
                            List<WordUnit> allWords = new List<WordUnit>(tag.WordUnits);
                            finalList.AddRange(DBComm.ConvertToVMWordUnits(allWords));
                        }
                        else if (cbTypeOfTagFilter.SelectedValue.ToString().Contains("Excluding"))
                        {
                            List<WordUnit> allWords = new List<WordUnit>(tag.WordUnits);
                            if (finalList.Count == 0)
                            {
                                finalList.AddRange(DBComm.ConvertToVMWordUnits(allWords));
                            }
                            else
                            {
                                var list = finalList;
                                for (int j = 0; j < list.ToList().Count; j++)
                                {
                                    if (finalList[j].Tags.Contains(tag) == false)
                                    {
                                        finalList.Remove(list[j]);
                                    }
                                }
                            }
                        }
                    }
                }
                var final = new ObservableCollection<VMWordUnit>(finalList.Intersect(initialUnits, ec).Distinct());
                return final;
            }
        }
        private ObservableCollection<VMWordUnit> GetFilteredBySource(ObservableCollection<VMWordUnit> initialUnits)
        {
            ObservableCollection<VMWordUnit> finalList = new ObservableCollection<VMWordUnit>();
            for (int i = 0; i < initialUnits.Count; i++)
            {
                if (initialUnits[i].SourceId == ((VMSource)lvSources.SelectedItem).Id)
                {
                    finalList.Add(initialUnits[i]);
                }
            }
            finalList = new ObservableCollection<VMWordUnit>(finalList.Intersect(initialUnits).Distinct());
            return finalList;
        }
        private ObservableCollection<VMWordUnit> GetFilteredByEverything(string textInput, Language lang)
        {
            ObservableCollection<VMWordUnit> InitialUnits = DBComm.GetVMWordUnits(lang.Id);

            if (InitialUnits.Count() != 0)
            {
                InitialUnits = GetFilteredByText(textInput, InitialUnits);
                InitialUnits = GetFilteredByTypesOfUnit(InitialUnits);
                InitialUnits = GetFilteredByTags(InitialUnits);
                if (SelectAll.IsSelected == false)
                {
                    if (lvSources.SelectedItems.Count != 0)
                    {
                        InitialUnits = GetFilteredBySource(InitialUnits);
                    }
                }
            }

            return InitialUnits;
        }
        #endregion

        private void CheckSearchInput(TextBox tb, string input)
        {
            int Checks = 0;
            if (tb.Text == input)
            {
                tb.Text = "";
                Checks++;
            }
            int counter = 0;
            counter = Regex.Matches(tb.Text, @"[a-zA-Z]").Count;
            if (counter == 0 && Checks == 0 && tb.IsFocused == false)
            {
                tb.Text = input;
            }
        }
        //private static void SortDataGrid(DataGrid dataGrid, int columnIndex = 1, ListSortDirection sortDirection = ListSortDirection.Ascending)
        //{
        //    var column = dataGrid.Columns[columnIndex];

        //    // Clear current sort descriptions
        //    dataGrid.Items.SortDescriptions.Clear();

        //    // Add the new sort description
        //    dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

        //    // Apply sort
        //    foreach (var col in dataGrid.Columns)
        //    {
        //        col.SortDirection = null;
        //    }
        //    column.SortDirection = sortDirection;

        //    // Refresh items to display sort
        //    dataGrid.Items.Refresh();
        //}
        private static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }
        private void OperateOnWordUnitSelectionChanged()
        {
            try
            {
                tbExample.Text = ((VMWordUnit)dgWordUnits.SelectedItem).Example;
                tbNote.Text = ((VMWordUnit)dgWordUnits.SelectedItem).Note;
                tbExample.IsEnabled = true;
                tbNote.IsEnabled = true;
                btnSaveExample.Visibility = Visibility.Visible;
                btnSaveNote.Visibility = Visibility.Visible;
            }
            catch (Exception) { }
        }
        private bool AllTagsChecked()
        {
            for (int i = 0; i < itemsControlTags.Items.Count; i++)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(itemsControlTags.Items[i]) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;
                if (checkBox.IsChecked == false)
                {
                    return false;
                }
            }
            return true;
        }

        private void CaseAddItem()
        {
            CurrentSourceFromList = lvSources.SelectedIndex;
            UIActions.OpenWindowWithAnimation(this, new AddSource());
            DisableFindButtons();
            lvSources.ItemsSource = DBComm.GetVMSources();
            lvSources.SelectedIndex = CurrentSourceFromList;

            if (iconAllChecked.Visibility == Visibility.Visible)
            {
                lvInitializedItems.SelectedIndex = 2;
            }
        }
        private void CaseSelectAll()
        {
            iconAllChecked.Visibility = Visibility.Visible;
        }

        private void CbTypeOfTagFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDictionaryList();
        }

        private void BtnEditSource_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnListViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            CurrentSourceForContextMenu = item.Content as VMSource;
            e.Handled = true;
        }

        private void contextEditClicked(object sender, RoutedEventArgs e)
        {
            EditSource editSource = new EditSource(CurrentSourceForContextMenu);
            editSource.ShowDialog();
            //if (wu.ContentOfUnit != null && wu.Meaning != null)
            //{
            //    wu.TagsToString();
            //    DictionaryUnits[index] = wu;
            //    dgWordUnits.ItemsSource = null;
            //    DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.OrderByDescending(x => x.Datetime));
            //    dgWordUnits.ItemsSource = DictionaryUnits;
            //    DisableFindButtons();
            //}
            //if (itemsControlTags.Items.Count != DBComm.GetTags().Count)
            //{
            //    itemsControlTags.ItemsSource = DBComm.GetTags();
            //}
        }
    }
}
