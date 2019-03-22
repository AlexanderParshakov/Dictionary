using New_designed_Dictionary.Customize_Interface;
using New_designed_Dictionary.MessageClasses;
using New_designed_Dictionary.ViewModels;
using Syn.WordNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using Word2vec.Tools;

namespace New_designed_Dictionary
{
    /// <summary>
    /// Interaction logic for AddWordUnit.xaml
    /// </summary>
    
    public partial class AddWordUnit : Window
    {
        /// Constants
        const string OntologyURI = "http://www.semanticweb.org/alexander/ontologies/2018/11/untitled-ontology-5";
        const int VectorLimit = 7;
        /// Constants
        static MyOwnDictionaryContext Context = new MyOwnDictionaryContext();

        private void AnimateUp()
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1,
                                   (Duration)TimeSpan.FromSeconds(2));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        private void AnimateDown()
        {
            DoubleAnimation animation = new DoubleAnimation(1, 0,
                                   (Duration)TimeSpan.FromSeconds(1.5));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
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

        private List<Tag> GetSpheresOfUsage()
        {
            List<Tag> SpheresOfUsage = new List<Tag>();
            SpheresOfUsage = Context.Tags.ToList();
            SpheresOfUsage.Remove(SpheresOfUsage[0]);
            return SpheresOfUsage;
        }
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
        
        private bool ValidateWord(string word)
        {
            List<string> words = OntologyProcessor.GetIndividuals(New_designed_Dictionary.Resources.Queries.Query_Indiv_WordUnit);
            if (word == New_designed_Dictionary.Resources.Literals.Placeholder_Contents)
            {
                return false;
            }
            foreach (var item in words)
            {
                if (item == word)
                {
                    return false;
                }
            }
            return true;
        }
        private void DefaultInputs()
        {
            tbContentOfUnit.Text = New_designed_Dictionary.Resources.Literals.Placeholder_Contents;
            tbMeaning.Text = New_designed_Dictionary.Resources.Literals.Placeholder_Meaning;
            tbExample.Text = New_designed_Dictionary.Resources.Literals.Placeholder_Example;
            tbNote.Text = New_designed_Dictionary.Resources.Literals.Placeholder_Note;

            cbSources.SelectedIndex = -1;
            cbPartsOfSpeech.SelectedIndex = -1;

            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                checkBox.IsChecked = false;
            }
        }
        
        private void AddUnit(string word, string meaning, string example, string note, string source, string partOfSpeech, ItemCollection items)
        {
            OntologyProcessor.UpdateGraph(word, "HasMeaning", meaning, "IsMeaningOf");
            OntologyProcessor.UpdateGraph(word, "HasExample", example, "Exemplifies");
            OntologyProcessor.UpdateGraph(word, "HasNote", note, "IsNoteOf");
            OntologyProcessor.UpdateGraph(word, "IsProvidedBy", source, "Provides");
            OntologyProcessor.UpdateGraph(word, "IsCategorizedBy", partOfSpeech, "IsMeaningOf");
            OntologyProcessor.AddIndividual(word, "WordUnit");
            foreach (var item in items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    OntologyProcessor.AddGraph(word, "IsAppliedIn", checkBox.Content.ToString().Replace(" ", "_"), "AppliesUnit");
                }
            }
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
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        
        /// START Ontology

        private void GetGraph()
        {
            Graph g = new Graph();

            IUriNode dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.semanticweb.org/alexander/ontologies/2018/11/Dictionary#SoilDecription"));
            IUriNode says = g.CreateUriNode(UriFactory.Create("http://www.semanticweb.org/alexander/ontologies/2018/11/Dictionary#HasSynonymWithPriority4"));
            ILiteralNode helloWorld = g.CreateLiteralNode("Hello World");
            ILiteralNode bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr");

            
            foreach (Triple t in g.Triples)
            {
                string subj = t.Subject.ToString();
                string obj = t.Object.ToString();
                string pred = t.Predicate.ToString();

            }



            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, @"D:\Folders\Учеба в НИУ ВШЭ - Пермь\3 курс\Курсовая - Разработка пользовательского словаря\Dictionary.owl");
            g.Assert(new Triple(dotNetRDF, says, helloWorld));
            g.Assert(new Triple(dotNetRDF, says, bonjourMonde));
            RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();


            IUriNode select = g.CreateUriNode(new Uri("http://www.semanticweb.org/alexander/ontologies/2018/11/untitled-ontology-5#HumanDescription"));
            IUriNode rdfType = g.CreateUriNode("rdf:type");
            IUriNode AdjectiveClass = g.CreateUriNode(new Uri(New_designed_Dictionary.Resources.Paths.Ontology_Adjective_Class));
            IEnumerable<Triple> ts = g.GetTriples(AdjectiveClass);
            ts = g.GetTriplesWithObject(AdjectiveClass);
            foreach (Triple t in ts)
            {
                if (t.Object.ToString().Contains("#"))
                {
                    string str = t.Subject.ToString().Split('#')[t.Subject.ToString().Split('#').Length - 1].Replace("Description", "");
                }
                else
                {
                    string str = t.Object.ToString();
                }
            }
            //rdfxmlwriter.Save(g, @"D:\Folders\Учеба в НИУ ВШЭ - Пермь\3 курс\Курсовая - Разработка пользовательского словаря\Dictionary.owl");

            //foreach (IUriNode u in g.Nodes.UriNodes())
            //{
            //    //Write the URI to the Console
            //    string str = u.Uri.ToString();
            //}
        }

        /// END Ontology


        

        private void Initiar()
        {
            cbSources.ItemsSource = OntologyProcessor.GetIndividuals(New_designed_Dictionary.Resources.Queries.Query_Indiv_Sources);
            cbPartsOfSpeech.ItemsSource = PartsOfSpeechToString(OntologyProcessor.GetPartsOfSpeech());
            itemsControlTags.ItemsSource = OntologyProcessor.GetClassTags(OntologyProcessor.GetIndividuals(New_designed_Dictionary.Resources.Queries.Query_Indiv_Tags), false);
        }
        
        private List<Tag> GetSpheresOfUsage (List<string> sous)
        {
            List<Tag> spheresOfUsage = new List<Tag>();

            foreach(var item in sous)
            {
                spheresOfUsage.Add(new Tag { Name = item });
            }

            return spheresOfUsage;
        }
        private List<string> PartsOfSpeechToString(List<PartsOfSpeech> parts)
        {
            List<string> stringSpheres = new List<string>();
            foreach(var item in parts)
            {
                if (!item.Name.Contains("auto"))
                {
                    stringSpheres.Add(item.Name.Replace("Class", ""));
                }
            }
            stringSpheres.Add("Sentences");
            return stringSpheres;
        }
        
        public AddWordUnit()

        {
            InitializeComponent();
            Initiar();
            GetGraph();
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1,
                                   (Duration)TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void gridAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void lvSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //lbSelectedSource.Text = ((VMSource)lvSources.SelectedItem).Name;
            //lbSelectedSource.Visibility = Visibility.Visible;

            //CustomView.ChangeCheckVisibility(lvSources, "iconCheckedSource");
        }

        private void lvSources_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            ScrollViewer scrollviewer = FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
            if (e.Delta > 0)
            {
                scrollviewer.LineLeft();
            }
            else
            { 
                scrollviewer.LineRight();
                }
            e.Handled = true;
        }

       

        private void lvSpheres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //lbSelectedSphere.Text = "sds";
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateWord(tbContentOfUnit.Text) == true)
            {
                AddUnit(
                    tbContentOfUnit.Text, 
                    tbMeaning.Text, 
                    tbExample.Text, 
                    tbNote.Text,
                    cbSources.SelectedValue.ToString(), 
                    cbPartsOfSpeech.SelectedValue.ToString(), 
                    itemsControlTags.Items);

                DefaultInputs();
                MyShortNotification.Show();
            }
        }

        #region Working with focusing

        private void tbContentOfUnit_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbContentOfUnit, New_designed_Dictionary.Resources.Literals.Placeholder_Contents);
        }
        private void tbContentOfUnit_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbContentOfUnit, New_designed_Dictionary.Resources.Literals.Placeholder_Contents);
        }

        private void tbMeaning_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbMeaning, New_designed_Dictionary.Resources.Literals.Placeholder_Meaning);
        }
        private void tbMeaning_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbMeaning, New_designed_Dictionary.Resources.Literals.Placeholder_Meaning);
        }

        private void tbExample_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbExample, New_designed_Dictionary.Resources.Literals.Placeholder_Example);
        }
        private void tbExample_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbExample, New_designed_Dictionary.Resources.Literals.Placeholder_Example);
        }

        private void tbNote_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbNote, New_designed_Dictionary.Resources.Literals.Placeholder_Note);
        }
        private void tbNote_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbNote, New_designed_Dictionary.Resources.Literals.Placeholder_Note);
        }
        #endregion
    }
}
