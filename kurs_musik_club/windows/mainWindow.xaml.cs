using System;
using System.Windows;
using System.Windows.Controls;
using kurs_musik_club.Classes;
using kurs_musik_club.Properties;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для mainWindow.xaml
    /// </summary>
    public partial class mainWindow : Window
    {
        private Classes.User currentUser;

        public mainWindow()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (Settings.Default.UserId != "0")
            {
                currentUser = new Classes.User
                {
                    Id = int.Parse(Settings.Default.UserId),
                    Login = Settings.Default.UserLogin,
                    Fio = Settings.Default.UserFio,
                    Role = Settings.Default.UserRole
                };
            }
            else
            {
                currentUser = new Classes.User
                {
                    Id = 0,
                    Login = "Гость",
                    Fio = "Не авторизован",
                    Role = "Гость"
                };
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Для доступа к профилю необходимо авторизоваться", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var profileWindow = new profil(currentUser.Id, currentUser.Role);
                profileWindow.Closed += (s, args) => this.Activate();
                profileWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_muz_curs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var coursesWindow = new Courses();
                coursesWindow.Closed += (s, args) => this.Activate();
                coursesWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void rent_halls_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rentHallsWindow = new rentHalls();
                rentHallsWindow.Closed += (s, args) => this.Activate();
                rentHallsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии аренды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_rec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var recordingWindow = new recording();
                recordingWindow.Closed += (s, args) => this.Activate();
                recordingWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
