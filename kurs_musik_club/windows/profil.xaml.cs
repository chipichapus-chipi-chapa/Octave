using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Media.Imaging;
using kurs_musik_club.Classes;
using Npgsql;
using kurs_musik_club.Properties;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace kurs_musik_club.windows
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int StudentsCount { get; set; }
    }

    public class EnrolledCourse
    {
        public string Name { get; set; }
        public string TeacherName { get; set; }
        public int Progress { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для profil.xaml
    /// </summary>
    public partial class profil : Window
    {
        private User currentUser;
        private int currentUserId;
        private string userRole;
        private NpgsqlConnection connection;

        public profil(int userId, string role)
        {
            InitializeComponent();
            currentUserId = userId;
            userRole = role;
            currentUser = new User { Id = userId, Role = role };
            connection = new NpgsqlConnection(Connection.connectionStr);
            
            // Скрываем все контенты сначала
            adminContent.Visibility = Visibility.Collapsed;
            teacherContents.Visibility = Visibility.Collapsed;
            clientContent.Visibility = Visibility.Collapsed;
            
            LoadUserData();

            // Показываем соответствующий контент в зависимости от роли
            switch (role)
            {
                case "1": // admin
                    adminContent.Visibility = Visibility.Visible;
                    LoadUsers();
                    break;
                case "2": // client
                    clientContent.Visibility = Visibility.Visible;
                    LoadClientData();
                    break;
                case "3": // teacher
                    teacherContents.Visibility = Visibility.Visible;
                    LoadTeacherCourses();
                    break;
                default:
                    MessageBox.Show($"Неизвестная роль: {role}");
                    break;
            }
        }

        private void LoadUserData()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT u.id_user, u.login, u.fio, ur.name_role, ur.id_role
                        FROM users u
                        JOIN user_role ur ON u.role_user = ur.id_role
                        WHERE u.id_user = @userId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userName.Text = reader["fio"].ToString();
                                userLogin.Text = reader["login"].ToString();
                                userRoleText.Text = reader["name_role"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT u.id_user, u.login, u.fio, ur.name_role as role
                        FROM users u 
                        JOIN user_role ur ON u.role_user = ur.id_role 
                        ORDER BY u.id_user";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        var users = new List<dynamic>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    id_user = reader.GetInt32(0),
                                    login = reader.GetString(1),
                                    full_name = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    role = reader.GetString(3)
                                });
                            }
                        }
                        usersDataGrid.ItemsSource = users;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUserRole_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int userId = (int)button.Tag;
            
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = "SELECT id_role, name_role FROM user_role ORDER BY id_role";
                    var roles = new List<dynamic>();
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new
                                {
                                    id_role = reader.GetInt32(0),
                                    name_role = reader.GetString(1)
                                });
                            }
                        }
                    }

                    var roleWindow = new Window
                    {
                        Title = "Изменение роли пользователя",
                        Width = 300,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Background = new SolidColorBrush(System.Windows.Media.Colors.DarkGray)
                    };

                    var stackPanel = new StackPanel { Margin = new Thickness(20) };
                    
                    var comboBox = new ComboBox
                    {
                        Margin = new Thickness(0, 10, 0, 20),
                        ItemsSource = roles,
                        DisplayMemberPath = "name_role",
                        SelectedValuePath = "id_role",
                        SelectedIndex = 0
                    };

                    var saveButton = new Button
                    {
                        Content = "Сохранить",
                        Style = (Style)FindResource("ButtonStyle"),
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    saveButton.Click += async (s, args) =>
                    {
                        try
                        {
                            using (var conn = new NpgsqlConnection(Connection.connectionStr))
                            {
                                conn.Open();
                                string updateQuery = "UPDATE users SET role_user = @roleId WHERE id_user = @userId";
                                
                                using (var command = new NpgsqlCommand(updateQuery, conn))
                                {
                                    command.Parameters.AddWithValue("@roleId", ((dynamic)comboBox.SelectedItem).id_role);
                                    command.Parameters.AddWithValue("@userId", userId);
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            MessageBox.Show("Роль пользователя успешно изменена", "Успех", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            roleWindow.Close();
                            LoadUsers();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при изменении роли: {ex.Message}", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = "Выберите новую роль:", 
                        Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                        Margin = new Thickness(0, 0, 0, 10)
                    });
                    stackPanel.Children.Add(comboBox);
                    stackPanel.Children.Add(saveButton);

                    roleWindow.Content = stackPanel;
                    roleWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ролей: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int userId = (int)button.Tag;

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить этого пользователя? Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Удаляем записи из связанных таблиц
                                string[] tables = new[]
                                {
                                    "rent_recording",
                                    "rent_course",
                                    "users"
                                };

                                foreach (var table in tables)
                                {
                                    string deleteQuery = $"DELETE FROM {table} WHERE id_user = @userId";
                                    using (NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@userId", userId);
                                        command.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                MessageBox.Show("Пользователь успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadUsers();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении пользователя: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void manageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            
            LoadUsers();
        }

        private void manageCoursesButton_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем другие сетки управления
            if (usersDataGrid != null)
            {
                usersDataGrid.Visibility = Visibility.Collapsed;
            }
            if (equipmentManagementGrid != null)
            {
                equipmentManagementGrid.Visibility = Visibility.Collapsed;
            }
            if (coursesManagementGrid != null)
            {
                coursesManagementGrid.Visibility = Visibility.Visible;
            }
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT c.id_course, c.name_course, c.duration, 
                               COALESCE(c.id_teacher, 0) as id_teacher,
                               COALESCE(e.fio, 'Преподаватель не назначен') as teacher_name,
                               COALESCE(co.cost_hour, 0) as cost_hour
                        FROM courses c
                        LEFT JOIN teachers t ON c.id_teacher = t.id_teacher
                        LEFT JOIN employees e ON t.id_employee = e.id_employee
                        LEFT JOIN cost co ON c.id_cost = co.id_cost
                        ORDER BY c.id_course";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        var courses = new List<dynamic>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new
                                {
                                    id_course = reader.GetInt32(0),
                                    name_course = reader.GetString(1),
                                    duration = reader.GetInt32(2),
                                    id_teacher = reader.GetInt32(3),
                                    teacher_name = reader.GetString(4),
                                    cost = reader.GetDecimal(5)
                                });
                            }
                        }
                        if (coursesDataGrid != null)
                        {
                            coursesDataGrid.ItemsSource = courses;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            var courseWindow = new Window
            {
                Title = "Добавление курса",
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(System.Windows.Media.Colors.DarkGray)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Поле для названия курса
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Название курса:", 
                Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            });
            var nameTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(nameTextBox);

            // Поле для длительности
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Длительность (часов):", 
                Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            });
            var durationTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(durationTextBox);

            // Поле для стоимости
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Стоимость:", 
                Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            });
            var costTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(costTextBox);

            // Выбор преподавателя
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = "Преподаватель:", 
                Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                Margin = new Thickness(0, 0, 0, 5)
            });
            var teacherComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
            stackPanel.Children.Add(teacherComboBox);

            // Загрузка списка преподавателей
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT u.id_user, e.fio  
                        FROM users u 
                        JOIN employees e ON u.id_user = e.id_user 
                        JOIN teachers t ON e.id_employee = t.id_employee
                        WHERE u.role_user = 3";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        var teachers = new List<dynamic>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                teachers.Add(new
                                {
                                    id_user = reader.GetInt32(0),
                                    name = reader.GetString(1)
                                });
                            }
                        }
                        teacherComboBox.ItemsSource = teachers;
                        teacherComboBox.DisplayMemberPath = "fio";
                        teacherComboBox.SelectedValuePath = "id_user";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка преподавателей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Кнопка сохранения
            var saveButton = new Button
            {
                Content = "Сохранить",
                Style = (Style)FindResource("ButtonStyle"),
                Margin = new Thickness(0, 20, 0, 0)
            };

            saveButton.Click += async (s, args) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(nameTextBox.Text) || 
                        string.IsNullOrWhiteSpace(durationTextBox.Text) || 
                        string.IsNullOrWhiteSpace(costTextBox.Text))
                    {
                        MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!int.TryParse(durationTextBox.Text, out int duration))
                    {
                        MessageBox.Show("Длительность должна быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!decimal.TryParse(costTextBox.Text, out decimal cost))
                    {
                        MessageBox.Show("Стоимость должна быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Создаем запись в таблице cost
                                string costQuery = @"
                                    INSERT INTO cost (cost_hour) 
                                    VALUES (@cost) 
                                    RETURNING id_cost";

                                int costId;
                                using (NpgsqlCommand command = new NpgsqlCommand(costQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@cost", cost);
                                    costId = (int)await command.ExecuteScalarAsync();
                                }

                                // Создаем запись в таблице courses
                                string courseQuery = @"
                                    INSERT INTO courses (name_course, duration, id_teacher, id_cost, status) 
                                    VALUES (@name, @duration, 
                                        (SELECT t.id_teacher 
                                         FROM teachers t 
                                         JOIN employees e ON t.id_employee = e.id_employee 
                                         WHERE e.id_user = @userId), 
                                        @costId, true)";

                                using (NpgsqlCommand command = new NpgsqlCommand(courseQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@name", nameTextBox.Text);
                                    command.Parameters.AddWithValue("@duration", duration);
                                    command.Parameters.AddWithValue("@userId", teacherComboBox.SelectedValue ?? DBNull.Value);
                                    command.Parameters.AddWithValue("@costId", costId);
                                    await command.ExecuteNonQueryAsync();
                                }

                                transaction.Commit();
                                MessageBox.Show("Курс успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                courseWindow.Close();
                                LoadCourses();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при добавлении курса: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            stackPanel.Children.Add(saveButton);
            courseWindow.Content = stackPanel;
            courseWindow.ShowDialog();
        }

        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int courseId = (int)button.Tag;

            try
            {
                // Получаем данные о курсе
                dynamic courseData = null;
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT c.name_course, c.duration, c.id_teacher, co.cost_hour
                        FROM courses c
                        LEFT JOIN cost co ON c.id_cost = co.id_cost
                        WHERE c.id_course = @courseId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@courseId", courseId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                courseData = new
                                {
                                    name_course = reader.GetString(0),
                                    duration = reader.GetInt32(1),
                                    id_teacher = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                    cost_hour = reader.GetDecimal(3)
                                };
                            }
                        }
                    }
                }

                if (courseData != null)
                {
                    var courseWindow = new Window
                    {
                        Title = "Редактирование курса",
                        Width = 400,
                        Height = 500,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Background = new SolidColorBrush(System.Windows.Media.Colors.DarkGray)
                    };

                    var stackPanel = new StackPanel { Margin = new Thickness(20) };

                    // Поле для названия курса
                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = "Название курса:", 
                        Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    var nameTextBox = new TextBox 
                    { 
                        Margin = new Thickness(0, 0, 0, 15),
                        Text = courseData.name_course
                    };
                    stackPanel.Children.Add(nameTextBox);

                    // Поле для длительности
                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = "Длительность (часов):", 
                        Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    var durationTextBox = new TextBox 
                    { 
                        Margin = new Thickness(0, 0, 0, 15),
                        Text = courseData.duration.ToString()
                    };
                    stackPanel.Children.Add(durationTextBox);

                    // Поле для стоимости
                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = "Стоимость:", 
                        Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    var costTextBox = new TextBox 
                    { 
                        Margin = new Thickness(0, 0, 0, 15),
                        Text = courseData.cost_hour.ToString()
                    };
                    stackPanel.Children.Add(costTextBox);

                    // Выбор преподавателя
                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = "Преподаватель:", 
                        Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    var teacherComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
                    stackPanel.Children.Add(teacherComboBox);

                    // Загрузка списка преподавателей
                    using (NpgsqlConnection conn = new NpgsqlConnection(Connection.connectionStr))
                    {
                        conn.Open();
                        string teachersQuery = @"
                            SELECT u.id_user, e.fio 
                            FROM users u 
                            JOIN employees e ON u.id_user = e.id_user 
                            JOIN teachers t ON e.id_employee = t.id_employee
                            WHERE u.role_user = 3";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(teachersQuery, conn))
                        {
                            var teachers = new List<dynamic>();
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    teachers.Add(new
                                    {
                                        id_user = reader.GetInt32(0),
                                        fio = reader.GetString(1)
                                    });
                                }
                            }
                            teacherComboBox.ItemsSource = teachers;
                            teacherComboBox.DisplayMemberPath = "fio";
                            teacherComboBox.SelectedValuePath = "id_user";
                            if (courseData.id_teacher > 0)
                            {
                                teacherComboBox.SelectedValue = courseData.id_teacher;
                            }
                        }
                    }

                    // Кнопка сохранения
                    var saveButton = new Button
                    {
                        Content = "Сохранить",
                        Style = (Style)FindResource("ButtonStyle"),
                        Margin = new Thickness(0, 20, 0, 0)
                    };

                    saveButton.Click += async (s, args) =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || 
                                string.IsNullOrWhiteSpace(durationTextBox.Text) || 
                                string.IsNullOrWhiteSpace(costTextBox.Text))
                            {
                                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (!int.TryParse(durationTextBox.Text, out int duration))
                            {
                                MessageBox.Show("Длительность должна быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (!decimal.TryParse(costTextBox.Text, out decimal cost))
                            {
                                MessageBox.Show("Стоимость должна быть числом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            using (NpgsqlConnection conn = new NpgsqlConnection(Connection.connectionStr))
                            {
                                conn.Open();
                                using (var transaction = conn.BeginTransaction())
                                {
                                    try
                                    {
                                        // Обновляем стоимость в таблице cost
                                        string costQuery = @"
                                            UPDATE cost 
                                            SET cost_hour = @cost 
                                            FROM courses c 
                                            WHERE c.id_cost = cost.id_cost 
                                            AND c.id_course = @courseId";

                                        using (NpgsqlCommand cmd = new NpgsqlCommand(costQuery, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@cost", cost);
                                            cmd.Parameters.AddWithValue("@courseId", courseId);
                                            await cmd.ExecuteNonQueryAsync();
                                        }

                                        // Обновляем информацию о курсе
                                        string courseQuery = @"
                                            UPDATE courses 
                                            SET name_course = @name, 
                                                duration = @duration, 
                                                id_teacher = (
                                                    SELECT t.id_teacher 
                                                    FROM teachers t 
                                                    JOIN employees e ON t.id_employee = e.id_employee 
                                                    WHERE e.id_user = @userId
                                                )
                                            WHERE id_course = @courseId";

                                        using (NpgsqlCommand cmd = new NpgsqlCommand(courseQuery, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@name", nameTextBox.Text);
                                            cmd.Parameters.AddWithValue("@duration", duration);
                                            cmd.Parameters.AddWithValue("@userId", teacherComboBox.SelectedValue ?? DBNull.Value);
                                            cmd.Parameters.AddWithValue("@courseId", courseId);
                                            await cmd.ExecuteNonQueryAsync();
                                        }

                                        transaction.Commit();
                                        MessageBox.Show("Курс успешно обновлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                        courseWindow.Close();
                                        LoadCourses();
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        throw new Exception($"Ошибка при обновлении курса: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при обновлении курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    stackPanel.Children.Add(saveButton);
                    courseWindow.Content = stackPanel;
                    courseWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int courseId = (int)button.Tag;

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить этот курс? Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        
                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Получаем id_cost для удаления
                                string getCostIdQuery = "SELECT id_cost FROM courses WHERE id_course = @courseId";
                                int costId;
                                using (NpgsqlCommand command = new NpgsqlCommand(getCostIdQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@courseId", courseId);
                                    costId = (int)command.ExecuteScalar();
                                }

                                // Удаляем запись из таблицы courses
                                string deleteCourseQuery = "DELETE FROM courses WHERE id_course = @courseId";
                                using (NpgsqlCommand command = new NpgsqlCommand(deleteCourseQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@courseId", courseId);
                                    command.ExecuteNonQuery();
                                }

                                // Удаляем запись из таблицы cost
                                string deleteCostQuery = "DELETE FROM cost WHERE id_cost = @costId";
                                using (NpgsqlCommand command = new NpgsqlCommand(deleteCostQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@costId", costId);
                                    command.ExecuteNonQuery();
                                }

                                transaction.Commit();
                                MessageBox.Show("Курс успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadCourses();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Ошибка при удалении курса: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadEquipment()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            me.id_musical_equipment,
                            tme.name_type_musical_equipment,
                            bme.brand_musical_equipment,
                            ame.availability_musical_equipment,
                            COALESCE(c.cost_hour, 0) as cost_hour,
                            COALESCE(rme.id_user, 0) as id_user
                        FROM musical_equipment me
                        JOIN type_musical_equipment tme ON me.id_type_musical_equipment = tme.id_type_musical_equipment
                        JOIN brand_musical_equipment bme ON tme.id_brand_musical_equipment = bme.id_brand_musical_equipment
                        JOIN availability_musical_equipment ame ON me.id_availability_musical_equipment = ame.id_availability_musical_equipment
                        LEFT JOIN rent_mus_eq rme ON me.id_musical_equipment = rme.id_musical_equipment
                        LEFT JOIN cost c ON rme.id_cost = c.id_cost
                        ORDER BY me.id_musical_equipment";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        var equipment = new List<dynamic>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                equipment.Add(new
                                {
                                    id_musical_equipment = reader.GetInt32(0),
                                    name_type_musical_equipment = reader.GetString(1),
                                    brand_musical_equipment = reader.GetString(2),
                                    availability_musical_equipment = reader.GetBoolean(3),
                                    cost_hour = reader.GetDecimal(4),
                                    id_user = reader.GetInt32(5)
                                });
                            }
                        }
                        equipmentDataGrid.ItemsSource = equipment;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddEquipment_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEquipmentWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadEquipment();
            }
        }

        private void EditEquipment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var equipment = button.DataContext as dynamic;
            if (equipment != null)
            {
                var editWindow = new EditEquipmentWindow(equipment.id_musical_equipment);
                if (editWindow.ShowDialog() == true)
                {
                    LoadEquipment();
                }
            }
        }

        private void DeleteEquipment_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var equipment = button.DataContext as dynamic;
            if (equipment != null)
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить это оборудование?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM musical_equipment WHERE id_musical_equipment = @id";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", equipment.id_musical_equipment);
                            cmd.ExecuteNonQuery();
                        }
                        LoadEquipment();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении оборудования: {ex.Message}");
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void EquipmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить дополнительную логику при выборе оборудования
        }

        private void ToggleEquipment_Click(object sender, RoutedEventArgs e)
        {
            // Этот метод больше не нужен, так как мы убрали кнопку показа/скрытия
        }

        private void manageEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем другие сетки управления
            if (usersDataGrid != null)
            {
                usersDataGrid.Visibility = Visibility.Collapsed;
            }
            if (coursesDataGrid != null)
            {
                coursesDataGrid.Visibility = Visibility.Collapsed;
            }
            if (equipmentDataGrid != null)
            {
                equipmentManagementGrid.Visibility = Visibility.Visible;
            }
            
            // Загружаем данные об оборудовании
            LoadEquipment();
        }

        private void LoadClientData()
        {
            LoadClientCourses();
            LoadRentedHalls();
            LoadRentedEquipment();
        }

        private void LoadRentedHalls()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        SELECT 
                            hr.id_rental,
                            h.hall_name,
                            hr.rental_date,
                            hr.start_time,
                            hr.end_time,
                            hr.rental_purpose
                        FROM hall_rentals hr
                        JOIN halls h ON hr.id_hall = h.id_hall
                        WHERE hr.id_client = @id_user
                        AND hr.rental_status = 'active'
                        ORDER BY hr.rental_date DESC", connection);

                    command.Parameters.AddWithValue("@id_user", currentUserId);
                    var reader = command.ExecuteReader();
                    var halls = new List<dynamic>();
                    while (reader.Read())
                    {
                        halls.Add(new
                        {
                            id_rental = reader.GetInt32(0),
                            name_hall = reader.GetString(1),
                            rent_date = reader.GetDateTime(2),
                            start_time = reader.GetTimeSpan(3),
                            end_time = reader.GetTimeSpan(4),
                            purpose = reader.IsDBNull(5) ? "" : reader.GetString(5)
                        });
                    }
                    rentedHallsList.ItemsSource = halls;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке арендованных залов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRentedEquipment()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        SELECT 
                            rme.id_rent_mus_eq,
                            tme.name_type_musical_equipment as equipment_name,
                            bme.brand_musical_equipment as type_name,
                            CASE 
                                WHEN ame.availability_musical_equipment = true THEN 'Доступно'
                                ELSE 'Недоступно'
                            END as condition_name
                        FROM rent_mus_eq rme
                        JOIN musical_equipment me ON rme.id_musical_equipment = me.id_musical_equipment
                        JOIN type_musical_equipment tme ON me.id_type_musical_equipment = tme.id_type_musical_equipment
                        JOIN brand_musical_equipment bme ON tme.id_brand_musical_equipment = bme.id_brand_musical_equipment
                        JOIN availability_musical_equipment ame ON me.id_availability_musical_equipment = ame.id_availability_musical_equipment
                        WHERE rme.id_user = @id_user", connection);

                    command.Parameters.AddWithValue("@id_user", currentUserId);
                    var reader = command.ExecuteReader();
                    var equipment = new List<dynamic>();
                    while (reader.Read())
                    {
                        equipment.Add(new
                        {
                            id_rent_mus_eq = reader.GetInt32(0),
                            equipment_name = reader.GetString(1),
                            type_name = reader.GetString(2),
                            condition_name = reader.GetString(3)
                        });
                    }
                    rentedEquipmentList.ItemsSource = equipment;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке арендованного оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientCourses()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        SELECT 
                            rc.id_rent_cor,
                            c.name_course,
                            e.fio as teacher_name,
                            rc.rent_date
                        FROM rent_course rc
                        JOIN courses c ON rc.id_course = c.id_course
                        JOIN teachers t ON c.id_teacher = t.id_teacher
                        JOIN employees e ON t.id_employee = e.id_employee
                        WHERE rc.id_user = @id_user", connection);

                    command.Parameters.AddWithValue("@id_user", currentUserId);
                    var reader = command.ExecuteReader();
                    var courses = new List<dynamic>();
                    while (reader.Read())
                    {
                        courses.Add(new
                        {
                            id_rent_course = reader.GetInt32(0),
                            name_course = reader.GetString(1),
                            teacher_name = reader.GetString(2),
                            rent_date = reader.GetDateTime(3)
                        });
                    }
                    clientCoursesList.ItemsSource = courses;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnenrollCourse_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag == null) return;

            int rentCourseId = (int)button.Tag;

            var result = MessageBox.Show(
                "Вы уверены, что хотите отменить запись на этот курс?",
                "Подтверждение отмены",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        string query = "DELETE FROM rent_course WHERE id_rent_cor = @rentCourseId AND id_user = @userId";
                        
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@rentCourseId", rentCourseId);
                            command.Parameters.AddWithValue("@userId", currentUserId);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("Запись на курс успешно отменена", "Успех", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadClientCourses();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отмене записи на курс: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelRent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag != null)
                {
                    int rentalId = Convert.ToInt32(button.Tag);
                    var result = MessageBox.Show(
                        "Вы уверены, что хотите отменить аренду зала?", 
                        "Подтверждение", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var connection = new NpgsqlConnection(Connection.connectionStr))
                        {
                            connection.Open();
                            string query = "UPDATE hall_rentals SET rental_status = 'cancelled' WHERE id_rental = @rentalId AND id_client = @userId";
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@rentalId", rentalId);
                                command.Parameters.AddWithValue("@userId", currentUserId);
                                command.ExecuteNonQuery();
                            }
                        }
                        LoadRentedHalls();
                        MessageBox.Show(
                            "Аренда зала успешно отменена", 
                            "Успех",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при отмене аренды зала: {ex.Message}", 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadTeacherCourses()
        {
            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            c.id_course,
                            c.name_course,
                            c.duration,
                            COALESCE(c.description, 'Описание отсутствует') as description,
                            (SELECT COUNT(*) FROM rent_course rc WHERE rc.id_course = c.id_course) as students_count,
                            COALESCE(co.cost_hour, 0) as cost_hour
                        FROM courses c
                        LEFT JOIN cost co ON c.id_cost = co.id_cost
                        WHERE c.id_teacher = (
                            SELECT t.id_teacher 
                            FROM teachers t 
                            JOIN employees e ON t.id_employee = e.id_employee 
                            WHERE e.id_user = @userId
                        )
                        ORDER BY c.name_course ASC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        var courses = new List<dynamic>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                courses.Add(new
                                {
                                    id_course = reader.GetInt32(0),
                                    name_course = reader.GetString(1),
                                    duration = reader.GetInt32(2),
                                    description = reader.GetString(3),
                                    students_count = reader.GetInt32(4),
                                    cost_hour = reader.GetDecimal(5)
                                });
                            }
                        }

                        if (courses.Count == 0)
                        {
                            MessageBox.Show(
                                "У вас пока нет назначенных курсов.",
                                "Информация",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                        }

                        teacherCoursesList.ItemsSource = courses;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке курсов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void CancelEquipmentRent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag != null)
                {
                    int rentId = Convert.ToInt32(button.Tag);
                    var result = MessageBox.Show(
                        "Вы уверены, что хотите отменить аренду оборудования?", 
                        "Подтверждение", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        using (var connection = new NpgsqlConnection(Connection.connectionStr))
                        {
                            connection.Open();
                            string query = "DELETE FROM rent_mus_eq WHERE id_rent_mus_eq = @rentId AND id_user = @userId";
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@rentId", rentId);
                                command.Parameters.AddWithValue("@userId", currentUserId);
                                command.ExecuteNonQuery();
                            }
                        }
                        LoadRentedEquipment();
                        MessageBox.Show(
                            "Аренда оборудования успешно отменена", 
                            "Успех",
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при отмене аренды оборудования: {ex.Message}", 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
            }
        }

        private void RentEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var rentWindow = new RentEquipmentWindow(currentUserId);
            if (rentWindow.ShowDialog() == true)
            {
                LoadRentedEquipment();
            }
        }

        public class RentedHall
        {
            public int id_rent_recording { get; set; }
            public string name_hall { get; set; }
            public string date_rent { get; set; }
            public string time_rent { get; set; }
            public decimal cost { get; set; }
            public string description { get; set; }
            public int capacity { get; set; }
        }

        public class RentedEquipment
        {
            public int id_rent_mus_eq { get; set; }
            public string equipment_name { get; set; }
            public string type_name { get; set; }
            public string condition_name { get; set; }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new Window
                {
                    Title = "Редактирование профиля",
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 26, 26))
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                // Поле для ФИО
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = "ФИО:", 
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 0, 0, 5)
                });
                var nameTextBox = new TextBox 
                { 
                    Margin = new Thickness(0, 0, 0, 15),
                    Text = userName.Text
                };
                stackPanel.Children.Add(nameTextBox);

                // Поле для логина
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = "Логин:", 
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(0, 0, 0, 5)
                });
                var loginTextBox = new TextBox 
                { 
                    Margin = new Thickness(0, 0, 0, 15),
                    Text = userLogin.Text
                };
                stackPanel.Children.Add(loginTextBox);

                // Кнопка сохранения
                var saveButton = new Button
                {
                    Content = "Сохранить",
                    Style = (Style)FindResource("ButtonStyle"),
                    Margin = new Thickness(0, 20, 0, 0)
                };

                saveButton.Click += async (s, args) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(nameTextBox.Text) || 
                            string.IsNullOrWhiteSpace(loginTextBox.Text))
                        {
                            MessageBox.Show(
                                "Пожалуйста, заполните все обязательные поля", 
                                "Ошибка", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error
                            );
                            return;
                        }

                        using (var connection = new NpgsqlConnection(Connection.connectionStr))
                        {
                            connection.Open();
                            string query = @"
                                UPDATE users 
                                SET fio = @fio, 
                                    login = @login 
                                WHERE id_user = @userId";
                            
                            using (var command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@fio", nameTextBox.Text);
                                command.Parameters.AddWithValue("@login", loginTextBox.Text);
                                command.Parameters.AddWithValue("@userId", currentUserId);

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        MessageBox.Show(
                            "Профиль успешно обновлен", 
                            "Успех", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information
                        );
                        editWindow.Close();
                        LoadUserData(); // Перезагружаем данные пользователя
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при обновлении профиля: {ex.Message}", 
                            "Ошибка", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error
                        );
                    }
                };

                stackPanel.Children.Add(saveButton);
                editWindow.Content = stackPanel;
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии окна редактирования: {ex.Message}", 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error
                );
            }
        }

        private void TeacherCoursesList_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null && dataGrid.ItemsSource != null)
            {
                var view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                if (view != null)
                {
                    view.SortDescriptions.Clear();
                    var direction = e.Column.SortDirection == ListSortDirection.Ascending ? 
                        ListSortDirection.Descending : ListSortDirection.Ascending;
                    e.Column.SortDirection = direction;
                    view.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, direction));
                    e.Handled = true;
                }
            }
        }
    }
}
