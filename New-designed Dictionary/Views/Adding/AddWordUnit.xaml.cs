using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.MessageClasses;
using New_designed_Dictionary.Modals;
using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace New_designed_Dictionary
{
    /// <summary>
    /// Interaction logic for AddWordUnit.xaml
    /// </summary>

    public partial class AddWordUnit : Window
    {
        public event Action<WordUnit> Unit;
        private void AnimateOpacity(double from, double to, double timespan)
        {
            DoubleAnimation animation = new DoubleAnimation(from, to,
                                   (Duration)TimeSpan.FromSeconds(timespan));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        private void CheckSearchInput(TextBox tb, string input)
        {
            if (tb.Text == input)
            {
                tb.Text = "";
            }
            if (tb.Text.Length == 0 && tb.IsFocused == false)
            {
                tb.Text = input;
            }
        }
        private bool ValidateWord(string word)
        {
            List<string> words = DBComm.GetVMWordUnits().ToList().Select(x => x.ContentOfUnit).ToList();
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


            foreach (var item in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                checkBox.IsChecked = false;
            }
            foreach (var item in itemControlTypesOfUnit.Items)
            {
                var container = itemControlTypesOfUnit.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemControlTypesOfUnit.ItemTemplate.FindName("TypeOfUnit", container) as CheckBox;
                checkBox.IsChecked = false;
            }
        }
        private void AddUnit(string word, string meaning, string example, string note, object selectedSource, ItemCollection types, ItemCollection tags)
        {
            // clearing placeholders if textboxes received no input
            if (meaning == New_designed_Dictionary.Resources.Literals.Placeholder_Meaning)
            {
                meaning = "";
            }
            if (example == New_designed_Dictionary.Resources.Literals.Placeholder_Example)
            {
                example = "";
            }
            if (note == New_designed_Dictionary.Resources.Literals.Placeholder_Note)
            {
                note = "";
            }

            WordUnit Wu = new WordUnit { ContentOfUnit = word, Meaning = meaning, Example = example, Note = note };
            if (selectedSource == null)
            {
                Wu.Sources.Add(DBComm.Context.Sources.SingleOrDefault(s => s.Name == "Non-specified"));
            }
            else
            {
                Wu.Sources.Add((Source)selectedSource);
            }
            bool atLeastOneTypeExists = false;
            bool atLeastOneTagExists = false;

            foreach (var item in types)
            {
                var container = itemControlTypesOfUnit.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemControlTypesOfUnit.ItemTemplate.FindName("TypeOfUnit", container) as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    atLeastOneTypeExists = true;
                    Wu.UnitTypes.Add((UnitType)item);
                }
            }
            if (atLeastOneTypeExists == false)
            {
                Wu.UnitTypes.Add(DBComm.Context.UnitTypes.SingleOrDefault(s => s.Name == "Miscellaneous"));
            }

            foreach (var item in tags)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    atLeastOneTagExists = true;
                    Wu.Tags.Add((Tag)item);
                }
            }
            if (atLeastOneTagExists == false)
            {
                Wu.Tags.Add(DBComm.Context.Tags.SingleOrDefault(s => s.Name == "Non-specified"));
            }
            Wu.Users.Add(DBComm.Context.Users.SingleOrDefault(s => s.Login == DBComm.GlobalUser.Login));
            DBComm.AddWordUnit(Wu);
            Unit(Wu);
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

        private void Initiar()
        {
            DefineItemSource();
        }
        private void DefineItemSource()
        {
            cbSources.ItemsSource = DBComm.GetSources();

            cbLanguages.ItemsSource = DBComm.GetVMLanguages();
            Language lang = DBComm.Context.Languages.SingleOrDefault(l => l.Id == DBComm.GlobalUser.LastUsedLanguage);
            VMLanguage vmLang = DBComm.FromLanguageToVMLanguage(lang);
            cbLanguages.SelectedValue = vmLang.Id;

            itemControlTypesOfUnit.ItemsSource = DBComm.GetUnitTypes();
            itemsControlTags.ItemsSource = DBComm.GetTags(false);
            cbSources.SelectedItem = CurrentFields.CurrentSource;
        }
        public AddWordUnit()
        {
            InitializeComponent();
            Initiar();
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
            try
            {
                DragMove();
            }
            catch (Exception) { }
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
                    cbSources.SelectedValue,
                    itemControlTypesOfUnit.Items,
                    itemsControlTags.Items);

                DefaultInputs();
                MyShortNotification.Show();
            }
            else
            {
                MessageBox.Show("Such a unit exists already.");
            }
        }

        #region Working with focusing
        private void tbContentOfUnit_GotFocus(object sender, RoutedEventArgs e)
        {
            tbContentOfUnit.SelectionStart = tbContentOfUnit.Text.Length; // add some logic if length is 0
            tbContentOfUnit.SelectionLength = 0;
            CheckSearchInput(tbContentOfUnit, New_designed_Dictionary.Resources.Literals.Placeholder_Contents);
        }
        private void tbContentOfUnit_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbContentOfUnit, New_designed_Dictionary.Resources.Literals.Placeholder_Contents);
        }
        private void tbMeaning_GotFocus(object sender, RoutedEventArgs e)
        {
            tbMeaning.SelectionStart = tbMeaning.Text.Length; // add some logic if length is 0
            tbMeaning.SelectionLength = 0;
            CheckSearchInput(tbMeaning, New_designed_Dictionary.Resources.Literals.Placeholder_Meaning);
        }
        private void tbMeaning_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbMeaning, New_designed_Dictionary.Resources.Literals.Placeholder_Meaning);
        }
        private void tbExample_GotFocus(object sender, RoutedEventArgs e)
        {
            tbExample.SelectionStart = tbExample.Text.Length; // add some logic if length is 0
            tbExample.SelectionLength = 0;
            CheckSearchInput(tbExample, New_designed_Dictionary.Resources.Literals.Placeholder_Example);
        }
        private void tbExample_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbExample, New_designed_Dictionary.Resources.Literals.Placeholder_Example);
        }
        private void tbNote_GotFocus(object sender, RoutedEventArgs e)
        {
            tbNote.SelectionStart = tbNote.Text.Length; // add some logic if length is 0
            tbNote.SelectionLength = 0;
            CheckSearchInput(tbNote, New_designed_Dictionary.Resources.Literals.Placeholder_Note);
        }
        private void tbNote_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckSearchInput(tbNote, New_designed_Dictionary.Resources.Literals.Placeholder_Note);
        }
        #endregion

        private void btnAddTag_Click(object sender, RoutedEventArgs e)
        {
            AddTag addWindow = new AddTag();
            AnimateOpacity(1, 0.5, 1.5);
            var checkedTags = GetCheckedTags();
            addWindow.ShowDialog();
            itemsControlTags.ItemsSource = DBComm.GetTags(false);
            CheckTags(checkedTags);
            Show();
            AnimateOpacity(0.5, 1, 0.5);
        }

        private List<Tag> GetCheckedTags()
        {
            List<Tag> tags = new List<Tag>();
            for (int i = 0; i < itemsControlTags.Items.Count; i++)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(itemsControlTags.Items[i]) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    var item = itemsControlTags.ItemContainerGenerator.ContainerFromIndex(i);
                    var chb = (FrameworkElement)item;
                    tags.Add((Tag)chb.DataContext);
                }
            }
            return tags;
        }
        private void CheckTags(List<Tag> checkedTags)
        {
            foreach (var checkedTag in checkedTags)
            {
                for (int j = 0; j < itemsControlTags.Items.Count; j++)
                {
                    var tag = itemsControlTags.ItemContainerGenerator.ContainerFromIndex(j);
                    var chb = (FrameworkElement)tag;
                    if (checkedTag.Id == ((Tag)chb.DataContext).Id)
                    {
                        var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(itemsControlTags.Items[j]) as FrameworkElement;
                        container.ApplyTemplate();
                        var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                        checkBox.IsChecked = true;
                        break;
                    }
                }
            }
        }

        private void ItemsControlTags_MouseEnter(object sender, MouseEventArgs e)
        {
            rNote.Height = new GridLength(0);
            rExample.Height = new GridLength(0);
        }

        private void ItemsControlTags_MouseLeave(object sender, MouseEventArgs e)
        {
            rNote.Height = new GridLength(100);
            rExample.Height = new GridLength(100);
        }
    }
}
