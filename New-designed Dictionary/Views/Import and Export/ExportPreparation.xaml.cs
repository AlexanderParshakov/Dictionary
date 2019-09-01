using Microsoft.Win32;
using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.HelperClasses.Customize_Interface;
using New_designed_Dictionary.Modals;
using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace New_designed_Dictionary.Import_and_Export
{
    /// <summary>
    /// Interaction logic for ExportPreparation.xaml
    /// </summary>
    public partial class ExportPreparation : Window
    {
        static ObservableCollection<VMWordUnit> DictionaryUnits = new ObservableCollection<VMWordUnit>();
        static SearchSettingsClass searchSettings = new SearchSettingsClass();

        public ExportPreparation()
        {
            InitializeComponent();
            Initiar();
        }

        private void Initiar()
        {
            chbAllParts.IsChecked = true;
            DictionaryUnits = DBComm.GetVMWordUnits();
            dgWordUnits.ItemsSource = DictionaryUnits;
            SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
            itemsControlTags.ItemsSource = DBComm.GetTags(false);
        }
        private void ExportToExcel()
        {
            DictionaryUnits = GetFilteredByEverything(tbSearchUnits.Text);
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook workbook = app.Workbooks.Add((Type.Missing));
            Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
            worksheet = workbook.Sheets["Лист1"];

            worksheet = workbook.ActiveSheet;
            worksheet.Name = "Exported dictionary";
            int rowCount = DictionaryUnits.Count();
            int numberOfHeaders = 7;

            worksheet.get_Range("A1", "G" + rowCount).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            worksheet.get_Range("A1", "G" + rowCount).VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            // setting information on the export
            worksheet.Cells[1, 1] = "Exporting date:";
            worksheet.Cells[1, 1].Font.Bold = true;
            worksheet.Cells[1, 2] = DateTime.Now.ToString("d.m.yyyy");
            worksheet.Cells[1, 2].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
            worksheet.Cells[2, 1] = "Exporting time:";
            worksheet.Cells[2, 1].Font.Bold = true;
            worksheet.Cells[2, 2] = DateTime.Now.ToString("h:mm:ss tt");
            worksheet.Cells[2, 2].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;

            // setting headers
            worksheet.Cells[5, 1] = "Dictionary Unit";
            worksheet.Cells[5, 1].Font.Bold = true;
            worksheet.Cells[5, 2] = "Meaning";
            worksheet.Cells[5, 2].Font.Bold = true;
            worksheet.Cells[5, 3] = "Source";
            worksheet.Cells[5, 3].Font.Bold = true;
            worksheet.Cells[5, 4] = "Example";
            worksheet.Cells[5, 4].Font.Bold = true;
            worksheet.Cells[5, 5] = "Note";
            worksheet.Cells[5, 5].Font.Bold = true;
            worksheet.Cells[5, 6] = "Types";
            worksheet.Cells[5, 6].Font.Bold = true;
            worksheet.Cells[5, 7] = "Tags";
            worksheet.Cells[5, 7].Font.Bold = true;
            worksheet.Columns[2].ColumnWidth = 80;
            worksheet.Columns[4].ColumnWidth = 60;
            worksheet.Columns[5].ColumnWidth = 60;

            for (int i = 0; i < DictionaryUnits.Count; i++)
            {
                for (int j = 0; j < numberOfHeaders; j++)
                {
                    if (j == 0) // Dictionary Unit
                    {
                        worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].ContentOfUnit;
                        worksheet.Cells[i + 6, j + 1].WrapText = true;
                    }
                    if (j == 1) // Meaning
                    {
                        worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Meaning;
                        worksheet.Cells[i + 6, j + 1].WrapText = true;
                    }
                    if (j == 2) // Source
                    {
                        //worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].SourceName;
                        worksheet.Cells[i + 6, j + 1].WrapText = true;
                    }
                    if (j == 3) // Example
                    {
                        worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Example;
                        worksheet.Cells[i + 6, j + 1].WrapText = true;
                    }
                    if (j == 4) // Note
                    {
                        worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Note;
                        worksheet.Cells[i + 6, j + 1].WrapText = true;
                    }
                    if (j == 5) // Types
                    {
                        string allTypes = "";
                        for (int typeNumber = 0; typeNumber < DictionaryUnits[i].UnitTypes.Count; typeNumber++)
                        {
                            allTypes += DictionaryUnits[i].UnitTypes.ToList()[typeNumber].Name + Environment.NewLine;
                            if (typeNumber == DictionaryUnits[i].UnitTypes.Count - 1)
                            {
                                allTypes = allTypes.Remove(allTypes.Length - 1);
                            }
                        }
                        worksheet.Cells[i + 6, j + 1] = allTypes;
                    }
                    if (j == 6) // Tags
                    {
                        string allTags = "";
                        for (int tagNumber = 0; tagNumber < DictionaryUnits[i].Tags.Count; tagNumber++)
                        {
                            allTags += DictionaryUnits[i].Tags.ToList()[tagNumber].Name + Environment.NewLine;
                            if (tagNumber == DictionaryUnits[i].Tags.Count - 1)
                            {
                                allTags = allTags.Remove(allTags.Length - 1);
                            }
                        }
                        worksheet.Cells[i + 6, j + 1] = allTags;
                    }
                }
            }
            worksheet.StandardWidth = 20;
            var SaveDialogue = new SaveFileDialog();
            SaveDialogue.FileName = tbTitle.Text;
            SaveDialogue.DefaultExt = ".xlsx";
            if (SaveDialogue.ShowDialog() == true)
            {
                workbook.SaveAs(SaveDialogue.FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                app.Quit();
                if (chbOpenAfterExport.IsChecked == true)
                {
                    Process.Start(SaveDialogue.FileName);
                    Process.GetCurrentProcess().Close();
                }
            }
            app.Quit();
        }
        private void ExportToExcelGroupedBySource()
        {
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook workbook = app.Workbooks.Add((Type.Missing));
            //List<string> sources = OntologyProcessor.GetIndividuals(New_designed_Dictionary.Resources.Queries.Query_Indiv_Sources);
            //for (int s = 0; s < sources.Count; s++)
            //{
            //    DictionaryUnits = GetFilteredByEverything(tbSearchUnits.Text);
            //    DictionaryUnits = new ObservableCollection<VMWordUnit>(DictionaryUnits.Where(x => x.SourceName == sources[s]));
            //    if (DictionaryUnits.Count() == 0)
            //    {
            //        continue;
            //    }
            //    Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
            //    workbook.Worksheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
            //    worksheet = workbook.Sheets[workbook.Sheets.Count];

            //    worksheet = workbook.ActiveSheet;

            //    worksheet.Name = StringWithoutStopChars(sources[s]);
            //    int rowCount = DictionaryUnits.Count();
            //    int numberOfHeaders = 7;

            //    worksheet.get_Range("A1", "G" + rowCount + 6).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            //    worksheet.get_Range("A1", "G" + rowCount + 6).VerticalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //    // setting information on the export
            //    worksheet.Cells[1, 1] = "Exporting date:";
            //    worksheet.Cells[1, 1].Font.Bold = true;
            //    worksheet.Cells[1, 2] = DateTime.Now.ToString("d.m.yyyy");
            //    worksheet.Cells[1, 2].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
            //    worksheet.Cells[2, 1] = "Exporting time:";
            //    worksheet.Cells[2, 1].Font.Bold = true;
            //    worksheet.Cells[2, 2] = DateTime.Now.ToString("h:mm:ss tt");
            //    worksheet.Cells[2, 2].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;

            //    // setting headers
            //    worksheet.Cells[5, 1] = "Dictionary Unit";
            //    worksheet.Cells[5, 1].Font.Bold = true;
            //    worksheet.Cells[5, 2] = "Meaning";
            //    worksheet.Cells[5, 2].Font.Bold = true;
            //    worksheet.Cells[5, 3] = "Source";
            //    worksheet.Cells[5, 3].Font.Bold = true;
            //    worksheet.Cells[5, 4] = "Example";
            //    worksheet.Cells[5, 4].Font.Bold = true;
            //    worksheet.Cells[5, 5] = "Note";
            //    worksheet.Cells[5, 5].Font.Bold = true;
            //    worksheet.Cells[5, 6] = "Types";
            //    worksheet.Cells[5, 6].Font.Bold = true;
            //    worksheet.Cells[5, 7] = "Tags";
            //    worksheet.Cells[5, 7].Font.Bold = true;
            //    worksheet.Columns[2].ColumnWidth = 70;
            //    worksheet.Columns[4].ColumnWidth = 40;
            //    worksheet.Columns[5].ColumnWidth = 40;

            //    for (int i = 0; i < DictionaryUnits.Count; i++)
            //    {
            //        for (int j = 0; j < numberOfHeaders; j++)
            //        {
            //            if (j == 0) // Dictionary Unit
            //            {
            //                worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].ContentOfUnit;
            //                worksheet.Cells[i + 6, j + 1].WrapText = true;
            //            }
            //            if (j == 1) // Meaning
            //            {
            //                worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Meaning;
            //                worksheet.Cells[i + 6, j + 1].WrapText = true;
            //            }
            //            if (j == 2) // Source
            //            {
            //                worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].SourceName;
            //                worksheet.Cells[i + 6, j + 1].WrapText = true;
            //            }
            //            if (j == 3) // Example
            //            {
            //                worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Example;
            //                worksheet.Cells[i + 6, j + 1].WrapText = true;
            //            }
            //            if (j == 4) // Note
            //            {
            //                worksheet.Cells[i + 6, j + 1] = DictionaryUnits[i].Note;
            //                worksheet.Cells[i + 6, j + 1].WrapText = true;
            //            }
            //            if (j == 5) // Types
            //            {
            //                string allTypes = "";
            //                for (int typeNumber = 0; typeNumber < DictionaryUnits[i].PartsOfSpeech.Count; typeNumber++)
            //                {
            //                    allTypes += DictionaryUnits[i].PartsOfSpeech[typeNumber] + Environment.NewLine;
            //                    if (typeNumber == DictionaryUnits[i].PartsOfSpeech.Count - 1)
            //                    {
            //                        allTypes = allTypes.TrimEnd(Environment.NewLine.ToCharArray());
            //                    }
            //                }
            //                worksheet.Cells[i + 6, j + 1] = allTypes;
            //            }
            //            if (j == 6) // Tags
            //            {
            //                string allTags = "";
            //                for (int tagNumber = 0; tagNumber < DictionaryUnits[i].Tags.Count; tagNumber++)
            //                {
            //                    allTags += DictionaryUnits[i].Tags[tagNumber] + Environment.NewLine;
            //                    if (tagNumber == DictionaryUnits[i].Tags.Count - 1)
            //                    {
            //                        allTags = allTags.TrimEnd(Environment.NewLine.ToCharArray());
            //                    }
            //                }
            //                worksheet.Cells[i + 6, j + 1] = allTags;
            //            }
            //        }
            //    }
            //    worksheet.StandardWidth = 20;
            //}
            workbook.Worksheets[1].Delete(); // delete the first (empty) worksheet
            var SaveDialogue = new SaveFileDialog();
            SaveDialogue.FileName = tbTitle.Text;
            SaveDialogue.DefaultExt = ".xlsx";
            if (SaveDialogue.ShowDialog() == true)
            {
                workbook.SaveAs(SaveDialogue.FileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                app.Quit();
                if (chbOpenAfterExport.IsChecked == true)
                {
                    Process.Start(SaveDialogue.FileName);
                    Process.GetCurrentProcess().Close();
                }
            }
            app.Quit();
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
        }
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

        private void RefreshDictionaryList()
        {
            DictionaryUnits = GetFilteredByEverything(tbSearchUnits.Text);
            dgWordUnits.ItemsSource = DictionaryUnits;
            SortDataGrid(dgWordUnits, 0, ListSortDirection.Ascending);
        }
        private ObservableCollection<VMWordUnit> GetFilteredByEverything(string textInput)
        {
            ObservableCollection<VMWordUnit> InitialUnits = DBComm.GetVMWordUnits();

            InitialUnits = GetFilteredByText(textInput, InitialUnits);
            InitialUnits = GetFilteredByTypesOfUnit(InitialUnits);
            InitialUnits = GetFilteredByTags(InitialUnits);

            return InitialUnits;
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

                if (!searchSettings.SearchMeaning && !searchSettings.SearchSource && !searchSettings.SearchExample && !searchSettings.SearchNote)
                {
                    return listByUnit;
                }
                finalList = new ObservableCollection<VMWordUnit>(finalList.Distinct());
                return finalList;
            }
        }
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
        private ObservableCollection<VMWordUnit> GetFilteredByTags(ObservableCollection<VMWordUnit> initialUnits)
        {
            ObservableCollection<VMWordUnit> finalList = new ObservableCollection<VMWordUnit>();
            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("chbTag", container) as CheckBox;

                if (checkBox.IsChecked == true)
                {
                    foreach (var unit in initialUnits)
                    {
                        if (unit.Tags.Select(x => x.Name).Contains(checkBox.Content))
                        {
                            finalList.Add(unit);
                        }
                    }
                }
            }

            finalList = new ObservableCollection<VMWordUnit>(finalList.Intersect(initialUnits).Distinct());
            return finalList;
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
        private void chbMiscellaneous_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckOtherPartsOfSpeech();
            RefreshDictionaryList();
        }
        private void chbTags_Checked(object sender, RoutedEventArgs e)
        {
            RefreshDictionaryList();
        }
        private void chbTags_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshDictionaryList();
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
                RefreshDictionaryList();
            }
        }
        private void btnSearchUnits_Click(object sender, RoutedEventArgs e)
        {
            RefreshDictionaryList();
        }
        private void tbSearchUnits_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDictionaryList();
        }

        private void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            UIActions.OpenWindowWithAnimation(this, new SearchSettings());
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
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if (chbGroupBySources.IsChecked == true)
            {
                ExportToExcelGroupedBySource();
            }
            else
            {
                ExportToExcel();
            }
        }
    }
}
