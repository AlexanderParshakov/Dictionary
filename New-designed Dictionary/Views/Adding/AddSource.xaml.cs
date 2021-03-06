﻿using Microsoft.Win32;
using New_designed_Dictionary.HelperClasses;
using New_designed_Dictionary.HelperClasses.Customize_Interface;
using System;
using System.Collections.Generic;
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

namespace New_designed_Dictionary
{
    /// <summary>
    /// Interaction logic for AddSource.xaml
    /// </summary>
    public partial class AddSource : Window
    {
        static string Destination = Directory.GetCurrentDirectory() + @"\SourcePictures";
        static string FileName = "";
        // FUNCTIONS
        private string StringWithoutStopChars(string stringToProcess)
        {
            return stringToProcess.Replace("\\", "").Replace("/", "").Replace("*", "").Replace("[", "").Replace("]", "").Replace(":", "").Replace("?", "");
        }
        private void CheckNameInput()
        {
            int Checks = 0;
            if (tbSourceName.Text == "Enter the name of the source...")
            {
                tbSourceName.Text = "";
                Checks++;
            }
            int counter = 0;
            counter = Regex.Matches(tbSourceName.Text, @"[a-zA-Z, а-яА-я]").Count;
            if (counter == 0 && Checks == 0)
            {
                tbSourceName.Text = "Enter the name of the source...";
            }
        }

        private void ChangeButtonEnabled()
        {
            try
            {
                if (tbSourceName.Text != "Enter the name of the source..." && imgSource.Source != null && tbSourceName.Text != "")
                {
                    btnAdd.IsEnabled = true;
                }
                if (tbSourceName.Text == "Enter the name of the source..." || imgSource.Source == null || tbSourceName.Text == "")
                {
                    btnAdd.IsEnabled = false;
                }
            }
            catch (Exception) { }
        }
        private void OpenAndShowSource()
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphic (*.png)|*.png";

            if (op.ShowDialog() == true)
            {
                imgSource.Source = new BitmapImage(new Uri(op.FileName));
                imgSource.Visibility = Visibility.Visible;
                lbPickAPicture.Visibility = Visibility.Collapsed;
                FileName = op.FileName;
            }
        }

        private void AddTheSource()
        {
            Source s = new Source();
            s.Name = tbSourceName.Text;
            s.Users.Add(DBComm.Context.Users.SingleOrDefault(so => so.Login == DBComm.GlobalUser.Login));
            if (DBComm.Context.Sources.SingleOrDefault(x => x.Name == s.Name) == null)
            {
                s.Picture = UIActions.GetReducedImage(File.ReadAllBytes(FileName));
                DBComm.Context.Sources.Add(s);
                DBComm.Context.SaveChanges();

                this.Close();
            }
            else
            {
                MessageBox.Show("A source with such a name already exists.");
            }
        }
        // FUNCTIONS
        public AddSource()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void gridAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void tbSourceName_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckNameInput();
        }

        private void tbSourceName_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckNameInput();
        }

        private void chbConfirm_Checked(object sender, RoutedEventArgs e)
        {
            btnAdd.IsEnabled = true;
        }

        private void chbConfirm_Unchecked(object sender, RoutedEventArgs e)
        {
            btnAdd.IsEnabled = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddTheSource();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenAndShowSource();
            ChangeButtonEnabled();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1,
                                   (Duration)TimeSpan.FromSeconds(0.5));
            this.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void tbSourceName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeButtonEnabled();
        }
    }
}
