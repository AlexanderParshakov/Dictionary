using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace New_designed_Dictionary.Modals
{
    /// <summary>
    /// Interaction logic for EditDictionaryUnit.xaml
    /// </summary>
    public partial class EditDictionaryUnit : Window
    {
        public event Action<VMWordUnit> Unit;
        static VMWordUnit Wu;
        static bool ChangingRealUnit = true;

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
        private void SetFields(VMWordUnit wu)
        {
            if (wu.ContentOfUnit != "")
            {
                tbContentOfUnit.Text = wu.ContentOfUnit;
            }
            if (wu.Meaning != "")
            {
                tbMeaning.Text = wu.Meaning;
            }
            if (wu.Example != "")
            {
                tbExample.Text = wu.Example;
            }
            if (wu.ContentOfUnit != "")
            {
                tbNote.Text = wu.Note;
            }

        }
        public EditDictionaryUnit(VMWordUnit wu, bool changingRealUnit)
        {
            InitializeComponent();
            Initiar(wu, changingRealUnit);
        }


        private void Initiar(VMWordUnit wu, bool changingRealUnit)
        {
            ChangingRealUnit = changingRealUnit;
            SetFields(wu);
            DefineItemSource(wu);
            Wu = wu;
        }
        private void DefineItemSource(VMWordUnit VMwu)
        {
            cbSources.ItemsSource = DBComm.GetSources();
            if (VMwu.SourceId == 0)
            {
                VMwu.SourceId = DBComm.Context.Sources.SingleOrDefault(s => s.Name == "Non-specified").Id;
            }
            cbSources.SelectedValue = VMwu.SourceId;
            cbSources.SelectedValuePath = "Id";

            itemControlTypesOfUnit.ItemsSource = DBComm.GetUnitTypes();
            itemsControlTags.ItemsSource = DBComm.GetTags(false);
        }


        private void gridAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception) { }
        }

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
            tbContentOfUnit.SelectionLength = 0;
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Wu.ContentOfUnit = tbContentOfUnit.Text;
            Wu.Meaning = tbMeaning.Text;
            Wu.Example = tbExample.Text;
            Wu.Note = tbNote.Text;
            if (cbSources.SelectedValue != null)
            {
                Wu.SourceId = ((Source)cbSources.SelectedItem).Id;
                Wu.Sources.Clear();
                Wu.Sources.Add((Source)cbSources.SelectedItem);
            }

            bool atLeastOneTypeExists = false;
            bool atLeastOneTagExists = false;
            Wu.UnitTypes.Clear();
            Wu.Tags.Clear();
            foreach (var item in itemControlTypesOfUnit.Items)
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

            foreach (var item in itemsControlTags.Items)
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
                Wu.Tags.Add(DBComm.Context.Tags.SingleOrDefault(t => t.Name == "Non-specified"));
            }

            if (Wu.ContentOfUnit == New_designed_Dictionary.Resources.Literals.Placeholder_Contents || Wu.ContentOfUnit == null)
            {
                Wu.ContentOfUnit = "";
            }
            if (Wu.Meaning == New_designed_Dictionary.Resources.Literals.Placeholder_Meaning || Wu.Meaning == null)
            {
                Wu.Meaning = "";
            }
            if (Wu.Example == New_designed_Dictionary.Resources.Literals.Placeholder_Example || Wu.Example == null)
            {
                Wu.Example = "";
            }
            if (Wu.Note == New_designed_Dictionary.Resources.Literals.Placeholder_Note || Wu.Note == null)
            {
                Wu.Note = "";
            }
            if (ChangingRealUnit)
            {
                DBComm.UpdateWordUnit(Wu);
            }
            Unit(Wu);

            this.Close();
        }

        private void itemControlTypesOfUnit_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var type in itemControlTypesOfUnit.Items)
            {
                var container = itemControlTypesOfUnit.ItemContainerGenerator.ContainerFromItem(type) as FrameworkElement;
                var checkBox = itemControlTypesOfUnit.ItemTemplate.FindName("TypeOfUnit", container) as CheckBox;
                if (Wu.UnitTypes != null)
                {
                    if (Wu.UnitTypes.ToList().Select(x => x.Name).ToList().Contains(checkBox.Content))
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }
        private void itemsControlTags_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var tag in itemsControlTags.Items)
            {
                var container = itemsControlTags.ItemContainerGenerator.ContainerFromItem(tag) as FrameworkElement;
                var checkBox = itemsControlTags.ItemTemplate.FindName("Tag", container) as CheckBox;
                if (Wu.Tags != null)
                {
                    if (Wu.Tags.ToList().Select(x => x.Name).ToList().Contains(checkBox.Content))
                    {
                        checkBox.IsChecked = true;
                    }
                }
            }
        }

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
