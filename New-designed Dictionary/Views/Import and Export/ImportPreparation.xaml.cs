using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Win32;
using New_designed_Dictionary.Import_and_Export;
using New_designed_Dictionary.ViewModels;
using VDS.RDF.Query;
using Excel = Microsoft.Office.Interop.Excel;

namespace New_designed_Dictionary.Import_and_Export
{
    /// <summary>
    /// Interaction logic for ImportPreparation.xaml
    /// </summary>
    public partial class ImportPreparation : Window
    {
        static string Filepath = "";
        public bool ImportDone { get; set; }

        private ObservableCollection<VMWordUnit> GetDictionaryUnitsToImport()
        {
            ObservableCollection<VMWordUnit> DictionaryUnits = new ObservableCollection<VMWordUnit>();

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(Filepath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = xlRange.Rows.Count;
            for (int i = 1; i <= rowCount; i++)
            {
                List<string> allSources = new List<string>();
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

                //foreach (SparqlResult sr in (SparqlResultSet)OntologyProcessor.GetIndividualQueryResults(New_designed_Dictionary.Resources.Queries.Query_Indiv_Sources))
                //{
                //    foreach (var variable in sr.ToList())
                //    {
                //        allSources.Add(variable.Value.ToString().Replace(New_designed_Dictionary.Resources.Paths.Ontology_Base, "").Replace("_", " ").Replace("'", ""));
                //    }
                //}
                wu.AllSources = allSources;
                DictionaryUnits.Add(wu);
            }

            return DictionaryUnits;
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
            //if (wu.SourceName == null)
            //{
            //    wu.SourceName = "";
            //}

            // fetching graphs to the knowledge base
            //OntologyProcessor.UpdateGraph(wu.ContentOfUnit.Replace(" ", "_"), "HasMeaning", wu.Meaning.Replace(" ", "_"), "IsMeaningOf");
            //OntologyProcessor.UpdateGraph(wu.ContentOfUnit.Replace(" ", "_"), "HasExample", wu.Example.Replace(" ", "_"), "Exemplifies");
            //OntologyProcessor.UpdateGraph(wu.ContentOfUnit.Replace(" ", "_"), "HasNote", wu.Note.Replace(" ", "_"), "IsNoteOf");
            //OntologyProcessor.UpdateGraph(wu.ContentOfUnit.Replace(" ", "_"), "IsProvidedBy", wu.SourceName, "Provides");
            //OntologyProcessor.AddIndividual(wu.ContentOfUnit.Replace(" ", "_"), "DictionaryUnit");

            //foreach (var item in wu.PartsOfSpeech)
            //{
            //    OntologyProcessor.AddGraph(wu.ContentOfUnit.Replace(" ", "_"), "IsCategorizedBy", item.Replace(" ", "_"), "CategorizesWord");
            //}
            //foreach (var item in wu.Tags)
            //{
            //    OntologyProcessor.AddGraph(wu.ContentOfUnit, "IsAppliedIn", item.Replace(" ", "_"), "AppliesUnit");
            //}
        }

        public ImportPreparation()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                lbFilename.ToolTip = dlg.FileName;
                Filepath = dlg.FileName;
                string filename = dlg.SafeFileName;
                lbFilename.Text = "Filename: " + filename;
                btnOK.IsEnabled = true;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (chbPreEdit.IsChecked == true)
            {
                PreEditAfterImport editAfterImport = new PreEditAfterImport(Filepath);
                this.Hide();
                editAfterImport.ShowDialog();
                if (editAfterImport.ImportDone == true)
                {
                    this.ImportDone = true;
                }
                this.Close();
            }
            else
            {
                ObservableCollection<VMWordUnit> dictionaryUnits = GetDictionaryUnitsToImport();
                foreach(var item in dictionaryUnits)
                {
                    AddUnit(item);
                }
            }
            this.Close();
        }
    }
}
