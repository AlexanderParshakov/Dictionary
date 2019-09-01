using New_designed_Dictionary.HelperClasses;
using System;
using System.Collections.Generic;
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

namespace New_designed_Dictionary.Authentication
{
    /// <summary>
    /// Interaction logic for Authentication.xaml
    /// </summary>
    public partial class Authentication : Window
    {
        static MyOwnDictionaryContext Context = new MyOwnDictionaryContext();

        public Authentication()
        {
            InitializeComponent();
        }

        private bool ValidateLogin(string login, string password)
        {
            try
            {
                User user = Context.Users.Single(x => x.Login == login && x.Password == password);
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    MessageBox.Show(e.ToString());
                }
                return false;
            }

            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateLogin(tbLogin.Text, tbPassword.Password) == true)
            {
                MainWindow main = new MainWindow(tbLogin.Text);
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            }
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (ValidateLogin(tbLogin.Text, tbPassword.Password) == true)
                {
                    MainWindow main = new MainWindow(tbLogin.Text);
                    App.Current.MainWindow = main;
                    this.Close();
                    main.Show();
                }
            }
        }

        private void tbLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                tbPassword.Focus();
            }
        }

        private void TbPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
