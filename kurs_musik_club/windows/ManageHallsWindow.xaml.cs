using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using kurs_musik_club.Classes;

namespace kurs_musik_club.windows
{
    public class Hall
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
    }

    public partial class ManageHallsWindow : Window
    {
        private List<Hall> halls;

        public ManageHallsWindow()
        {
            InitializeComponent();
            LoadHalls();
        }

        private void LoadHalls()
        {
            try
            {
                halls = new List<Hall>();
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = "SELECT id, name, description, capacity FROM halls ORDER BY id";
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                halls.Add(new Hall
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.GetString(2),
                                    Capacity = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }

                hallsListView.ItemsSource = halls;
                statusText.Text = $"Загружено залов: {halls.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке залов: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                statusText.Text = "Ошибка при загрузке данных";
            }
        }

        private void btnAddHall_Click(object sender, RoutedEventArgs e)
        {
           // var addHallWindow = new AddHallWindow();
           /* if (addHallWindow.ShowDialog() == true)
            {
                LoadHalls();
            }*/
        }

        private void btnEditHall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int hallId = (int)button.Tag;
                var hall = halls.Find(h => h.Id == hallId);
                if (hall != null)
                {
                   /* var editHallWindow = new AddHallWindow(hall);
                    if (editHallWindow.ShowDialog() == true)
                    {
                        LoadHalls();
                    }*/
                }
            }
        }

        private void btnDeleteHall_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int hallId = (int)button.Tag;
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этот зал?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                        {
                            connection.Open();
                            string query = "DELETE FROM halls WHERE id = @id";
                            
                            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id", hallId);
                                command.ExecuteNonQuery();
                            }
                        }

                        LoadHalls();
                        statusText.Text = "Зал успешно удален";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении зала: {ex.Message}", "Ошибка", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        statusText.Text = "Ошибка при удалении зала";
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadHalls();
        }
    }
} 