using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;
using kurs_musik_club.Properties;

namespace kurs_musik_club.windows
{
    public partial class RecordingBookingWindow : Window
    {
        private dynamic selectedService;
        private DateTime selectedDate;
        private string selectedTime;
        private string selectedDuration;

        public RecordingBookingWindow(dynamic service)
        {
            InitializeComponent();
            selectedService = service;
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            serviceName.Text = selectedService.name_rec;
            serviceDetails.Text = $"Длительность: {selectedService.recording_duration} | Стоимость: {selectedService.price} руб";

            // Initialize time slots
            for (int hour = 9; hour <= 21; hour++)
            {
                timeComboBox.Items.Add($"{hour}:00");
            }
            timeComboBox.SelectedIndex = 0;
            selectedTime = timeComboBox.SelectedItem.ToString();

            // Initialize duration options
            durationComboBox.Items.Add("1 час");
            durationComboBox.Items.Add("2 часа");
            durationComboBox.Items.Add("3 часа");
            durationComboBox.SelectedIndex = 0;
            selectedDuration = durationComboBox.SelectedItem.ToString();

            // Set minimum date to today
            bookingCalendar.DisplayDateStart = DateTime.Today;
        }

        private void timeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (timeComboBox.SelectedItem != null)
            {
                selectedTime = timeComboBox.SelectedItem.ToString();
            }
        }

        private void durationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedDuration = durationComboBox.SelectedItem.ToString();
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!bookingCalendar.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату бронирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedTime))
            {
                MessageBox.Show("Пожалуйста, выберите время бронирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedDuration))
            {
                MessageBox.Show("Пожалуйста, выберите длительность бронирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, что пользователь авторизован
                if (string.IsNullOrEmpty(Settings.Default.UserId) || Settings.Default.UserId == "0")
                {
                    MessageBox.Show("Для бронирования необходимо авторизоваться", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Преобразуем ID пользователя в число
                if (!int.TryParse(Settings.Default.UserId, out int userId))
                {
                    MessageBox.Show("Ошибка при определении ID пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    
                    // Получаем id_recording_and_mixing и id_cost из таблицы recording_and_mixing
                    string getServiceQuery = @"
                        SELECT id_recording_and_mixing, price 
                        FROM recording_and_mixing 
                        WHERE name_rec = @serviceName";
                    
                    int recordingId = 0;
                    int costId = 0;
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(getServiceQuery, connection))
                    {
                        command.Parameters.AddWithValue("@serviceName", selectedService.name_rec);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                recordingId = reader.GetInt32(0);
                                costId = reader.GetInt32(1);
                            }
                            else
                            {
                                MessageBox.Show("Не удалось найти информацию об услуге", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    // Создаем запись о бронировании
                    string insertQuery = @"
                        INSERT INTO rent_recording (id_cost, id_recording_and_mixing, rent_date, id_user) 
                        VALUES (@costId, @recordingId, @rentalDate, @userId)";
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@costId", costId);
                        command.Parameters.AddWithValue("@recordingId", recordingId);
                        command.Parameters.AddWithValue("@rentalDate", selectedDate);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Бронирование успешно создано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bookingCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bookingCalendar.SelectedDate.HasValue)
            {
                selectedDate = bookingCalendar.SelectedDate.Value;
            }
        }
    }
}