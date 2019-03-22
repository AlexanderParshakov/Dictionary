using New_designed_Dictionary.Customize_Interface;
using New_designed_Dictionary.Modals;
using New_designed_Dictionary.ViewModels;
using Syn.WordNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using Word2vec.Tools;

namespace New_designed_Dictionary
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region GlobalDeclarations

        static MyOwnDictionaryContext Context = new MyOwnDictionaryContext();
        static Vocabulary vectorVocabulary = null;
        static int CurrentSourceFromList = 0;

        #endregion 
        #region Animation

        private void AnimateOpacity(double from, double to, double timespan)
        {
            DoubleAnimation animation = new DoubleAnimation(from, to,
                                   (Duration)TimeSpan.FromSeconds(timespan));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private static void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
        {
            if (cell == null || cell.IsEditing || cell.IsReadOnly) return;

            DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null)
                return;

            if (!cell.IsFocused)
            {
                cell.Focus();
            }


            DataGridRow row = FindVisualParent<DataGridRow>(cell);
            if (row != null && !row.IsSelected)
            {
                row.IsSelected = true;
            }

        }
        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
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
            return Total;
        }
        private void SetCheck(bool AllParts)
        {
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
            }
            if (AllParts == false && Total == 35)
            {
                chbAdjectives.IsChecked = false;
                chbCollocations.IsChecked = false;
                chbInterjections.IsChecked = false;
                chbNouns.IsChecked = false;
                chbContractions.IsChecked = false;
                chbVerbs.IsChecked = false;
                chbSentences.IsChecked = false;
            }
        }
        private void CheckOtherPartsOfSpeech()
        {
            int Total = CheckBoxUidSummary();
            if (Total >= 35)
            {
                chbAllParts.IsChecked = true;
            }
            if (Total < 35)
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
                foreach(string syn in synSet.Words)
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
            List<string> list = GetSimilars(word);
            List<VMWordUnit> wordUnits = new List<VMWordUnit>();

            foreach (var item in list)
            {
                VMWordUnit wu = new VMWordUnit { ContentOfUnit = item };
                wordUnits.Add(wu);
            }
            itemsControlSimilars.ItemsSource = wordUnits.Distinct();
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
            chbAllParts.IsChecked = true;
            DefineItemSources();
            tbSearchUnits.TextChanged += tbSearchUnits_TextChanged;
            vectorVocabulary = new Word2VecBinaryReader().Read(New_designed_Dictionary.Resources.Paths.Word2Vec_bin_file);
        }

        public MainWindow()
        {
            InitializeComponent();
            Initiar();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            AnimateOpacity(0, 1, 0.5);
        }

        #endregion
        #region Events

        private void gridAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnOpenList_Click(object sender, RoutedEventArgs e)
        {
            OpenSourceMenu();
        }

        private void btnCloseList_Click(object sender, RoutedEventArgs e)
        {
            CollapseSourceMenu();
        }

        private void tbSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearch, New_designed_Dictionary.Resources.Literals.Placeholder_SearchSources);
        }

        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearch, New_designed_Dictionary.Resources.Literals.Placeholder_SearchSources);
        }

        #region CheckBoxes to Parts of Speech

        private void chbAllParts_Checked(object sender, RoutedEventArgs e)
        {
            SetCheck((bool)chbAllParts.IsChecked);
        }

        private void chbAllParts_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheck((bool)chbAllParts.IsChecked);
        }

        private void chbNouns_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbVerbs_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbAdjectives_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbContractions_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbCollocations_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbInterjections_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbNouns_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbVerbs_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbAdjectives_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbContractions_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbCollocations_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbInterjections_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        #endregion

        private void btnThemes_Click(object sender, RoutedEventArgs e)
        {
            ThemeGallery tg = new ThemeGallery();
            tg.ShowDialog();
        }

        private void lvSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CustomView.ChangeCheckVisibility(lvSources, "iconChecked");
            if (iconAllChecked.Visibility == Visibility.Visible)
                lvInitializedItems.UnselectAll();
            iconAllChecked.Visibility = Visibility.Hidden;
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

        private void chbSentence_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }

        private void chbSentence_Checked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
        }


        private void LiBoxWordUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cbWordSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string NewSource = ((ComboBox)sender).SelectedValue.ToString();
                string Word = ((WordUnit)dgWordUnits.SelectedItem).ContentOfUnit;
                OntologyProcessor.UpdateGraph(Word, "IsProvidedBy", NewSource.Replace(" ", "_"), "Provides");
                //ChangeSource(NewSourceId, WordId);
            }
            catch (Exception exc) { string str = exc.ToString(); }
        }

        private void lvWordUnits_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void lvWordUnits_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void cbWordSources_GotFocus(object sender, RoutedEventArgs e)
        {
            ListViewItem lvi = GetAncestorByType(e.OriginalSource as DependencyObject, typeof(ListViewItem)) as ListViewItem;
            if (lvi != null)
            {
                dgWordUnits.SelectedIndex = dgWordUnits.ItemContainerGenerator.IndexFromContainer(lvi);
            }
        }

        private void tbExample_GotFocus(object sender, RoutedEventArgs e)
        {
            tbExample.SelectionStart = tbExample.Text.Length; // add some logic if length is 0
            tbExample.SelectionLength = 0;
        }

        private void btnSaveExample_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OntologyProcessor.UpdateGraph(
                    ((WordUnit)dgWordUnits.SelectedItem).ContentOfUnit,
                    "HasExample", tbExample.Text.Replace(" ", "_"),
                    "Exemplifies"
                    );

                ((VMWordUnit)dgWordUnits.SelectedItem).Example = tbExample.Text;
                lbSavedExample.Text = "Saved successfully!";
            }
            catch (Exception ex) { lbSavedExample.Text = ex.ToString(); }
        }

        private void btnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OntologyProcessor.UpdateGraph(
                    ((WordUnit)dgWordUnits.SelectedItem).ContentOfUnit,
                    "HasNote", tbNote.Text.Replace(" ", "_"),
                    "IsNoteOf"
                    );

                ((VMWordUnit)dgWordUnits.SelectedItem).Note = tbNote.Text;
                lbSavedNote.Text = "Saved successfully!";
            }
            catch (Exception ex) { lbSavedNote.Text = ex.ToString(); }

        }

        private void tbNote_GotFocus(object sender, RoutedEventArgs e)
        {
            tbNote.SelectionStart = tbNote.Text.Length; // add some logic if length is 0
            tbNote.SelectionLength = 0;
        }

        private void btnAddWordUnit_Click(object sender, RoutedEventArgs e)
        {
            AddWordUnit addWindow = new AddWordUnit();
            AnimateOpacity(1, 0.5, 1.5);
            addWindow.Owner = this;
            addWindow.ShowInTaskbar = false;
            addWindow.ShowDialog();
            Show();
            AnimateOpacity(0.5, 1, 0.5);
        }

        private void btnSearchUnits_Click(object sender, RoutedEventArgs e)
        {
            dgWordUnits.ItemsSource = GetFilteredList(tbSearchUnits.Text);
            SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
        }

        private void tbSearchUnits_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }

        private void tbSearchUnits_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }

        private void tbSearchUnits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                dgWordUnits.ItemsSource = GetFilteredList(tbSearchUnits.Text);
                SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
            }
        }

        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SearchSettings settings = new SearchSettings();
            AnimateOpacity(1, 0.5, 1.5);
            settings.Owner = this;
            settings.ShowDialog();
            Show();
            AnimateOpacity(0.5, 1, 0.5);
        }


        private void tbSearchUnits_TextChanged(object sender, TextChangedEventArgs e)
        {
            dgWordUnits.ItemsSource = GetFilteredList(tbSearchUnits.Text);
            SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
        }

        private void rbSphereOfUsage_Checked(object sender, RoutedEventArgs e)
        {

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
        #region SourceMenu

        private void CollapseSourceMenu()
        {
            btnOpenList.Visibility = Visibility.Visible;
            btnCloseList.Visibility = Visibility.Collapsed;
            btnCloseList.Width = 50;
            lbHideSourceMenu.Visibility = Visibility.Collapsed;
            lbOpenSourceMenu.Visibility = Visibility.Visible;
            gridSearch.Width = 50;
            tbSearch.Visibility = Visibility.Collapsed;
            lvSources.Visibility = Visibility.Collapsed;
            lvInitializedItems.Visibility = Visibility.Collapsed;
        }
        private void OpenSourceMenu()
        {
            btnOpenList.Visibility = Visibility.Collapsed;
            btnCloseList.Visibility = Visibility.Visible;
            btnCloseList.Width = 210;
            lbHideSourceMenu.Visibility = Visibility.Visible;
            lbOpenSourceMenu.Visibility = Visibility.Collapsed;
            gridSearch.Width = 210;
            tbSearch.Visibility = Visibility.Visible;
            lvSources.Visibility = Visibility.Visible;
            lvInitializedItems.Visibility = Visibility.Visible;
        }

        #endregion SourceMenu
        #region Bind Items to Controls

        

        private ObservableCollection<VMSource> GetSources()
        {
            ObservableCollection<VMSource> sources = new ObservableCollection<VMSource>();
            for (int i = 0; i < Context.Sources.Count(); i++)
            {
                sources.Add
                    (new VMSource

                    {
                        Id = Context.Sources.ToList()[i].Id,
                        Name = Context.Sources.ToList()[i].Name,
                        ImageData = ToImage(Context.Sources.ToList()[i].Picture)
                    }
                    );
            }
            return sources;
        }

        private ObservableCollection<VMWordUnit> GetWordUnits()
        {
            ObservableCollection<VMWordUnit> VMWordUnits = new ObservableCollection<VMWordUnit>();
            object results = OntologyProcessor.GetIndividualQueryResults(New_designed_Dictionary.Resources.Queries.Query_Indiv_WordUnit);

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset.Results)
                {
                    VMWordUnit wu = WordUnit.WordUnitFromQueryResult(r);
                    VMWordUnits.Add(wu);
                }
            }


            return VMWordUnits;
        }
        public static void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            var column = dataGrid.Columns[columnIndex];

            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();

            // Add the new sort description
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            // Apply sort
            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = sortDirection;

            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        private void DefineItemSources()
        {
            lvSources.ItemsSource = GetSources();
            dgWordUnits.ItemsSource = GetWordUnits();
            SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
            lvSpheres.ItemsSource = OntologyProcessor.GetClassTags(OntologyProcessor.GetIndividuals(New_designed_Dictionary.Resources.Queries.Query_Indiv_Tags), true);
        }

        #endregion

        private static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }
        
        private List<Theme> GetThemes()
        {
            List<Theme> themes = new List<Theme>();
            themes = Context.Themes.ToList();
            return themes;
        }

        private BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        
        private List<object> JoinedCollections(ObservableCollection<VMWordUnit> wus, ObservableCollection<VMSource> sources)
        {
            List<object> final = new List<object>();
            final.Add(wus);
            final.Add(sources);
            return final;
        }
        private List<string> SourceNames()
        {
            List<string> list = new List<string>();
            for (int j = 0; j < Context.Sources.ToList().Count(); j++) // get all sources
            {
                list.Add(Context.Sources.ToList()[j].Id + ". " + Context.Sources.ToList()[j].Name);
            }
            return list;
        }
        
        



        
        

        
        

        private WordUnit GetInstance(int Id)
        {
            try
            {
                WordUnit wu = GetWordUnits().Single(x => x.Id == Id);
                return wu;
            }
            catch (Exception)
            {

            }
            return null;
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
        
        private void CaseAddItem()
        {
            CurrentSourceFromList = lvSources.SelectedIndex;
            AddSource addWindow = new AddSource();
            AnimateOpacity(1, 0.5, 1.5);
            addWindow.ShowDialog();
            Show();
            AnimateOpacity(0.5, 1, 0.5);
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

        private ObservableCollection<VMWordUnit> GetFilteredList(string input)
        {
            ObservableCollection<VMWordUnit> InitialUnits = GetWordUnits();
            if (input == "" || input == New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits)
            {
                return InitialUnits;
            }
            ObservableCollection<VMWordUnit> FilteredUnits = new ObservableCollection<VMWordUnit>();
            foreach (var item in InitialUnits)
            {
                if (item.ContentOfUnit == null)
                {
                    item.ContentOfUnit = "";
                }
                if (item.Meaning == null)
                {
                    item.Meaning = "";
                }
                if (item.Example == null)
                {
                    item.Example = "";
                }
                if (item.Note == null)
                {
                    item.Note = "";
                }
                if (item.SourceName == null)
                {
                    item.SourceName = "";
                }
                bool containsUnit = item.ContentOfUnit.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0;
                bool containsMeaning = item.Meaning.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0;
                bool containsSourceName = item.SourceName.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0;
                bool containsExample = item.Example.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0;
                bool containsNote = item.Note.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0;

                if (containsUnit || containsMeaning || containsSourceName || containsExample || containsNote)
                {
                    if (item.SourceName == "")
                    {
                        item.SourceName = null;
                    }
                    FilteredUnits.Add(item);
                }
            }
            //for (int i = 0; i < filteredunits.count; i++)
            //{
                
            //}

            return FilteredUnits;
        }

        private void dgWordUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void dgWordUnits_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindSynonyms_Click(object sender, RoutedEventArgs e)
        {
            ShowSynonyms(((VMWordUnit)dgWordUnits.SelectedItem).ContentOfUnit);
            btnFindSynonyms.Visibility = Visibility.Collapsed;
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
            OntologyProcessor.UpdateGraph(Word, "HasMeaning", NewMeaning.Replace(" ", "_"), "IsMeaningOf");

            //Debug.WriteLine(t.Text .ToString());
        }

        private void btnFindSeeAlso_Click(object sender, RoutedEventArgs e)
        {
            //List<string> list = GetAnalogies("Strong", "Strength", "Intelligent");
            ShowSimilars(((VMWordUnit)dgWordUnits.SelectedItem).ContentOfUnit);
        }
    }
}
