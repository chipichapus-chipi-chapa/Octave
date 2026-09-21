using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using kurs_musik_club.Classes;
using kurs_musik_club.Properties;

namespace kurs_musik_club.windows
{
    public partial class ProfileWindow : Window
    {
        private User currentUser;
        private List<string> recentActions;

        public ProfileWindow()
        {
            InitializeComponent();
            LoadUserData();
            LoadRoleSpecificContent();
        }

        private void LoadUserData()
        {
            try
            {
                if (Settings.Default.UserId != "0")
                {
                    currentUser = new User
                    {
                        Id = int.Parse(Settings.Default.UserId),
                        Login = Settings.Default.UserLogin,
                        Fio = Settings.Default.UserFio,
                        Role = Settings.Default.UserRole
                    };

                    // Обновляем UI
                    userName.Text = currentUser.Fio;
                    userLogin.Text = currentUser.Login;
                    userRole.Text = GetRoleName(currentUser.Role);
                    FioTextBlock.Text = currentUser.Fio;
                    LoginTextBlock.Text = currentUser.Login;
                    RoleTextBlock.Text = GetRoleName(currentUser.Role);
                }
                else
                {
                    MessageBox.Show("Пользователь не авторизован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private string GetRoleName(string role)
        {
            switch (role.ToLower())
            {
                case "teacher":
                    return "Преподаватель";
                case "client":
                    return "Клиент";
                case "admin":
                    return "Администратор";
                default:
                    return role;
            }
        }

        private void LoadRoleSpecificContent()
        {
            if (currentUser.Role.ToLower() == "admin")
            {
                adminButtons.Visibility = Visibility.Visible;
                adminSection.Visibility = Visibility.Visible;
                LoadAdminStatistics();
                LoadRecentActions();
            }
        }

        private void LoadAdminStatistics()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();

                    // Получаем количество пользователей
                    string usersQuery = "SELECT COUNT(*) FROM users";
                    using (NpgsqlCommand command = new NpgsqlCommand(usersQuery, connection))
                    {
                        int totalUsers = Convert.ToInt32(command.ExecuteScalar());
                        totalUsersText.Text = $"Всего пользователей: {totalUsers}";
                    }

                    // Получаем количество курсов
                    string coursesQuery = "SELECT COUNT(*) FROM courses";
                    using (NpgsqlCommand command = new NpgsqlCommand(coursesQuery, connection))
                    {
                        int totalCourses = Convert.ToInt32(command.ExecuteScalar());
                        totalCoursesText.Text = $"Всего курсов: {totalCourses}";
                    }

                    // Получаем количество бронирований
                    string bookingsQuery = "SELECT COUNT(*) FROM rent_course";
                    using (NpgsqlCommand command = new NpgsqlCommand(bookingsQuery, connection))
                    {
                        int totalBookings = Convert.ToInt32(command.ExecuteScalar());
                        totalBookingsText.Text = $"Всего бронирований: {totalBookings}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статистики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentActions()
        {
            recentActions = new List<string>();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT u.fio, rc.rent_date, c.name_course
                        FROM rent_course rc
                        JOIN users u ON rc.id_user = u.id
                        JOIN courses c ON rc.id_course = c.id_course
                        ORDER BY rc.rent_date DESC
                        LIMIT 5";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string action = $"{reader.GetString(0)} записался на курс {reader.GetString(2)} ({reader.GetDateTime(1).ToString("dd.MM.yyyy")})";
                                recentActions.Add(action);
                            }
                        }
                    }
                }
                recentActionsList.ItemsSource = recentActions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке последних действий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void editProfileButton_Click(object sender, RoutedEventArgs e)
        {
            EditProfileWindow editWindow = new EditProfileWindow(currentUser);
            editWindow.ShowDialog();
            LoadUserData();
        }

        private void manageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            ManageUsersWindow manageUsersWindow = new ManageUsersWindow();
            manageUsersWindow.ShowDialog();
            LoadAdminStatistics();
            LoadRecentActions();
        }

        private void manageCoursesButton_Click(object sender, RoutedEventArgs e)
        {
            ManageCoursesWindow manageCoursesWindow = new ManageCoursesWindow();
            manageCoursesWindow.ShowDialog();
            LoadAdminStatistics();
            LoadRecentActions();
        }

        private void manageBookingsButton_Click(object sender, RoutedEventArgs e)
        {
            ManageBookingsWindow manageBookingsWindow = new ManageBookingsWindow();
            manageBookingsWindow.ShowDialog();
            LoadAdminStatistics();
            LoadRecentActions();
        }

        private void manageHallsButton_Click(object sender, RoutedEventArgs e)
        {
            ManageHallsWindow manageHallsWindow = new ManageHallsWindow();
            manageHallsWindow.ShowDialog();
            LoadAdminStatistics();
            LoadRecentActions();
        }

        private void reportsButton_Click(object sender, RoutedEventArgs e)
        {
            ReportsWindow reportsWindow = new ReportsWindow();
            reportsWindow.ShowDialog();
        }
    }
} 