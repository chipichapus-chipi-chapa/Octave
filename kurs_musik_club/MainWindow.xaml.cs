using kurs_musik_club.windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kurs_musik_club
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void sign_in_btn_Click(object sender, RoutedEventArgs e)
        {
           sing_in sing_in = new sing_in();
            sing_in.Show();

        }

        private void registration_btn_Click(object sender, RoutedEventArgs e)
        {
            registration registration = new registration();
            registration.Show();
        }

        private void no_registration_label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow mainWindow = new mainWindow();
            mainWindow.Show();
        }
    }
}
