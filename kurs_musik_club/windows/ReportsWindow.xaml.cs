using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using kurs_musik_club.Classes;

namespace kurs_musik_club.windows
{
    public partial class ReportsWindow : Window
    {
        private GridView gridView;

        public ReportsWindow()
        {
            InitializeComponent();
            gridView = (GridView)reportListView.View;
            cmbReportType.SelectedIndex = 0;
        }

        private void cmbReportType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbReportType.SelectedItem != null)
            {
                gridView.Columns.Clear();
                switch (cmbReportType.SelectedIndex)
                {
                    case 0: // Courses Report
                        gridView.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new System.Windows.Data.Binding("Id") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Название", DisplayMemberBinding = new System.Windows.Data.Binding("Name") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Преподаватель", DisplayMemberBinding = new System.Windows.Data.Binding("TeacherName") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Количество студентов", DisplayMemberBinding = new System.Windows.Data.Binding("StudentsCount") });
                        break;
                    case 1: // Bookings Report
                        gridView.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new System.Windows.Data.Binding("Id") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Зал", DisplayMemberBinding = new System.Windows.Data.Binding("HallName") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Клиент", DisplayMemberBinding = new System.Windows.Data.Binding("ClientName") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Дата", DisplayMemberBinding = new System.Windows.Data.Binding("Date") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Время", DisplayMemberBinding = new System.Windows.Data.Binding("Time") });
                        break;
                    case 2: // Users Report
                        gridView.Columns.Add(new GridViewColumn { Header = "ID", DisplayMemberBinding = new System.Windows.Data.Binding("Id") });
                        gridView.Columns.Add(new GridViewColumn { Header = "ФИО", DisplayMemberBinding = new System.Windows.Data.Binding("Fio") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Логин", DisplayMemberBinding = new System.Windows.Data.Binding("Login") });
                        gridView.Columns.Add(new GridViewColumn { Header = "Роль", DisplayMemberBinding = new System.Windows.Data.Binding("Role") });
                        break;
                }
            }
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (cmbReportType.SelectedIndex)
                {
                    case 0:
                        GenerateCoursesReport();
                        break;
                    case 1:
                        GenerateBookingsReport();
                        break;
                    case 2:
                        GenerateUsersReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                statusText.Text = "Ошибка при формировании отчета";
            }
        }

        private void GenerateCoursesReport()
        {
            var courses = new List<dynamic>();
            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                string query = @"
                    SELECT c.id, c.name, u.fio as teacher_name, 
                           COUNT(ec.user_id) as students_count
                    FROM courses c
                    LEFT JOIN users u ON c.teacher_id = u.id
                    LEFT JOIN enrolled_courses ec ON c.id = ec.course_id
                    GROUP BY c.id, c.name, u.fio
                    ORDER BY c.id";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                TeacherName = reader.IsDBNull(2) ? "Не назначен" : reader.GetString(2),
                                StudentsCount = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }

            reportListView.ItemsSource = courses;
            statusText.Text = $"Сформирован отчет по курсам: {courses.Count} записей";
        }

        private void GenerateBookingsReport()
        {
            var bookings = new List<dynamic>();
            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                string query = @"
                    SELECT b.id, h.name as hall_name, u.fio as client_name, 
                           b.date, b.start_time, b.end_time
                    FROM bookings b
                    JOIN halls h ON b.hall_id = h.id
                    JOIN users u ON b.user_id = u.id
                    ORDER BY b.date DESC, b.start_time";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookings.Add(new
                            {
                                Id = reader.GetInt32(0),
                                HallName = reader.GetString(1),
                                ClientName = reader.GetString(2),
                                Date = reader.GetDateTime(3).ToString("dd.MM.yyyy"),
                                Time = $"{reader.GetTimeSpan(4).ToString(@"hh\:mm")} - {reader.GetTimeSpan(5).ToString(@"hh\:mm")}"
                            });
                        }
                    }
                }
            }

            reportListView.ItemsSource = bookings;
            statusText.Text = $"Сформирован отчет по бронированиям: {bookings.Count} записей";
        }

        private void GenerateUsersReport()
        {
            var users = new List<dynamic>();
            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                string query = @"
                    SELECT id, fio, login, 
                           CASE role_user 
                               WHEN '1' THEN 'Администратор'
                               WHEN '2' THEN 'Пользователь'
                               WHEN '3' THEN 'Преподаватель'
                           END as role
                    FROM users
                    ORDER BY id";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Fio = reader.GetString(1),
                                Login = reader.GetString(2),
                                Role = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            reportListView.ItemsSource = users;
            statusText.Text = $"Сформирован отчет по пользователям: {users.Count} записей";
        }
    }
} 