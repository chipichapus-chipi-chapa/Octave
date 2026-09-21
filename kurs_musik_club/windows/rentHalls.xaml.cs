using System;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using kurs_musik_club.Classes;
using Npgsql;
using kurs_musik_club.Properties;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для rentHalls.xaml
    /// </summary>
    public partial class rentHalls : Window
    {
        private dynamic selectedHall;
        private DateTime selectedDate;
        private string selectedTime;
        private List<DateTime> bookedDates;

        public rentHalls()
        {
            InitializeComponent();
            InitializeDatabase();
            InitializeWindow();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();

                    // Создание таблицы halls
                    string createHallsTable = @"
                        CREATE TABLE IF NOT EXISTS public.halls (
                            id_hall SERIAL PRIMARY KEY,
                            hall_name VARCHAR(100) NOT NULL,
                            description TEXT,
                            image_path TEXT
                        );";
                    
                    // Создание таблицы cost
                    string createCostTable = @"
                        CREATE TABLE IF NOT EXISTS public.cost (
                            id_cost SERIAL PRIMARY KEY,
                            price NUMERIC(10,2) NOT NULL
                        );";

                    // Создание таблицы hall_rentals
                    string createRentalsTable = @"
                        CREATE TABLE IF NOT EXISTS public.hall_rentals (
                            id_rental SERIAL PRIMARY KEY,
                            id_hall INTEGER REFERENCES public.halls(id_hall),
                            rental_date DATE NOT NULL,
                            start_time TIME NOT NULL,
                            end_time TIME NOT NULL,
                            id_client INTEGER,
                            rental_purpose VARCHAR(255),
                            rental_status VARCHAR(50) DEFAULT 'active',
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";

                    // Выполнение создания таблиц
                    using (NpgsqlCommand command = new NpgsqlCommand(createHallsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(createCostTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(createRentalsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Проверка наличия данных в таблице halls
                    string checkHalls = "SELECT COUNT(*) FROM halls";
                    using (NpgsqlCommand command = new NpgsqlCommand(checkHalls, connection))
                    {
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        if (count == 0)
                        {
                            // Заполнение таблицы halls
                            string insertHalls = @"
                                INSERT INTO halls (hall_name, description, image_path) VALUES
                                ('Большой зал', 'Просторный зал для репетиций и выступлений', 'images/halls/big_hall.jpg'),
                                ('Малый зал', 'Уютный зал для индивидуальных занятий', 'images/halls/small_hall.jpg'),
                                ('Студия звукозаписи', 'Профессиональная студия для записи музыки', 'images/halls/studio.jpg');";
                            using (NpgsqlCommand command2 = new NpgsqlCommand(insertHalls, connection))
                            {
                                command2.ExecuteNonQuery();
                            }
                            MessageBox.Show("Данные залов успешно добавлены в базу данных");
                        }
                        else
                        {
                            // Проверяем существующие данные
                            string checkData = "SELECT hall_name, image_path FROM halls";
                            using (NpgsqlCommand command2 = new NpgsqlCommand(checkData, connection))
                            {
                                using (var reader = command2.ExecuteReader())
                                {
                                    /*while (reader.Read())
                                    {
                                        MessageBox.Show($"Зал: {reader["hall_name"]}, путь к изображению: {reader["image_path"]}");
                                    }*/
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeWindow()
        {
            LoadHalls();

            // Initialize time slots
            for (int hour = 9; hour <= 21; hour++)
            {
                cmbTime.Items.Add($"{hour}:00");
            }
            cmbTime.SelectedIndex = 0;
            selectedTime = cmbTime.SelectedItem.ToString();

            // Set minimum date to today
            dpDate.DisplayDateStart = DateTime.Today;
            dpDate.DisplayDateEnd = DateTime.Today.AddMonths(3); // Можно бронировать на 3 месяца вперед
        }

        private void LoadHalls()
        {
            try
            {
                List<dynamic> hallsList = new List<dynamic>();
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = "SELECT id_hall, hall_name, description, image_path FROM halls";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var hall = new
                                {
                                    id_hall = reader["id_hall"],
                                    hall_name = reader["hall_name"],
                                    description = reader["description"],
                                    image_path = reader["image_path"]
                                };
                                hallsList.Add(hall);
                                // Отладочная информация
                               // MessageBox.Show($"Загружен зал: {hall.hall_name}, путь к изображению: {hall.image_path}");
                            }
                        }
                    }
                }
                cmbHalls.ItemsSource = hallsList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка залов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbHalls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHalls.SelectedItem != null)
            {
                selectedHall = cmbHalls.SelectedItem;
                txtHallName.Text = selectedHall.hall_name;
                txtHallDescription.Text = selectedHall.description;
                txtHallCapacity.Text = "Вместимость: 50 человек"; // Временное значение
                
                // Отладочная информация
               // MessageBox.Show($"Выбран зал: {selectedHall.hall_name}, путь к изображению: {selectedHall.image_path}");
                
                if (selectedHall.image_path != null && !string.IsNullOrEmpty(selectedHall.image_path.ToString()))
                {
                    try
                    {
                        string imagePath = selectedHall.image_path.ToString();
                        // Преобразуем относительный путь в абсолютный
                        string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath.TrimStart('/'));
                        
                        // Отладочная информация
                       // MessageBox.Show($"Полный путь к изображению: {fullPath}");
                        
                        if (System.IO.File.Exists(fullPath))
                        {
                            imgHall.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
                            MessageBox.Show("Изображение успешно загружено");
                        }
                        else
                        {
                          //  MessageBox.Show($"Файл не найден по пути: {fullPath}");
                            // Если файл не найден, показываем заглушку
                            imgHall.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/images/halls/default_hall.jpg"));
                        }
                    }
                    catch (Exception ex)
                    {
                      //  MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        // Показываем заглушку в случае ошибки
                        imgHall.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/images/halls/default_hall.jpg"));
                    }
                }
                else
                {
                    MessageBox.Show("Путь к изображению пуст или null");
                }

                // Загружаем занятые даты для выбранного зала
                LoadBookedDates();
            }
        }

        private void LoadBookedDates()
        {
            if (selectedHall != null)
            {
                bookedDates = HallRentalManager.GetBookedDates(
                    selectedHall.id_hall,
                    DateTime.Today,
                    DateTime.Today.AddMonths(3)
                );

                // Обновляем стиль календаря для отображения занятых дат
                UpdateCalendarStyle();
            }
        }

        private void UpdateCalendarStyle()
        {
            if (bookedDates != null)
            {
                foreach (DateTime date in bookedDates)
                {
                    CalendarDayButton button = GetCalendarDayButton(date);
                    if (button != null)
                    {
                        button.Background = Brushes.LightGray;
                        button.IsEnabled = false;
                    }
                }
            }
        }

        private CalendarDayButton GetCalendarDayButton(DateTime date)
        {
            // Находим кнопку календаря для указанной даты
            var calendar = dpDate.Template.FindName("PART_Calendar", dpDate) as Calendar;
            if (calendar != null)
            {
                var grid = calendar.Template.FindName("PART_CalendarItemsGrid", calendar) as Grid;
                if (grid != null)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is CalendarDayButton button)
                        {
                            if (button.DataContext is DateTime buttonDate && buttonDate.Date == date.Date)
                            {
                                return button;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void cmbTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTime.SelectedItem != null)
            {
                selectedTime = cmbTime.SelectedItem.ToString();
                CheckTimeAvailability();
            }
        }

        private void CheckTimeAvailability()
        {
            if (selectedHall != null && dpDate.SelectedDate.HasValue && !string.IsNullOrEmpty(selectedTime))
            {
                TimeSpan startTime = TimeSpan.Parse(selectedTime);
                TimeSpan endTime = startTime.Add(TimeSpan.FromHours(1));

                bool isAvailable = HallRentalManager.IsHallAvailable(
                    selectedHall.id_hall,
                    dpDate.SelectedDate.Value,
                    startTime,
                    endTime
                );

                // Обновляем UI в зависимости от доступности
                btnBook.IsEnabled = isAvailable;
                if (!isAvailable)
                {
                    MessageBox.Show("Выбранное время уже занято. Пожалуйста, выберите другое время.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            if (cmbHalls.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите зал для аренды", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату аренды", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedTime))
            {
                MessageBox.Show("Пожалуйста, выберите время аренды", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем, что пользователь авторизован
                if (string.IsNullOrEmpty(Settings.Default.UserId) || Settings.Default.UserId == "0")
                {
                    MessageBox.Show("Для аренды зала необходимо авторизоваться", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Преобразуем ID пользователя в число
                if (!int.TryParse(Settings.Default.UserId, out int userId))
                {
                    MessageBox.Show("Ошибка при определении ID пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                TimeSpan startTime = TimeSpan.Parse(selectedTime);
                TimeSpan endTime = startTime.Add(TimeSpan.FromHours(1));

                // Создаем бронирование
                if (HallRentalManager.CreateRental(
                    selectedHall.id_hall,
                    dpDate.SelectedDate.Value,
                    startTime,
                    endTime,
                    userId,
                    "Аренда зала"
                ))
                {
                    MessageBox.Show("Аренда зала успешно оформлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении аренды: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpDate.SelectedDate.HasValue)
            {
                selectedDate = dpDate.SelectedDate.Value;
                CheckTimeAvailability();
            }
        }
    }
}

