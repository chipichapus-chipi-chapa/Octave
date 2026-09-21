using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using kurs_musik_club.Classes;

namespace kurs_musik_club.Classes
{
    public class HallRentalManager
    {
        /// <summary>
        /// Проверяет, доступен ли зал на указанную дату и время
        /// </summary>
        public static bool IsHallAvailable(int hallId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT COUNT(*) FROM hall_rentals
                        WHERE id_hall = @hallId
                        AND rental_date = @date
                        AND rental_status = 'active'
                        AND (
                            (start_time <= @startTime AND end_time > @startTime)
                            OR (start_time < @endTime AND end_time >= @endTime)
                            OR (start_time >= @startTime AND end_time <= @endTime)
                        )";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hallId", hallId);
                        command.Parameters.AddWithValue("@date", date.Date);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при проверке доступности зала: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Получает список занятых дат для указанного зала
        /// </summary>
        public static List<DateTime> GetBookedDates(int hallId, DateTime startDate, DateTime endDate)
        {
            List<DateTime> bookedDates = new List<DateTime>();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT DISTINCT rental_date FROM hall_rentals
                        WHERE id_hall = @hallId
                        AND rental_date BETWEEN @startDate AND @endDate
                        AND rental_status = 'active'";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hallId", hallId);
                        command.Parameters.AddWithValue("@startDate", startDate.Date);
                        command.Parameters.AddWithValue("@endDate", endDate.Date);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bookedDates.Add(reader.GetDateTime(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении занятых дат: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            return bookedDates;
        }

        /// <summary>
        /// Создает новое бронирование зала
        /// </summary>
        public static bool CreateRental(int hallId, DateTime date, TimeSpan startTime, TimeSpan endTime, int userId, string purpose)
        {
            try
            {
                // Проверяем доступность зала
                if (!IsHallAvailable(hallId, date, startTime, endTime))
                {
                    System.Windows.MessageBox.Show("Зал уже забронирован на это время", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }

                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO hall_rentals (id_hall, rental_date, start_time, end_time, id_client, rental_purpose)
                        VALUES (@hallId, @date, @startTime, @endTime, @userId, @purpose)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hallId", hallId);
                        command.Parameters.AddWithValue("@date", date.Date);
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@purpose", purpose ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании бронирования: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Получает список занятых временных слотов для указанной даты и зала
        /// </summary>
        public static List<TimeSlot> GetBookedTimeSlots(int hallId, DateTime date)
        {
            List<TimeSlot> bookedSlots = new List<TimeSlot>();
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT start_time, end_time FROM hall_rentals
                        WHERE id_hall = @hallId
                        AND rental_date = @date
                        AND rental_status = 'active'
                        ORDER BY start_time";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hallId", hallId);
                        command.Parameters.AddWithValue("@date", date.Date);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TimeSpan startTime = reader.GetTimeSpan(0);
                                TimeSpan endTime = reader.GetTimeSpan(1);
                                bookedSlots.Add(new TimeSlot { StartTime = startTime, EndTime = endTime });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении занятых временных слотов: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            return bookedSlots;
        }
    }

    public class TimeSlot
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
} 