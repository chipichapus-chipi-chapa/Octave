using System;
using System.Collections.Generic;
using System.Windows;
using Npgsql;
using kurs_musik_club.model;
using kurs_musik_club.Classes;

namespace kurs_musik_club.windows
{
    public partial class AddCourseWindow : Window
    {
        private User currentUser;

        public AddCourseWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            try
            {
                MessageBox.Show("Начало загрузки преподавателей");
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT t.id_teacher, e.fio
                        FROM teachers t
                        JOIN employees e ON t.id_employee = e.id_employee
                        ORDER BY e.fio";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            var teachers = new List<dynamic>();
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                teachers.Add(new
                                {
                                    id_teacher = reader.GetInt32(0),
                                    fio = reader.GetString(1)
                                });
                            }
                            MessageBox.Show($"Загружено преподавателей: {count}");
                            teacherComboBox.ItemsSource = teachers;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке преподавателей: {ex.Message}");
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(courseName.Text) ||
                string.IsNullOrWhiteSpace(courseDescription.Text) ||
                string.IsNullOrWhiteSpace(coursePrice.Text) ||
                teacherComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля и выберите преподавателя");
                return;
            }

            if (!decimal.TryParse(coursePrice.Text, out decimal price))
            {
                MessageBox.Show("Пожалуйста, введите корректную цену");
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO courses (name_course, description, price, id_teacher) 
                        VALUES (@name, @description, @price, @teacher_id)";
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        var selectedTeacher = (dynamic)teacherComboBox.SelectedItem;
                        command.Parameters.AddWithValue("@name", courseName.Text);
                        command.Parameters.AddWithValue("@description", courseDescription.Text);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@teacher_id", selectedTeacher.id_teacher);
                        
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Курс успешно добавлен");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении курса: {ex.Message}");
            }
        }

        private void teacherComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
} 