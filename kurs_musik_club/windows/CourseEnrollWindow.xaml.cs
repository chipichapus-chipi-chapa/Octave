using System;
using System.Windows;
using System.Windows.Controls;
using kurs_musik_club.Classes;
using Npgsql;
using kurs_musik_club.Properties;

namespace kurs_musik_club.windows
{
    public partial class CourseEnrollWindow : Window
    {
        private dynamic course;
        private DateTime selectedStartDate;
        private string selectedTime;
        private string selectedLessonsPerWeek;

        public CourseEnrollWindow(dynamic course)
        {
            InitializeComponent();
            this.course = course;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            try
            {
                // Отображаем информацию о курсе
                courseName.Text = course.name_course;
                courseDetails.Text = $"Описание: {course.description}";

                // Initialize time slots
                for (int hour = 9; hour <= 21; hour++)
                {
                    timeComboBox.Items.Add($"{hour}:00");
                }
                timeComboBox.SelectedIndex = 0;
                selectedTime = timeComboBox.SelectedItem.ToString();

                // Initialize lessons per week options
                lessonsPerWeekComboBox.Items.Add("1 раз в неделю");
                lessonsPerWeekComboBox.Items.Add("2 раза в неделю");
                lessonsPerWeekComboBox.Items.Add("3 раза в неделю");
                lessonsPerWeekComboBox.SelectedIndex = 0;
                selectedLessonsPerWeek = lessonsPerWeekComboBox.SelectedItem.ToString();

                // Set minimum date to today
                startDateCalendar.DisplayDateStart = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void startDateCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (startDateCalendar.SelectedDate.HasValue)
            {
                selectedStartDate = startDateCalendar.SelectedDate.Value;
            }
        }

        private void timeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeComboBox.SelectedItem != null)
            {
                selectedTime = timeComboBox.SelectedItem.ToString();
            }
        }

        private void lessonsPerWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lessonsPerWeekComboBox.SelectedItem != null)
            {
                selectedLessonsPerWeek = lessonsPerWeekComboBox.SelectedItem.ToString();
            }
        }

        private void EnrollButton_Click(object sender, RoutedEventArgs e)
        {
            if (!startDateCalendar.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату начала курса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedTime))
            {
                MessageBox.Show("Пожалуйста, выберите время занятий", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();

                    // Get user ID from settings
                    int userId = int.Parse(Settings.Default.UserId);

                    // Get course cost ID
                    int costId;
                    using (var costCommand = new NpgsqlCommand(
                        "SELECT id_cost FROM courses WHERE id_course = @courseId",
                        connection))
                    {
                        costCommand.Parameters.AddWithValue("@courseId", course.id_course);
                        
                        var result = costCommand.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Для данного курса не установлена стоимость. Пожалуйста, обратитесь к администратору.", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        costId = Convert.ToInt32(result);
                    }

                    // Insert enrollment record
                    using (var command = new NpgsqlCommand(
                        "INSERT INTO rent_course (id_user, id_course, id_cost, rent_date) VALUES (@userId, @courseId, @costId, @startDate)",
                        connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@courseId", course.id_course);
                        command.Parameters.AddWithValue("@costId", costId);
                        command.Parameters.AddWithValue("@startDate", selectedStartDate);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Вы успешно записаны на курс!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи на курс: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}