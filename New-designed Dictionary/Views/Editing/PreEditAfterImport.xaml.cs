using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.Modals;
using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDS.RDF.Query;
using Excel = Microsoft.Office.Interop.Excel;

namespace New_designed_Dictionary.Import_and_Export
{
    /// <summary>
    /// Interaction logic for PreEditAfterImport.xaml
    /// </summary>
    public partial class PreEditAfterImport : System.Windows.Window
    {
        static string Filepath = "";
        static int rowCount = 0;
        public bool ImportDone { get; set; }
        static bool InsertingIntoBase = false;
        ObservableCollection<VMWordUnit> DictionaryUnits = new ObservableCollection<VMWordUnit>();

        Excel.Application xlApp;
        Excel.Workbook xlWorkbook;
        Excel._Worksheet xlWorksheet;
        Excel.Range xlRange;


        private static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;
            if (element.GetType() == type) return element;
            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }
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
        private void CheckSearchInput(TextBox tb, string input)
        {
            int Checks = 0;
            if (tb.Text == input)
            {
                tb.Text = "";
                Checks++;
            }
            int counter = 0;
            counter = Regex.Matches(tb.Text, @"[a-zA-Z]|[а-яА-Я]").Count;
            if (counter == 0 && Checks == 0 && tb.IsFocused == false)
            {
                tb.Text = input;
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
                ObservableCollection<VMWordUnit> finalList = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByUnit = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByMeaning = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listBySourceName = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByExample = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByNote = new ObservableCollection<VMWordUnit>();
                ObservableCollection<VMWordUnit> listByTags = new ObservableCollection<VMWordUnit>();
                bool containsSourceName = false;

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
                    if (containsMeaning)
                    {
                        listByMeaning.Add(initialUnits[i]);
                    }
                    if (containsSourceName)
                    {
                        listBySourceName.Add(initialUnits[i]);
                    }
                    if (containsExample)
                    {
                        listByExample.Add(initialUnits[i]);
                    }
                    if (containsNote)
                    {
                        listByNote.Add(initialUnits[i]);
                    }
                    if (containsTag)
                    {
                        listByTags.Add(initialUnits[i]);
                    }

                }
                );

                foreach (var p in listByUnit.Union(listByMeaning))
                {
                    finalList.Add(p);
                }
                foreach (var p in listByUnit.Union(listBySourceName))
                {
                    finalList.Add(p);
                }
                foreach (var p in listByUnit.Union(listByExample))
                {
                    finalList.Add(p);
                }
                foreach (var p in listByUnit.Union(listByNote))
                {
                    finalList.Add(p);
                }
                foreach (var p in listByTags.Union(listByNote))
                {
                    finalList.Add(p);
                }
                finalList = new ObservableCollection<VMWordUnit>(finalList.Distinct());
                return finalList;
            }
        }
        public PreEditAfterImport(string filepath)
        {
            InitializeComponent();
            var cbs = FindVisualChildren<ComboBox>(dgWordUnits);
            Filepath = filepath;
            InsertingIntoBase = false;

            xlApp = new Excel.Application();
            xlWorkbook = xlApp.Workbooks.Open(Filepath);
            xlWorksheet = xlWorkbook.Sheets[1];
            xlRange = xlWorksheet.UsedRange;

            rowCount = xlRange.Rows.Count;

            pbUnitsImported.Maximum = rowCount;

            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (InsertingIntoBase == false)
            {
                pbUnitsImported.Value = e.ProgressPercentage;
                lbUnitsLoaded.Text = e.ProgressPercentage.ToString("## ###") + "/" + rowCount.ToString("## ###") + " units loaded...";
            }
            else
            {
                pbUnitsImported.Value = e.ProgressPercentage;
                lbUnitsLoaded.Text = e.ProgressPercentage.ToString("## ###") + "/" + DictionaryUnits.Count.ToString("## ###") + " units imported...";
            }
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (InsertingIntoBase == false)
            {
                var AllSources = DBComm.GetSources();

                worker.ReportProgress(0, "0 processed out of " + rowCount + "...");
                for (int i = 1; i <= rowCount; i++)
                {
                    string firstLanguage = xlRange.Cells[i, 1].Value2;
                    string secondLanguage = xlRange.Cells[i, 2].Value2;
                    VMWordUnit wu = new VMWordUnit();
                    if (firstLanguage == "english" || firstLanguage == "английский")
                    {
                        if (secondLanguage == "russian" || secondLanguage == "русский")
                        {
                            wu.ContentOfUnit = xlRange.Cells[i, 3].Value2;
                            wu.Meaning = xlRange.Cells[i, 4].Value2;
                        }
                    }
                    else if (firstLanguage == "russian" || firstLanguage == "русский")
                    {
                        if (secondLanguage == "english" || secondLanguage == "английский")
                        {
                            wu.ContentOfUnit = xlRange.Cells[i, 4].Value2;
                            wu.Meaning = xlRange.Cells[i, 3].Value2;
                        }
                    }

                    wu.Sources = AllSources;
                    DictionaryUnits.Add(wu);
                    worker.ReportProgress(i, i + " processed out of " + rowCount + "...");
                }
            }
            else
            {
                worker.ReportProgress(0, "0 imported out of " + DictionaryUnits.Count + "...");
                int rows = DictionaryUnits.Count;
                for (int i = 0; i < DictionaryUnits.Count; i++)
                {
                    if (DictionaryUnits[i].UnitTypes.Count == 0)
                    {
                        DictionaryUnits[i].UnitTypes.Add(DBComm.Context.UnitTypes.SingleOrDefault(u => u.Name == "Miscellaneous"));
                    }
                    if (DictionaryUnits[i].Tags.Count == 0)
                    {
                        DictionaryUnits[i].Tags.Add(DBComm.Context.Tags.SingleOrDefault(u => u.Name == "Non-specified"));
                    }
                    if (DictionaryUnits[i].SourceId == 0)
                    {
                        DictionaryUnits[i].SourceId = DBComm.Context.Sources.SingleOrDefault(u => u.Name == "Non-specified").Id;
                    }
                    AddUnit(DictionaryUnits[i]);
                    worker.ReportProgress(i, i + " imported out of " + rows + "...");
                }
            }
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (InsertingIntoBase == false)
            {
                dgWordUnits.ItemsSource = DictionaryUnits;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);

                lbUnitsLoaded.Visibility = Visibility.Collapsed;
                pbUnitsImported.Visibility = Visibility.Collapsed;
                dgWordUnits.Visibility = Visibility.Visible;
                tbSearchUnits.Visibility = Visibility.Visible;
                btnSearchUnits.Visibility = Visibility.Visible;
                btnOK.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Dictionary units have been successfully loaded.");
                this.Close();
            }
        }
        private void AddUnit(VMWordUnit wu)
        {
            // clearing placeholders if textboxes received no input
            if (wu.ContentOfUnit == New_designed_Dictionary.Resources.Literals.Placeholder_Contents || wu.ContentOfUnit == null)
            {
                wu.ContentOfUnit = "";
            }
            if (wu.Meaning == New_designed_Dictionary.Resources.Literals.Placeholder_Meaning || wu.Meaning == null)
            {
                wu.Meaning = "";
            }
            if (wu.Example == New_designed_Dictionary.Resources.Literals.Placeholder_Example || wu.Example == null)
            {
                wu.Example = "";
            }
            if (wu.Note == New_designed_Dictionary.Resources.Literals.Placeholder_Note || wu.Note == null)
            {
                wu.Note = "";
            }
            WordUnit wordUnit = DBComm.FromVMWordUnitToWordUnit(wu);

            wordUnit.Sources.Add(DBComm.Context.Sources.SingleOrDefault(s => s.Id == wu.SourceId));
            wordUnit.Tags = wu.Tags;
            wordUnit.UnitTypes = wu.UnitTypes;
            wordUnit.Users.Add(DBComm.Context.Users.SingleOrDefault(s => s.Login == DBComm.GlobalUser.Login));
            DBComm.AddWordUnit(wordUnit);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ImportPreparation importPreparation = new ImportPreparation();
            this.Hide();
            importPreparation.ShowDialog();
            this.Close();
        }
        private void dgWordUnits_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TextBlock t = e.EditingElement as TextBlock;
            DataGridColumn dgc = e.Column;
            DataGridRow dgr = e.Row;
            VMWordUnit wu = (VMWordUnit)dgr.Item;

            if (dgc.DisplayIndex == 1)
            {
                var textbox = dgWordUnits.FindAllVisualDescendants()
        .Where(elt => elt.Name == "tbDictionaryUnit")
        .OfType<TextBox>()
        .FirstOrDefault();
                wu.ContentOfUnit = textbox.Text;
            }
            if (dgc.DisplayIndex == 2)
            {
                var textbox = dgWordUnits.FindAllVisualDescendants()
        .Where(elt => elt.Name == "tbMeaning")
        .OfType<TextBox>()
        .FirstOrDefault();
                wu.Meaning = textbox.Text;
            }

            int index = dgWordUnits.SelectedIndex;
            if (wu.ContentOfUnit != null && wu.Meaning != null)
            {
                DictionaryUnits[index] = wu;
                dgWordUnits.ItemsSource = null;
                dgWordUnits.ItemsSource = DictionaryUnits;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            InsertingIntoBase = true;
            //CutExistingWords();
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();

            pbUnitsImported.Value = 0;
            pbUnitsImported.Maximum = DictionaryUnits.Count;

            lbUnitsLoaded.Visibility = Visibility.Visible;
            pbUnitsImported.Visibility = Visibility.Visible;
            dgWordUnits.Visibility = Visibility.Collapsed;
            tbSearchUnits.Visibility = Visibility.Collapsed;
            btnSearchUnits.Visibility = Visibility.Collapsed;

            this.ImportDone = true;
        }

        private void tbSearchUnits_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }

        private void tbSearchUnits_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbSearchUnits, New_designed_Dictionary.Resources.Literals.Placeholder_SearchWordUnits);
        }

        private void tbSearchUnits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RefreshDictionaryUnits();
            }
        }
        private void btnSearchUnits_Click(object sender, RoutedEventArgs e)
        {
            RefreshDictionaryUnits();
        }

        private void RefreshDictionaryUnits()
        {
            dgWordUnits.ItemsSource = GetFilteredByText(tbSearchUnits.Text, DictionaryUnits);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            VMWordUnit wu = (VMWordUnit)dgWordUnits.SelectedItem;
            EditDictionaryUnit editDictionaryUnit = new EditDictionaryUnit(wu, false);
            int index = dgWordUnits.SelectedIndex;
            editDictionaryUnit.Unit += value => wu = value;
            editDictionaryUnit.ShowDialog();
            if (wu.ContentOfUnit != null && wu.Meaning != null)
            {
                DictionaryUnits[index] = wu;
                dgWordUnits.Items.Refresh();
                //dgWordUnits.ItemsSource = null;
                //dgWordUnits.ItemsSource = DictionaryUnits;
            }
        }

        private void btnDeleteUnit_Click(object sender, RoutedEventArgs e)
        {
            DictionaryUnits.Remove((VMWordUnit)dgWordUnits.SelectedItem);
            dgWordUnits.Items.Refresh();
        }

    }
}
