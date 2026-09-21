using System;
using System.Windows;
using System.Windows.Media.Imaging;
using kurs_musik_club.Classes;
using kurs_musik_club.model;
using Npgsql;
using System.Configuration;
using System.Data.SqlClient;

namespace kurs_musik_club.windows
{
    public partial class CourseDetailsWindow : Window
    {
        private string connectionString;
        private courses selectedCourse;

        public CourseDetailsWindow(courses course)
        {
            InitializeComponent();
            NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr);
            selectedCourse = course;
            LoadCourseDetails();
        }

        private void LoadCourseDetails()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"SELECT c.id_course, c.id_teacher, c.duration, 
                                          c.name_course, COALESCE(c.description, 'Описание отсутствует') as description,
                                          COALESCE(e.fio, 'Преподаватель не назначен') as teacher_name,
                                          e.photo_path
                                   FROM courses c
                                   LEFT JOIN teachers t ON c.id_teacher = t.id_teacher
                                   LEFT JOIN employees e ON t.id_employee = e.id_employee
                                   WHERE c.id_course = @id_course";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_course", selectedCourse.id_course);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                courseName.Text = reader["name_course"].ToString();
                                teacherName.Text = $"Преподаватель: {reader["teacher_name"].ToString()}";
                                courseDescription.Text = reader["description"].ToString();

                                if (!reader.IsDBNull(reader.GetOrdinal("photo_path")))
                                {
                                    string photoPath = reader["photo_path"].ToString();
                                    if (!string.IsNullOrEmpty(photoPath))
                                    {
                                        teacherImage.Source = new BitmapImage(new Uri(photoPath, UriKind.RelativeOrAbsolute));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке деталей курса: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}